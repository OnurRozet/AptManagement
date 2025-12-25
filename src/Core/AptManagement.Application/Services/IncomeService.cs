using AptManagement.Application.Common;
using AptManagement.Application.Common.Base.Request;
using AptManagement.Application.Common.Base.Response;
using AptManagement.Application.Dtos;
using AptManagement.Application.Extensions;
using AptManagement.Application.Interfaces;
using AptManagement.Domain.Entities;
using AptManagement.Domain.Interfaces;
using AutoMapper;
using FluentValidation;
using X.PagedList.Extensions;

namespace AptManagement.Application.Services
{
    public class IncomeService(IRepository<Income> repository, IMapper mapper, IValidator<Income> validator) : IIncomeService
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
                .Select(x => new IncomeResponse()
                {
                    Id = x.Id,
                    Amount = x.Amount,
                    Title = x.Title,
                    IncomeDate = x.IncomeDate,
                    PaymentCategory = x.PaymentCategory,
                    IncomeCategoryId = x.IncomeCategoryId,
                    ApartmentId = x.ApartmentId
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

