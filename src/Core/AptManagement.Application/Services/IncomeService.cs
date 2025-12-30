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
    public class IncomeService(IRepository<Income> repository,IRepository<Apartment> apartmentRepo, IMapper mapper, IValidator<Income> validator) : IIncomeService
    {
        public async Task<ServiceResult<CreateOrEditResponse>> CreateOrEdit(IncomeDto request)
        {
            var income = mapper.Map<Income>(request);
            //validasyon ekle
            var validationResult = validator.Validate(income);

            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                return ServiceResult<CreateOrEditResponse>.Error(errors);
            }

            if (income == null) return ServiceResult<CreateOrEditResponse>.Error();

            if (income.Id > 0)
            {
                repository.Update(income);
                return ServiceResult<CreateOrEditResponse>.Success(new CreateOrEditResponse { ID = income.Id }, "Başarılı şekilde güncellenmiştir.");
            }
            await repository.CreateAsync(income);

            return ServiceResult<CreateOrEditResponse>.Success(new CreateOrEditResponse { ID = income.Id }, "Başarılı şekilde oluşturulmuştur.");
        }

        public async Task<bool> DeleteIncomeAsync(int id)
        {
            var income = await repository.GetByIdAsync(id);
            if (income == null) return false;
            repository.Delete(income);
            return true;
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

            var highestRevenue = repository.GetAll().Where(x => x.IncomeDate.Year == date.Year)
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
                .Where(x => x.IncomeDate.Year == currentYear)
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

            // Alt sorgular SQL tarafında 'Left Join' ve 'Sum' olarak çalışır
            Jan = apt.Incomes.Where(x => x.IncomeDate.Year == year && x.IncomeDate.Month == 1).Sum(x => (decimal?)x.Amount) ?? 0,
            Feb = apt.Incomes.Where(x => x.IncomeDate.Year == year && x.IncomeDate.Month == 2).Sum(x => (decimal?)x.Amount) ?? 0,
            Mar = apt.Incomes.Where(x => x.IncomeDate.Year == year && x.IncomeDate.Month == 3).Sum(x => (decimal?)x.Amount) ?? 0,
            Apr = apt.Incomes.Where(x => x.IncomeDate.Year == year && x.IncomeDate.Month == 4).Sum(x => (decimal?)x.Amount) ?? 0,
            May = apt.Incomes.Where(x => x.IncomeDate.Year == year && x.IncomeDate.Month == 5).Sum(x => (decimal?)x.Amount) ?? 0,
            Jun = apt.Incomes.Where(x => x.IncomeDate.Year == year && x.IncomeDate.Month == 6).Sum(x => (decimal?)x.Amount) ?? 0,
            Jul = apt.Incomes.Where(x => x.IncomeDate.Year == year && x.IncomeDate.Month == 7).Sum(x => (decimal?)x.Amount) ?? 0,
            Aug = apt.Incomes.Where(x => x.IncomeDate.Year == year && x.IncomeDate.Month == 8).Sum(x => (decimal?)x.Amount) ?? 0,
            Sep = apt.Incomes.Where(x => x.IncomeDate.Year == year && x.IncomeDate.Month == 9).Sum(x => (decimal?)x.Amount) ?? 0,
            Oct = apt.Incomes.Where(x => x.IncomeDate.Year == year && x.IncomeDate.Month == 10).Sum(x => (decimal?)x.Amount) ?? 0,
            Nov = apt.Incomes.Where(x => x.IncomeDate.Year == year && x.IncomeDate.Month == 11).Sum(x => (decimal?)x.Amount) ?? 0,
            Dec = apt.Incomes.Where(x => x.IncomeDate.Year == year && x.IncomeDate.Month == 12).Sum(x => (decimal?)x.Amount) ?? 0
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
                .OrderBy(x => x.Id)
                .ToPagedList(request.Page, (int)request.PageSize);

            return ServiceResult<SearchResponse<IncomeResponse>>.Success(new SearchResponse<IncomeResponse>
            {
                SearchResult = filteredQuery.ToList(),
                TotalItemCount = query.Count()
            });
        }
    }
}

