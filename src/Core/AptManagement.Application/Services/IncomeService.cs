using AptManagement.Application.Common;
using AptManagement.Application.Common.Base.Request;
using AptManagement.Application.Common.Base.Response;
using AptManagement.Application.Dtos;
using AptManagement.Application.Dtos.Reports;
using AptManagement.Application.Extensions;
using AptManagement.Application.Interfaces;
using AptManagement.Domain.Entities;
using AptManagement.Domain.Interfaces;
using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using X.PagedList.Extensions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace AptManagement.Application.Services
{
    public class IncomeService(
        IRepository<Income> repository,
        IRepository<Apartment> apartmentRepo,
        IRepository<ApartmentDebt> debtRepo,
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
                    var existingIncome = await repository.GetByIdAsync(income.Id);
                    if (existingIncome == null) return ServiceResult<CreateOrEditResponse>.Error("Kayıt bulunamadı.");

                    var specificDebt = await debtRepo.GetAll()
                         .FirstOrDefaultAsync(x => x.ApartmentId == income.ApartmentId &&
                                                   x.DueDate.Month == income.IncomeDate.Month &&
                                                   x.DueDate.Year == income.IncomeDate.Year);

                    if (specificDebt != null)
                    {
                        // Eski ödemeyi geri al
                        specificDebt.PaidAmount -= existingIncome.Amount;

                        // Yeni tutarı ekle 
                        specificDebt.PaidAmount += income.Amount;

                        // Eğer ödenen tutar borçtan fazlaysa sadece borç kadarını yaz.
                        decimal remaningMoney = 0;
                        if (specificDebt.PaidAmount > specificDebt.Amount)
                        {
                            remaningMoney = specificDebt.PaidAmount - specificDebt.Amount;
                            specificDebt.PaidAmount = specificDebt.Amount;

                        }

                        specificDebt.IsClosed = specificDebt.PaidAmount >= specificDebt.Amount;
                        debtRepo.Update(specificDebt);


                        // Parayı al ve borçlara sırasıyla dağıt
                        await DistributePaymentToDebtsAsync(income.ApartmentId, income.IncomeDate, remaningMoney);
                    }

                    mapper.Map(request, existingIncome);
                    repository.Update(existingIncome);

                    return ServiceResult<CreateOrEditResponse>.Success(new CreateOrEditResponse { ID = income.Id }, "Güncellendi.");
                }

                // --- YENİ KAYIT (CREATE) SENARYOSU ---
                // Gelir kaydını oluştur
                await repository.CreateAsync(income); // Önce ID oluşsun diye create ediyoruz (Transaction varsa rollback edilebilir)

                // Parayı al ve borçlara sırasıyla dağıt
                await DistributePaymentToDebtsAsync(income.ApartmentId, income.IncomeDate, income.Amount);

                return ServiceResult<CreateOrEditResponse>.Success(new CreateOrEditResponse { ID = income.Id }, "Oluşturuldu ve borçlardan düşüldü.");
            });
        }

        public async Task<ServiceResult<bool>> DeleteIncomeAsync(int id)
        {
            return await unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var income = await repository.GetAll()
                    .Include(x => x.ApartmentDebt)
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (income == null) return ServiceResult<bool>.Error("Silinecek Gelir Kaydı Bulunamadı");

                //Eğer bu gelir bir borca (aidata) bağlıysa, borç durumunu geri al
                if (income.ApartmentDebt != null)
                {
                    // Borcun ödenmiş tutarından, silinen gelir miktarını düş
                    income.ApartmentDebt.PaidAmount -= income.Amount;

                    // Eğer ödenen miktar toplam borçtan az kaldıysa IsClosed'ı false yap
                    if (income.ApartmentDebt.PaidAmount < income.ApartmentDebt.Amount)
                    {
                        income.ApartmentDebt.IsClosed = false;
                    }

                    // Eğer borç miktarı 0'ın altına düşerse (güvenlik kontrolü) sıfıra eşitle
                    if (income.ApartmentDebt.PaidAmount < 0)
                        income.ApartmentDebt.PaidAmount = 0;

                    debtRepo.Update(income.ApartmentDebt); // Entity Framework bunu otomatik takip eder zaten
                }

                repository.Delete(income);

                return ServiceResult<bool>.Success(true);
            });
        }

        public async Task<ServiceResult<DetailResponse<IncomeResponse>>> GetIncomeById(int id)
        {
            var income = await repository.GetByIdAsync(id);

            if (income == null)
                return ServiceResult<DetailResponse<IncomeResponse>>.Error("Belirtilen id ye sahip bir gelir bulunamadı");

            var incomeResponse = mapper.Map<IncomeResponse>(income);

            return ServiceResult<DetailResponse<IncomeResponse>>.Success(new DetailResponse<IncomeResponse> { Detail = incomeResponse });
        }

        public ServiceResult<IncomeSummaryDto> GetSummaryIncomeReport()
        {
            DateTime date = DateTime.Now;
            int currentYear = DateTime.Now.Year;


            var totalRevenue = repository.GetAll().Sum(x => x.Amount);

            var revenueByCurrentMonth = repository.GetAll().Where(x => x.IncomeDate.Month == date.Month).Sum(x => x.Amount);

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
                .ThenByDescending(x => x.TotalAmount)       // Eşitlik varsa tutarı çok olanı üste al
                .Take(2) // İstenen sayı kadar al (Sen 2 istedin)
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
         var matrix = await apartmentRepo.GetAll()
        .AsNoTracking() // Performans için önemli
        .Select(apt => new PaymentMatrixDto
        {
            ApartmentId = apt.Id,
            ApartmentLabel = apt.Label,
            OwnerName = apt.OwnerName,
            IsManager = apt.IsManager,
            // Alt sorgular SQL tarafında 'Left Join' ve 'Sum' olarak çalışır
            Jan = apt.Debts.Where(x => x.DueDate.Year == year && x.DueDate.Month == 1).Sum(x => (decimal?)x.PaidAmount) ?? 0,
            Feb = apt.Debts.Where(x => x.DueDate.Year == year && x.DueDate.Month == 2).Sum(x => (decimal?)x.PaidAmount) ?? 0,
            Mar = apt.Debts.Where(x => x.DueDate.Year == year && x.DueDate.Month == 3).Sum(x => (decimal?)x.PaidAmount) ?? 0,
            Apr = apt.Debts.Where(x => x.DueDate.Year == year && x.DueDate.Month == 4).Sum(x => (decimal?)x.PaidAmount) ?? 0,
            May = apt.Debts.Where(x => x.DueDate.Year == year && x.DueDate.Month == 5).Sum(x => (decimal?)x.PaidAmount) ?? 0,
            Jun = apt.Debts.Where(x => x.DueDate.Year == year && x.DueDate.Month == 6).Sum(x => (decimal?)x.PaidAmount) ?? 0,
            Jul = apt.Debts.Where(x => x.DueDate.Year == year && x.DueDate.Month == 7).Sum(x => (decimal?)x.PaidAmount) ?? 0,
            Aug = apt.Debts.Where(x => x.DueDate.Year == year && x.DueDate.Month == 8).Sum(x => (decimal?)x.PaidAmount) ?? 0,
            Sep = apt.Debts.Where(x => x.DueDate.Year == year && x.DueDate.Month == 9).Sum(x => (decimal?)x.PaidAmount) ?? 0,
            Oct = apt.Debts.Where(x => x.DueDate.Year == year && x.DueDate.Month == 10).Sum(x => (decimal?)x.PaidAmount) ?? 0,
            Nov = apt.Debts.Where(x => x.DueDate.Year == year && x.DueDate.Month == 11).Sum(x => (decimal?)x.PaidAmount) ?? 0,
            Dec = apt.Debts.Where(x => x.DueDate.Year == year && x.DueDate.Month == 12).Sum(x => (decimal?)x.PaidAmount) ?? 0
        })
        .OrderBy(x => x.ApartmentId)
        .ToListAsync();

            return matrix;
        }

        public async Task<ServiceResult<SearchResponse<IncomeResponse>>> Search(IncomeSearch request)
        {
            var query = repository.GetAll();
            var filteredQuery = query.WhereIf(request.IncomeId.HasValue && request.IncomeId.Value > 0, x => x.Id == request.IncomeId.Value)
                .WhereIf(request.Amount.HasValue && request.Amount.Value > 0, x => x.Amount == request.Amount.Value)
                .WhereIf(!string.IsNullOrEmpty(request.Title), x => x.Title == request.Title)
                .WhereIf(request.IncomeDate.HasValue, x => x.IncomeDate.Date == request.IncomeDate.Value.Date)
                .WhereIf(request.PaymentCategory.HasValue, x => x.PaymentCategory == request.PaymentCategory.Value)
                .WhereIf(request.IncomeCategoryId.HasValue && request.IncomeCategoryId.Value > 0, x => x.IncomeCategoryId == request.IncomeCategoryId.Value)
                .WhereIf(request.ApartmentId.HasValue && request.ApartmentId.Value > 0, x => x.ApartmentId == request.ApartmentId.Value)
                .WhereIf(!string.IsNullOrEmpty(request.Keyword), x => x.Title.Contains(request.Keyword) || x.IncomeCategory.Name.Contains(request.Keyword) || x.Apartment.OwnerName.Contains(request.Keyword))
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
                    ApartmentNo = x.Apartment.Label + " - " + ($"{(!string.IsNullOrEmpty(x.Apartment.TenantName) ? x.Apartment.TenantName : x.Apartment.OwnerName)}")
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

        private async Task DistributePaymentToDebtsAsync(int apartmentId, DateTime incomeDate, decimal paymentAmount)
        {
            // 1. Adım: Dairenin KAPANMAMIŞ tüm borçlarını çekiyoruz.
            // Sıralamayı henüz yapmıyoruz, hepsini hafızaya alıp biz sıralayacağız.
            var allOpenDebts = await debtRepo.GetAll()
                .Where(x => x.ApartmentId == apartmentId && !x.IsClosed)
                .ToListAsync();

            // Eğer hiç borç yoksa işlem yapmaya gerek yok (veya bakiyeye atılabilir)
            if (!allOpenDebts.Any())
            {
                //Yapılan ödemeyi alacak listesine eklemeliyiz
                var apartment = await apartmentRepo.GetByIdAsync(apartmentId);
             
                apartment.Balance += paymentAmount; 
                apartmentRepo.Update(apartment);
                return;
            };

            // 2. Adım: Borçları kategorilere ayırıyoruz

            // A) MEVCUT AYIN BORCU (Ödeme tarihiyle aynı Ay ve Yılda olan borç)
            var currentMonthDebt = allOpenDebts
                .FirstOrDefault(x => x.DueDate.Month == incomeDate.Month && x.DueDate.Year == incomeDate.Year);

            // B) ESKİ BORÇLAR (Vadesi mevcut aydan önce olanlar - Tarihe göre artan sıralı)
            var pastDebts = allOpenDebts
                .Where(x => x.DueDate.Date < new DateTime(incomeDate.Year, incomeDate.Month, 1))
                .OrderBy(x => x.DueDate)
                .ToList();

            // C) GELECEK BORÇLAR (Vadesi mevcut aydan sonra olanlar - Tarihe göre artan sıralı)
            //var futureDebts = allOpenDebts
            //    .Where(x => x.DueDate.Date > new DateTime(incomeDate.Year, incomeDate.Month, 1).AddMonths(1).AddDays(-1))
            //    // Not: Basitçe mevcut ayın borcu ve eski borçlar dışındakiler de diyebilirdik ama garanti olsun.
            //    .Where(x => x != currentMonthDebt) // Mevcut ayı tekrar almamak için
            //    .Except(pastDebts) // Eskileri tekrar almamak için
            //    .OrderBy(x => x.DueDate)
            //    .ToList();


            // 3. Adım: ÖDEME KUYRUĞUNU OLUŞTUR 
            var paymentQueue = new List<ApartmentDebt>();

            // Öncelik 1: Önce şu anki ay
            if (currentMonthDebt != null)
                paymentQueue.Add(currentMonthDebt);

            // Öncelik 2: Sonra geçmiş borçlar
            paymentQueue.AddRange(pastDebts);

            // Öncelik 3: En son gelecek borçlar
            //paymentQueue.AddRange(futureDebts);


            // 4. Adım: Dağıtım Döngüsü (Standart Mantık)
            decimal remainingMoney = paymentAmount;

            foreach (var debt in paymentQueue)
            {
                if (remainingMoney <= 0) break;

                decimal debtBalance = debt.Amount - debt.PaidAmount;

                if (remainingMoney >= debtBalance)
                {
                    // Borcu tamamen kapat
                    debt.PaidAmount = debt.Amount;
                    debt.IsClosed = true;
                    remainingMoney -= debtBalance;
                }
                else
                {
                    // Paranın yettiği kadarını öde
                    debt.PaidAmount += remainingMoney;
                    remainingMoney = 0;
                }

                debtRepo.Update(debt);
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
    }
}

