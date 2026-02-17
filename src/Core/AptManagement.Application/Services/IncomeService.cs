using AptManagement.Application.Common;
using AptManagement.Application.Common.Base.Request;
using AptManagement.Application.Common.Base.Response;
using AptManagement.Application.Dtos;
using AptManagement.Application.Dtos.Reports;
using AptManagement.Application.Extensions;
using AptManagement.Application.Interfaces;
using AptManagement.Domain.Entities;
using AptManagement.Domain.Enums;
using AptManagement.Domain.Interfaces;
using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using X.PagedList.Extensions;

namespace AptManagement.Application.Services
{
    public class IncomeService(
        IRepository<Income> repository,
        IRepository<Apartment> apartmentRepo,
        IRepository<ApartmentDebt> debtRepo,
        IRepository<IncomeDebtAllocation> allocationDebtRepo,
        IRepository<DuesSetting> duesRepo,
        IRepository<ManagementPeriod> managementPeriodRepo,
        IMapper mapper,
        IValidator<Income> validator,
        IUnitOfWork unitOfWork) : IIncomeService
    {
        public async Task<ServiceResult<CreateOrEditResponse>> CreateOrEdit(IncomeDto request)
        {
            var income = mapper.Map<Income>(request);

            // Validasyon
            var validationResult = validator.Validate(income);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                return ServiceResult<CreateOrEditResponse>.Error(errors);
            }

            return await unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                // --- GÜNCELLEME (EDIT) SENARYOSU ---
                if (income.Id > 0)
                {
                    var existingIncome = await repository.GetAll()
                        .FirstOrDefaultAsync(x => x.Id == income.Id);

                    if (existingIncome == null) return ServiceResult<CreateOrEditResponse>.Error("Kayıt bulunamadı.");

                    // 1. Eski tahsisleri geri al (yanlış daireye eklenmiş ödemeler de dahil)
                    // Böylece daire veya tutar değiştiğinde aidat takibi doğru güncellenir
                    await RevertIncomeAllocationsAsync(existingIncome);

                    // 2. Gelir kaydını güncelle (yeni daire, tutar vb.)
                    mapper.Map(request, existingIncome);
                    repository.Update(existingIncome);

                    // 3. Ödemeyi (doğru) dairenin borçlarına yeniden dağıt
                    await DistributePaymentToDebtsAsync(existingIncome.Id, existingIncome.ApartmentId, existingIncome.IncomeDate, existingIncome.Amount);

                    return ServiceResult<CreateOrEditResponse>.Success(new CreateOrEditResponse { ID = income.Id },
                        "Güncellendi. Aidat takibi yeni daireye göre düzeltildi.");
                }

                // --- YENİ KAYIT (CREATE) SENARYOSU ---
                // Gelir kaydını oluştur
                await repository
                    .CreateAsync(
                        income); // Önce ID oluşsun diye create ediyoruz (Transaction varsa rollback edilebilir)

                // Parayı al ve borçlara sırasıyla dağıt
                await DistributePaymentToDebtsAsync(income.Id, income.ApartmentId, income.IncomeDate, income.Amount);

                return ServiceResult<CreateOrEditResponse>.Success(new CreateOrEditResponse { ID = income.Id },
                    "Oluşturuldu ve borçlardan düşüldü.");
            });
        }

        public async Task<ServiceResult<bool>> DeleteIncomeAsync(int id)
        {
            return await unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var income = await repository.GetAll()
                    .Include(x => x.Allocations)
                    .ThenInclude(x => x.ApartmentDebt)
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (income == null) return ServiceResult<bool>.Error("Silinecek Gelir Kaydı Bulunamadı");

                foreach (var allocation in income.Allocations)
                {
                    var debt = allocation.ApartmentDebt;

                    debt.PaidAmount -= allocation.AllocatedAmount;

                    if (debt.IsClosed && debt.PaidAmount < debt.Amount) debt.IsClosed = false;
                    debtRepo.Update(debt);
                    allocationDebtRepo.Delete(allocation);
                }

                ////Eğer bu gelir bir borca (aidata) bağlıysa, borç durumunu geri al
                //if (income.Allocations != null)
                //{
                //    // Borcun ödenmiş tutarından, silinen gelir miktarını düş
                //    income.ApartmentDebt.PaidAmount -= income.Amount;

                //    // Eğer ödenen miktar toplam borçtan az kaldıysa IsClosed'ı false yap
                //    if (income.ApartmentDebt.PaidAmount < income.ApartmentDebt.Amount)
                //    {
                //        income.ApartmentDebt.IsClosed = false;
                //    }

                //    // Eğer borç miktarı 0'ın altına düşerse (güvenlik kontrolü) sıfıra eşitle
                //    if (income.ApartmentDebt.PaidAmount < 0)
                //        income.ApartmentDebt.PaidAmount = 0;

                //    debtRepo.Update(income.ApartmentDebt); // Entity Framework bunu otomatik takip eder zaten
                //}

                repository.Delete(income);

                return ServiceResult<bool>.Success(true);
            });
        }

        public async Task<ServiceResult<DetailResponse<IncomeResponse>>> GetIncomeById(int id)
        {
            var income = await repository.GetByIdAsync(id);

            if (income == null)
                return ServiceResult<DetailResponse<IncomeResponse>>.Error(
                    "Belirtilen id ye sahip bir gelir bulunamadı");

            var incomeResponse = mapper.Map<IncomeResponse>(income);

            return ServiceResult<DetailResponse<IncomeResponse>>.Success(new DetailResponse<IncomeResponse>
            { Detail = incomeResponse });
        }

        public ServiceResult<IncomeSummaryDto> GetSummaryIncomeReport()
        {
            DateTime date = DateTime.Now;
            int currentYear = DateTime.Now.Year;


            var totalRevenue = repository.GetAll().Sum(x => x.Amount);

            var revenueByCurrentMonth =
                repository.GetAll().Where(x => x.IncomeDate.Month == date.Month).Sum(x => x.Amount);

            var totalItemCount = repository.GetAll().Count();

            var highestRevenue = repository.GetAll()
                //.Where(x => x.IncomeDate.Year == date.Year)
                .GroupBy(x => new { x.Apartment.Label, x.Apartment.OwnerName, x.Apartment.TenantName })
                .Select(g => new TopPayerDto
                {
                    ApartmentLabel = g.Key.Label,
                    OwnerName = g.Key.OwnerName,
                    TotalAmount = g.Sum(x => x.Amount),
                    TransactionCount = g.Count()
                })
                .OrderByDescending(x => x.TotalAmount)
                .FirstOrDefault();

            var regularPayers = repository.GetAll()
                //.Where(x => x.IncomeDate.Year == currentYear)
                .GroupBy(x => new { x.Apartment.Label, x.Apartment.OwnerName })
                .Select(g => new TopPayerDto
                {
                    ApartmentLabel = g.Key.Label,
                    OwnerName = g.Key.OwnerName,
                    TotalAmount = g.Sum(x => x.Amount), // Ne kadar ödediği (Bilgi amaçlı)
                    TransactionCount = g.Count()
                })
                .OrderByDescending(x => x.TransactionCount) // Ödeme sayısına göre sırala (En çoktan aza)
                .ThenByDescending(x => x.TotalAmount) // Eşitlik varsa tutarı çok olanı üste al
                .Take(5) // İstenen sayı kadar al (Sen 2 istedin)
                .ToList();

            IncomeSummaryDto incomeSummaryDto = new()
            {
                TotalIncome = totalRevenue,
                TotalIncomeByCurrentMonth = revenueByCurrentMonth,
                TotalItemCount = totalItemCount,
                HighestApartmentFeeRevenue = highestRevenue,
                MostRegularPayer = regularPayers
            };

            return ServiceResult<IncomeSummaryDto>.Success(incomeSummaryDto);
        }

        public async Task<List<PaymentMatrixDto>> GetYearlyPaymentMatrixAsync(int year)
        {
            var currentYear = DateTime.Now.Year;
            var currentMonth = DateTime.Now.Month;

            var yearStart = new DateTime(year, 1, 1);
            var yearEnd = new DateTime(year, 12, 31);

            // Bu yılla kesişen tüm yönetici muafiyet dönemleri (aktif + geçmiş - eski yöneticiler dahil)
            var exemptPeriods = await managementPeriodRepo.GetAll()
                .Where(mp => mp.IsExemptFromDues &&
                            mp.StartDate <= yearEnd &&
                            (mp.EndDate == null || mp.EndDate >= yearStart))
                .Select(mp => new { mp.ApartmentId, mp.StartDate, mp.EndDate })
                .ToListAsync();

            var rawData = await apartmentRepo.GetAll()
                .AsNoTracking()
                .Select(apt => new
                {
                    Apt = apt,
                    YearlyDebts = apt.Debts.Where(d => d.DueDate.Year == year).ToList()
                })
                .ToListAsync();

            var dues = await duesRepo.GetAll().Where(x => x.StartDate.Year == year && x.IsActive).ToListAsync();

            // Daire bazında bu yılda muaf olunan ayları hesapla - sadece bulunduğumuz aydan öncesi (geçmiş aylar)
            static List<int> GetExemptMonths(int apartmentId, int y, int currentY, int currentM, IEnumerable<(int ApartmentId, DateTime StartDate, DateTime? EndDate)> periods)
            {
                var result = new List<int>();
                for (int m = 1; m <= 12; m++)
                {
                    // Sadece geçmiş aylar: (yıl < şimdi) veya (aynı yıl ve ay < şimdiki ay)
                    bool isPastMonth = y < currentY || (y == currentY && m < currentM);
                    if (!isPastMonth) continue;

                    var monthStart = new DateTime(y, m, 1);
                    var monthEnd = m == 2 ? new DateTime(y, 2, 28) : new DateTime(y, m, 30);
                    var inExemptPeriod = periods.Any(p =>
                        p.ApartmentId == apartmentId &&
                        p.StartDate <= monthEnd &&
                        (p.EndDate == null || p.EndDate >= monthStart));
                    if (inExemptPeriod) result.Add(m);
                }
                return result;
            }

            var periodsList = exemptPeriods.Select(p => (p.ApartmentId, p.StartDate, p.EndDate)).ToList();

            var matrix = rawData.Select(item => new PaymentMatrixDto
            {
                ApartmentId = item.Apt.Id,
                ApartmentLabel = item.Apt.Label,
                OwnerName = item.Apt.OwnerName,
                IsManager = item.Apt.IsManager,
                ExemptMonths = GetExemptMonths(item.Apt.Id, year, currentYear, currentMonth, periodsList),

                // LINQ to Objects (RAM'de çalışır, çok hızlıdır)
                Jan = item.YearlyDebts.Where(x => x.DueDate.Month == 1 && x.DebtType != DebtType.TransferFromPast).Sum(x => x.PaidAmount),
                Feb = item.YearlyDebts.Where(x => x.DueDate.Month == 2 && x.DebtType != DebtType.TransferFromPast).Sum(x => x.PaidAmount),
                Mar = item.YearlyDebts.Where(x => x.DueDate.Month == 3 && x.DebtType != DebtType.TransferFromPast).Sum(x => x.PaidAmount),
                Apr = item.YearlyDebts.Where(x => x.DueDate.Month == 4 && x.DebtType != DebtType.TransferFromPast).Sum(x => x.PaidAmount),
                May = item.YearlyDebts.Where(x => x.DueDate.Month == 5 && x.DebtType != DebtType.TransferFromPast).Sum(x => x.PaidAmount),
                Jun = item.YearlyDebts.Where(x => x.DueDate.Month == 6 && x.DebtType != DebtType.TransferFromPast).Sum(x => x.PaidAmount),
                Jul = item.YearlyDebts.Where(x => x.DueDate.Month == 7 && x.DebtType != DebtType.TransferFromPast).Sum(x => x.PaidAmount),
                Aug = item.YearlyDebts.Where(x => x.DueDate.Month == 8 && x.DebtType != DebtType.TransferFromPast).Sum(x => x.PaidAmount),
                Sep = item.YearlyDebts.Where(x => x.DueDate.Month == 9 && x.DebtType != DebtType.TransferFromPast).Sum(x => x.PaidAmount),
                Oct = item.YearlyDebts.Where(x => x.DueDate.Month == 10 && x.DebtType != DebtType.TransferFromPast).Sum(x => x.PaidAmount),
                Nov = item.YearlyDebts.Where(x => x.DueDate.Month == 11 && x.DebtType != DebtType.TransferFromPast).Sum(x => x.PaidAmount),
                Dec = item.YearlyDebts.Where(x => x.DueDate.Month == 12 && x.DebtType != DebtType.TransferFromPast).Sum(x => x.PaidAmount),

                TotalYearlyDebt = item.YearlyDebts.Where(x => x.DebtType != DebtType.TransferFromPast).Sum(x => x.Amount-x.PaidAmount),
                TotalPaid = item.YearlyDebts.Sum(x => x.PaidAmount),
                TransferredDebt = item.YearlyDebts
                        .Where(x => x.DebtType == Domain.Enums.DebtType.TransferFromPast)
                        .Sum(x => x.Amount),
                TotalDebtUntilNow = item.YearlyDebts.Where(x => x.DueDate.Month <= currentMonth).Sum(x => x.Amount),
            })
                .OrderBy(x => x.ApartmentId)
                .ToList();

            return matrix;
        }

        public async Task<ServiceResult<SearchResponse<IncomeResponse>>> Search(IncomeSearch request)
        {
            var query = repository.GetAll();
            var filteredQuery = query.WhereIf(request.IncomeId.HasValue && request.IncomeId.Value > 0,
                    x => x.Id == request.IncomeId.Value)
                .WhereIf(request.Amount.HasValue && request.Amount.Value > 0, x => x.Amount == request.Amount.Value)
                .WhereIf(!string.IsNullOrEmpty(request.Title), x => x.Title == request.Title)
                .WhereIf(request.IncomeDate.HasValue, x => x.IncomeDate.Date == request.IncomeDate.Value.Date)
                .WhereIf(request.PaymentCategory.HasValue, x => x.PaymentCategory == request.PaymentCategory.Value)
                .WhereIf(request.IncomeCategoryId.HasValue && request.IncomeCategoryId.Value > 0,
                    x => x.IncomeCategoryId == request.IncomeCategoryId.Value)
                .WhereIf(request.ApartmentId.HasValue && request.ApartmentId.Value > 0,
                    x => x.ApartmentId == request.ApartmentId.Value)
                .WhereIf(!string.IsNullOrEmpty(request.Keyword),
                    x => x.Title.Contains(request.Keyword) || x.IncomeCategory.Name.Contains(request.Keyword) ||
                         x.Apartment.OwnerName.Contains(request.Keyword))
                .Select(x => new IncomeResponse()
                {
                    Id = x.Id,
                    Amount = x.Amount,
                    Title = x.Title,
                    IncomeDate = x.IncomeDate,
                    PaymentCategory = x.PaymentCategory,
                    IncomeCategoryId = x.IncomeCategoryId,
                    ApartmentId = x.ApartmentId,
                    IncomeCategory = x.IncomeCategory.Name,
                    ApartmentNo = x.Apartment.Label + " - " +
                                  ($"{(!string.IsNullOrEmpty(x.Apartment.TenantName) ? x.Apartment.TenantName : x.Apartment.OwnerName)}")
                })
                .OrderByDescending(x => x.IncomeDate)
                .ThenByDescending(x => x.Id)
                .ToPagedList(request.Page, (int)request.PageSize);

            return ServiceResult<SearchResponse<IncomeResponse>>.Success(new SearchResponse<IncomeResponse>
            {
                SearchResult = filteredQuery.ToList(),
                TotalItemCount = query.Count()
            });
        }

        /// <summary>
        /// Gelirin eski tahsislerini geri alır (yanlış daireye eklenen ödemeleri düzeltmek için).
        /// Eski dairenin borçlarındaki PaidAmount düşürülür, fazla ödeme Balance'a gitmişse o da geri alınır.
        /// </summary>
        private async Task RevertIncomeAllocationsAsync(Income income)
        {
            var allocations = await allocationDebtRepo.GetAll(a => a.ApartmentDebt)
                .Where(a => a.IncomeId == income.Id)
                .ToListAsync();

            decimal totalAllocated = 0;

            foreach (var allocation in allocations)
            {
                var debt = allocation.ApartmentDebt;
                totalAllocated += allocation.AllocatedAmount;

                debt.PaidAmount -= allocation.AllocatedAmount;
                if (debt.PaidAmount < 0) debt.PaidAmount = 0;
                debt.IsClosed = debt.PaidAmount >= debt.Amount;
                debtRepo.Update(debt);

                allocationDebtRepo.Delete(allocation);
            }

            // Fazla ödeme (borçlara dağıtılmayan kısım) eski dairenin Balance'ına gitmişti; geri al
            decimal amountToRevertFromBalance = income.Amount - totalAllocated;
            if (amountToRevertFromBalance > 0)
            {
                var oldApartment = await apartmentRepo.GetByIdAsync(income.ApartmentId);
                if (oldApartment != null)
                {
                    oldApartment.Balance -= amountToRevertFromBalance;
                    if (oldApartment.Balance < 0) oldApartment.Balance = 0;
                    apartmentRepo.Update(oldApartment);
                }
            }
        }

        private async Task DistributePaymentToDebtsAsync(int incomeId, int apartmentId, DateTime incomeDate,
            decimal paymentAmount)
        {
            decimal remainingMoney = paymentAmount;

            // 1. Adım: Önce TransferFromPast (geçmiş dönem) borçlarını işle
            var transferFromPastDebts = await debtRepo.GetAll()
                .Where(x => x.ApartmentId == apartmentId &&
                           !x.IsClosed &&
                           x.DebtType == DebtType.TransferFromPast)
                .ToListAsync();

            // TransferFromPast borçlarını önce tamamen bitir
            if (transferFromPastDebts.Any())
            {
                foreach (var debt in transferFromPastDebts)
                {
                    if (remainingMoney <= 0) break;

                    decimal debtRequiredAmount = debt.Amount - debt.PaidAmount;
                    decimal amountToAllocate = Math.Min(remainingMoney, debtRequiredAmount);

                    // Borcu güncelle
                    debt.PaidAmount += amountToAllocate;
                    debt.IsClosed = debt.PaidAmount >= debt.Amount;
                    debtRepo.Update(debt);

                    // Tahsisat Kaydını Oluştur (Allocation)
                    var allocation = new IncomeDebtAllocation
                    {
                        IncomeId = incomeId,
                        ApartmentDebtId = debt.Id,
                        AllocatedAmount = amountToAllocate
                    };
                    await allocationDebtRepo.CreateAsync(allocation);

                    remainingMoney -= amountToAllocate;
                }
            }

            // 2. Adım: TransferFromPast borçları bittikten sonra kalan para varsa normal borçlara dağıt
            if (remainingMoney > 0)
            {
                // Normal borçları getir (TransferFromPast hariç)
                // Eski aylardan başlayarak DueDate'e göre artan sırada sırala (eski borçlar önce)
                var normalDebts = await debtRepo.GetAll()
                    .Where(x => x.ApartmentId == apartmentId &&
                               !x.IsClosed &&
                               x.DebtType != DebtType.TransferFromPast)
                    .OrderBy(x => x.DueDate)
                    .ToListAsync();

                // Normal borçlara kalan parayı dağıt
                foreach (var debt in normalDebts)
                {
                    if (remainingMoney <= 0) break;

                    decimal debtRequiredAmount = debt.Amount - debt.PaidAmount;
                    decimal amountToAllocate = Math.Min(remainingMoney, debtRequiredAmount);

                    // Borcu güncelle
                    debt.PaidAmount += amountToAllocate;
                    debt.IsClosed = debt.PaidAmount >= debt.Amount;
                    debtRepo.Update(debt);

                    // Tahsisat Kaydını Oluştur (Allocation)
                    var allocation = new IncomeDebtAllocation
                    {
                        IncomeId = incomeId,
                        ApartmentDebtId = debt.Id,
                        AllocatedAmount = amountToAllocate
                    };
                    await allocationDebtRepo.CreateAsync(allocation);

                    remainingMoney -= amountToAllocate;
                }
            }

            // NOT: Döngü bittiğinde 'remainingMoney' hala artıyorsa, 
            // sistemde henüz 'gelecek ayın' borç kaydı oluşturulmamış demektir.
            // Bu durumda artan para havada kalır. Bunu 'Cari Alacak' (Credit) olarak kaydetmen gerekebilir.

            if (remainingMoney > 0)
            {
                var apartment = await apartmentRepo.GetByIdAsync(apartmentId);
                apartment.Balance += remainingMoney; // Fazla parayı alacak bakiyesine yaz
                apartmentRepo.Update(apartment);
            }
        }

        public async Task<byte[]> ExportPaymentMatrixToExcelAsync(int year)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var data = await GetYearlyPaymentMatrixAsync(year);

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add($"Aidat Takibi {year}");

            // Başlık satırı
            var headers = new[]
            {
                "Daire No", "Ev Sahibi", "Geçmişten Devir", "Toplam Borç", "Toplam Ödenen", "Kalan Borç",
                "Ocak", "Şubat", "Mart", "Nisan", "Mayıs", "Haziran",
                "Temmuz", "Ağustos", "Eylül", "Ekim", "Kasım", "Aralık"
            };

            // Başlıkları ekle ve formatla
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cells[1, i + 1].Value = headers[i];
                worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                worksheet.Cells[1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(79, 129, 189));
                worksheet.Cells[1, i + 1].Style.Font.Color.SetColor(Color.White);
                worksheet.Cells[1, i + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[1, i + 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }

            // Veri satırlarını ekle
            int row = 2;
            foreach (var item in data)
            {
                worksheet.Cells[row, 1].Value = item.ApartmentLabel;
                worksheet.Cells[row, 2].Value = item.OwnerName;
                worksheet.Cells[row, 3].Value = item.TransferredDebt;
                worksheet.Cells[row, 4].Value = item.TotalDebtUntilNow;
                worksheet.Cells[row, 5].Value = item.TotalPaid;
                worksheet.Cells[row, 6].Value = item.CurrentBalance;

                // Aylık ödemeler
                worksheet.Cells[row, 7].Value = item.Jan;
                worksheet.Cells[row, 8].Value = item.Feb;
                worksheet.Cells[row, 9].Value = item.Mar;
                worksheet.Cells[row, 10].Value = item.Apr;
                worksheet.Cells[row, 11].Value = item.May;
                worksheet.Cells[row, 12].Value = item.Jun;
                worksheet.Cells[row, 13].Value = item.Jul;
                worksheet.Cells[row, 14].Value = item.Aug;
                worksheet.Cells[row, 15].Value = item.Sep;
                worksheet.Cells[row, 16].Value = item.Oct;
                worksheet.Cells[row, 17].Value = item.Nov;
                worksheet.Cells[row, 18].Value = item.Dec;

                // Para formatı uygula (3. sütundan itibaren)
                for (int col = 3; col <= 18; col++)
                {
                    worksheet.Cells[row, col].Style.Numberformat.Format = "#,##0.00 ₺";
                    worksheet.Cells[row, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }

                // Kalan Borç pozitifse kırmızı, değilse yeşil
                if (item.CurrentBalance > 0)
                {
                    worksheet.Cells[row, 6].Style.Font.Color.SetColor(Color.Red);
                }
                else
                {
                    worksheet.Cells[row, 6].Style.Font.Color.SetColor(Color.Green);
                }

                // İlk iki sütun için border
                worksheet.Cells[row, 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                worksheet.Cells[row, 2].Style.Border.BorderAround(ExcelBorderStyle.Thin);

                row++;
            }

            // Sütun genişliklerini otomatik ayarla
            worksheet.Cells.AutoFitColumns();

            // Özet satırı ekle
            row++;
            worksheet.Cells[row, 1].Value = "TOPLAM";
            worksheet.Cells[row, 1].Style.Font.Bold = true;
            worksheet.Cells[row, 3].Value = data.Sum(x => x.TransferredDebt);
            worksheet.Cells[row, 4].Value = data.Sum(x => x.TotalDebtUntilNow);
            worksheet.Cells[row, 5].Value = data.Sum(x => x.TotalPaid);
            worksheet.Cells[row, 6].Value = data.Sum(x => x.CurrentBalance);

            for (int col = 3; col <= 6; col++)
            {
                worksheet.Cells[row, col].Style.Numberformat.Format = "#,##0.00 ₺";
                worksheet.Cells[row, col].Style.Font.Bold = true;
                worksheet.Cells[row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[row, col].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(221, 235, 247));
            }

            return await package.GetAsByteArrayAsync();
        }
    }
}