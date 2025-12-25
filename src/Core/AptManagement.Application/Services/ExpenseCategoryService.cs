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
    public class ExpenseCategoryService(IRepository<ExpenseCategory> repository, IMapper mapper, IValidator<ExpenseCategory> validator) : IExpenseCategoryService
    {
        public async Task<ServiceResult<CreateOrEditResponse>> CreateOrEdit(ExpenseCategoryDto request)
        {
            var expenseCategory = mapper.Map<ExpenseCategory>(request);
            //validasyon ekle
            var validationResult = validator.Validate(expenseCategory);

            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                return ServiceResult<CreateOrEditResponse>.Error(errors);
            }

            if (expenseCategory == null) return ServiceResult<CreateOrEditResponse>.Error();

            if (expenseCategory.Id > 0) repository.Update(expenseCategory);

            await repository.CreateAsync(expenseCategory);

            return ServiceResult<CreateOrEditResponse>.Success(new CreateOrEditResponse { ID = expenseCategory.Id }, "Başarılı şekilde oluşturulmuştur.");
        }

        public async Task<bool> DeleteExpenseCategoryAsync(int id)
        {
            var expenseCategory = await repository.GetByIdAsync(id);
            if (expenseCategory == null) return false;
            repository.Delete(expenseCategory);
            return true;
        }

        public async Task<ServiceResult<DetailResponse<ExpenseCategoryResponse>>> GetExpenseCategoryById(int id)
        {
            var expenseCategory = await repository.GetByIdAsync(id);

            if (expenseCategory == null)
                return ServiceResult<DetailResponse<ExpenseCategoryResponse>>.Error("Belirtilen id ye sahip bir gider kategorisi bulunamadı");

            var expenseCategoryResponse = mapper.Map<ExpenseCategoryResponse>(expenseCategory);

            return ServiceResult<DetailResponse<ExpenseCategoryResponse>>.Success(new DetailResponse<ExpenseCategoryResponse> { Detail = expenseCategoryResponse });
        }

        public async Task<ServiceResult<SearchResponse<ExpenseCategoryResponse>>> Search(ExpenseCategorySearch request)
        {
            var query = repository.GetAll();
            var filteredQuery = query.WhereIf(!string.IsNullOrEmpty(request.Name), x => x.Name == request.Name)
                .WhereIf(!string.IsNullOrEmpty(request.Description), x => x.Description == request.Description)
                .Select(x => new ExpenseCategoryResponse()
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description
                })
                .OrderBy(x => x.Id)
                .ToPagedList(request.Page, (int)request.PageSize);

            return ServiceResult<SearchResponse<ExpenseCategoryResponse>>.Success(new SearchResponse<ExpenseCategoryResponse>
            {
                SearchResult = filteredQuery.ToList(),
                TotalItemCount = query.Count()
            });
        }
    }
}

