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
    public class IncomeCategoryService(IRepository<IncomeCategory> repository, IMapper mapper, IValidator<IncomeCategory> validator) : IIncomeCategoryService
    {
        public async Task<ServiceResult<CreateOrEditResponse>> CreateOrEdit(IncomeCategoryDto request)
        {
            var incomeCategory = mapper.Map<IncomeCategory>(request);
            //validasyon ekle
            var validationResult = validator.Validate(incomeCategory);

            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                return ServiceResult<CreateOrEditResponse>.Error(errors);
            }

            if (incomeCategory == null) return ServiceResult<CreateOrEditResponse>.Error();

            if (incomeCategory.Id > 0)
            {
                repository.Update(incomeCategory);
                return ServiceResult<CreateOrEditResponse>.Success(new CreateOrEditResponse { ID = incomeCategory.Id }, "Başarılı şekilde güncellenmiştir.");
            }
            await repository.CreateAsync(incomeCategory);

            return ServiceResult<CreateOrEditResponse>.Success(new CreateOrEditResponse { ID = incomeCategory.Id }, "Başarılı şekilde oluşturulmuştur.");
        }

        public async Task<bool> DeleteIncomeCategoryAsync(int id)
        {
            var incomeCategory = await repository.GetByIdAsync(id);
            if (incomeCategory == null) return false;
            repository.Delete(incomeCategory);
            return true;
        }

        public async Task<ServiceResult<DetailResponse<IncomeCategoryResponse>>> GetIncomeCategoryById(int id)
        {
            var incomeCategory = await repository.GetByIdAsync(id);

            if (incomeCategory == null)
                return ServiceResult<DetailResponse<IncomeCategoryResponse>>.Error("Belirtilen id ye sahip bir gelir kategorisi bulunamadı");

            var incomeCategoryResponse = mapper.Map<IncomeCategoryResponse>(incomeCategory);

            return ServiceResult<DetailResponse<IncomeCategoryResponse>>.Success(new DetailResponse<IncomeCategoryResponse> { Detail = incomeCategoryResponse });
        }

        public async Task<ServiceResult<SearchResponse<IncomeCategoryResponse>>> Search(IncomeCategorySearch request)
        {
            var query = repository.GetAll();
            var filteredQuery = query.WhereIf(!string.IsNullOrEmpty(request.Name), x => x.Name == request.Name)
                .WhereIf(!string.IsNullOrEmpty(request.Description), x => x.Description == request.Description)
                .Select(x => new IncomeCategoryResponse()
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description
                })
                .OrderBy(x => x.Id)
                .ToPagedList(request.Page, (int)request.PageSize);

            return ServiceResult<SearchResponse<IncomeCategoryResponse>>.Success(new SearchResponse<IncomeCategoryResponse>
            {
                SearchResult = filteredQuery.ToList(),
                TotalItemCount = query.Count()
            });
        }
    }
}

