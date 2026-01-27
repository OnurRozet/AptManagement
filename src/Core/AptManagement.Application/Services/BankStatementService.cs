using OfficeOpenXml; // EPPlus namespace
using AptManagement.Application.Interfaces;
using System.Text.RegularExpressions;
using AptManagement.Application.Dtos;
using AptManagement.Application.Common;
using AptManagement.Domain.Entities;
using AptManagement.Domain.Enums;
using AptManagement.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AptManagement.Application.Services;

public class BankStatementService(
    IRepository<Income> _incomeRepo,
    IRepository<Expense> _expenseRepo,
    IRepository<ApartmentDebt> _aptDebtRepo,
    IRepository<ExpenseCategory> _expenseCat,
    IRepository<Apartment> _apartmentRepo,
    IRepository<IncomeDebtAllocation> _allocationRepo,
    IUnitOfWork _unitOfWork) : IBankStatementService
{
    private readonly string _apartmentRegexPattern = @"(?i)(?:daire|no|d|kapı|konut)\s*[:.]?\s*(\d+)";

    // Gider Anahtar Kelimeleri
    private readonly Dictionary<string, string> _expenseKeywords = new()
    {
        { "enerjisa", "Elektrik" },
        { "ayesaş", "Elektrik" },
        { "iski", "Su" },
        { "igdaş", "Doğalgaz" },
        { "asansör", "Asansör" },
        { "temizlik", "Temizlik" },
        { "bakım", "Bakım-Onarım" },
        //{ "sigorta", "Bina Sigortası" },
        //{ "noter", "Resmi Giderler" },
        //{ "komisyon", "Banka Masrafı" },
        //{ "para çekme", "Nakit Para Çıkışı" },
        { "işlem komisyonu", "Komisyon Giderleri" },
        { "bsmv", "Vergi Giderleri" },
        { "havale komisyonu", "Komisyon Giderleri" },
        { "atm para çekme", "Nakit Para Çıkışı" },
    };

    public async Task<List<BankTransactionDto>> ParseExcelFile(Stream fileStream)
    {
        var transactions = new List<BankTransactionDto>();
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using (var package = new ExcelPackage(fileStream))
        {
            // Ziraat Excel'i genelde tek sayfa olur, ilkini alıyoruz.
            var worksheet = package.Workbook.Worksheets[0];

            // Satır sayısını bul
            int rowCount = worksheet.Dimension?.Rows ?? 0;
            if (rowCount == 0) return transactions;

            // --- BAŞLIK SATIRINI BULMA MANTIĞI ---
            // Ziraat dosyalarında ilk 10-15 satır boş veya bilgi notudur.
            // "Tarih" kelimesinin olduğu satırı bulup veri okumaya oradan başlayacağız.
            int startRow = 1;
            bool headerFound = false;

            for (int row = 1; row <= 20; row++) // İlk 20 satırı tara
            {
                var col1 = worksheet.Cells[row, 1].Text.Trim(); // 1. Sütuna bak
                var col2 = worksheet.Cells[row, 3].Text.Trim(); // 3. Sütuna bak 
                if (col1.Equals("Tarih",StringComparison.OrdinalIgnoreCase) && col2.Equals("Açıklama",StringComparison.OrdinalIgnoreCase))
                {
                    startRow = row + 1; // Veri bir alt satırda başlar
                    headerFound = true;
                    break;
                }
            }

            if (!headerFound) return transactions; // Başlık yoksa boş dön

            var allCategories = await _expenseCat.GetAll().ToListAsync();

            // --- VERİ OKUMA DÖNGÜSÜ ---
            for (int row = startRow; row <= rowCount; row++)
            {
                // Tarih hücresini al (1. Sütun)
                // EPPlus tarihleri bazen sayı (OADate) olarak tutar, o yüzden GetValue<DateTime> en güvenlisidir.
                var dateCell = worksheet.Cells[row, 1].GetValue<DateTime?>();

                // Tarih yoksa (dosya sonu toplam satırları vb.) atla
                if (dateCell == null) continue;

                var transactionId = worksheet.Cells[row, 2].Text;      // 2. Sütun: Fiş No
                var description = worksheet.Cells[row, 3].Text;        // 3. Sütun: Açıklama
                var amountValue = worksheet.Cells[row, 4].Value;       // 4. Sütun: Tutar

                if (amountValue == null) continue;

                // Tutarı decimal'e çevir
                if (!decimal.TryParse(amountValue.ToString(), out decimal amount)) continue;

                var dto = new BankTransactionDto
                {
                    Date = dateCell.Value,
                    TransactionId = transactionId,
                    Description = description,
                    Amount = amount
                };

                // --- LOGIC (AYNI MANTIK) ---
                if (amount < 0) // Gider
                {
                    dto.TransactionType = "Gider";
                    dto.Amount = Math.Abs(amount); // Pozitif yap
                    dto.SuggestedCategory = await AnalyzeExpenseCategory(description,allCategories);
                }
                else // Gelir
                {
                    dto.TransactionType = "Gelir";
                    dto.MatchedApartmentId = ExtractApartmentId(description);
                    dto.SuggestedCategory = "Aidat";
                }

                transactions.Add(dto);
            }
        }

        return transactions;
    }

    private async Task<string> AnalyzeExpenseCategory(string description,List<ExpenseCategory> expenseCategories)
    {
        if (string.IsNullOrEmpty(description)) return "Genel Giderler";

        var descLower = description.ToLower(new System.Globalization.CultureInfo("tr-TR"));

        foreach (var keyword in _expenseKeywords)
        {
            if (descLower.Contains(keyword.Key.ToLower(new System.Globalization.CultureInfo("tr-TR"))))
            {
                return keyword.Value; // Sözlükteki karşılığı döndür (Örn: "Vergi Giderleri")
            }
        }
        var matchedCategory = expenseCategories.FirstOrDefault(c =>
        !string.IsNullOrEmpty(c.Description) &&
        descLower.Contains(c.Description.ToLower(new System.Globalization.CultureInfo("tr-TR"))));

        return matchedCategory?.Name ?? "Diğer Giderler";
    }

    private int? ExtractApartmentId(string description)
    {
        var match = Regex.Match(description, _apartmentRegexPattern);
        if (match.Success && match.Groups.Count > 1)
        {
            if (int.TryParse(match.Groups[1].Value, out int aptNo))
            {
                if (aptNo >= 1 && aptNo <= 14) return aptNo;
            }
        }
        return null;
    }

    private async Task ApplyPaymentToDebtsAsync(int incomeId, int apartmentId, decimal paymentAmount)
    {
        // 1. O dairenin açık borçlarını en eskiden yeniye çek
        var openDebts = await _aptDebtRepo.GetAll()
            .Where(d => d.ApartmentId == apartmentId && !d.IsClosed)
            .OrderBy(d => d.DueDate)
            .ThenBy(d => d.Id)
            .ToListAsync();

        if (!openDebts.Any())
        {
            // Eğer hiç borç yoksa, parayı daire bakiyesine ekleyebilirsin
            var apartment = await _apartmentRepo.GetByIdAsync(apartmentId);
            apartment.Balance += paymentAmount;
            _apartmentRepo.Update(apartment);
            return;
        }

        decimal remainingPayment = paymentAmount;

        foreach (var debt in openDebts)
        {
            if (remainingPayment <= 0) break;

            decimal remainingDebt = debt.Amount - debt.PaidAmount;

            // Bu borç için ne kadar para ayıracağız? (Paranın yettiği kadar veya borcun tamamı kadar)
            decimal amountToAllocate = Math.Min(remainingPayment, remainingDebt);

            if (amountToAllocate > 0)
            {
                // A) Borç Kaydını Güncelle
                debt.PaidAmount += amountToAllocate;
                if (debt.PaidAmount >= debt.Amount)
                {
                    debt.IsClosed = true;
                    debt.PaidAmount = debt.Amount; // Kuruşu kuruşuna eşitle
                }
                _aptDebtRepo.Update(debt);

                // B) KRİTİK: Tahsisat Kaydını At (İz Bırak!)
                var allocation = new IncomeDebtAllocation
                {
                    IncomeId = incomeId,
                    ApartmentDebtId = debt.Id,
                    AllocatedAmount = amountToAllocate
                };
                // Allocation repo'yu inject etmiş olmalısın
                await _allocationRepo.CreateAsync(allocation);

                // C) Kalan paradan düş
                remainingPayment -= amountToAllocate;
            }
        }

        // Eğer tüm borçlar bitti ama hala para arttıysa, artanı cüzdana (bakiyeye) koy
        if (remainingPayment > 0)
        {
            var apartment = await _apartmentRepo.GetByIdAsync(apartmentId);
            apartment.Balance += remainingPayment;
            _apartmentRepo.Update(apartment);
        }
    }

    public async Task<ServiceResult<bool>> ProcessBankStatementAsync(List<BankTransactionDto> transactions)
    {
        try
        {
            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                foreach (var item in transactions)
                {
                    // Zaten işlenmişse atla
                    if (item.IsProcessed) continue;

                    if (item.TransactionType == "Gelir")
                    {
                        // GELİR KAYDI
                        var income = new Income
                        {
                            Amount = item.Amount,
                            Title = item.Description.Length > 100 ? item.Description.Substring(0, 100) : item.Description,
                            IncomeDate = item.Date,
                            PaymentCategory = PaymentCategory.BankTransfer, // Enum: Banka Havalesi
                            IncomeCategoryId = 1, // Varsayılan: Aidat (Bunu dinamik yapabiliriz)
                            ApartmentId = item.MatchedApartmentId ?? 0 // Eşleşen daire yoksa 0 (Belirsiz)
                        };

                        await _incomeRepo.CreateAsync(income);

                        // Eğer daire belliyse borçtan düşme
                        if (income.ApartmentId > 0)
                        {
                            await ApplyPaymentToDebtsAsync(income.Id, income.ApartmentId, income.Amount);
                        }
                    }
                    else
                    {
                        // GİDER KAYDI
                        var expenseCategory = await _expenseCat.GetAll().FirstOrDefaultAsync(x => x.Name == item.SuggestedCategory);
                        if (expenseCategory == null) expenseCategory = new ExpenseCategory { Id = 11, Name = "Diğer Giderler" };

                        var expense = new Expense
                        {
                            Amount = item.Amount,
                            Title = item.Description.Length > 100 ? item.Description.Substring(0, 100) : item.Description,
                            ExpenseDate = item.Date,
                            PaymentCategory = PaymentCategory.BankTransfer,
                            ExpenseCategoryId = expenseCategory.Id
                        };
                        await _expenseRepo.CreateAsync(expense);
                    }
                }
            });

            return ServiceResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.Error($"Kayıt sırasında hata oluştu: {ex.Message}");
        }
    }
}