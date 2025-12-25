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
    public class ExpenseService(IRepository<Expense> repository, IMapper mapper, IValidator<Expense> validator) : IExpenseService
    {
        public async Task<ServiceResult<CreateOrEditResponse>> CreateOrEdit(ExpenseDto request)
        {
            var expense = mapper.Map<Expense>(request);
            //validasyon ekle
            var validationResult = validator.Validate(expense);

            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                return ServiceResult<CreateOrEditResponse>.Error(errors);
            }

            if (expense == null) return ServiceResult<CreateOrEditResponse>.Error();

            if (expense.Id > 0) repository.Update(expense);

            await repository.CreateAsync(expense);

            return ServiceResult<CreateOrEditResponse>.Success(new CreateOrEditResponse { ID = expense.Id }, "Başarılı şekilde oluşturulmuştur.");
        }

        public async Task<bool> DeleteExpenseAsync(int id)
        {
            var expense = await repository.GetByIdAsync(id);
            if (expense == null) return false;
            repository.Delete(expense);
            return true;
        }

        public async Task<ServiceResult<DetailResponse<ExpenseResponse>>> GetExpenseById(int id)
        {
            var expense = await repository.GetByIdAsync(id);

            if (expense == null)
                return ServiceResult<DetailResponse<ExpenseResponse>>.Error("Belirtilen id ye sahip bir gider bulunamadı");

            var expenseResponse = mapper.Map<ExpenseResponse>(expense);

            return ServiceResult<DetailResponse<ExpenseResponse>>.Success(new DetailResponse<ExpenseResponse> { Detail = expenseResponse });
        }

        public async Task<ServiceResult<SearchResponse<ExpenseResponse>>> Search(ExpenseSearch request)
        {
            var query = repository.GetAll();
            var filteredQuery = query.WhereIf(request.ExpenseId.HasValue && request.ExpenseId.Value > 0, x => x.Id == request.ExpenseId.Value)
                .WhereIf(request.Amount.HasValue && request.Amount.Value > 0, x => x.Amount == request.Amount.Value)
                .WhereIf(!string.IsNullOrEmpty(request.Title), x => x.Title == request.Title)
                .WhereIf(request.ExpenseDate.HasValue, x => x.ExpenseDate.Date == request.ExpenseDate.Value.Date)
                .WhereIf(request.PaymentCategory.HasValue, x => x.PaymentCategory == request.PaymentCategory.Value)
                .WhereIf(request.ExpenseCategoryId.HasValue && request.ExpenseCategoryId.Value > 0, x => x.ExpenseCategoryId == request.ExpenseCategoryId.Value)
                .Select(x => new ExpenseResponse()
                {
                    Id = x.Id,
                    Amount = x.Amount,
                    Title = x.Title,
                    ExpenseDate = x.ExpenseDate,
                    PaymentCategory = x.PaymentCategory,
                    ExpenseCategoryId = x.ExpenseCategoryId
                })
                .OrderBy(x => x.Id)
                .ToPagedList(request.Page, (int)request.PageSize);

            return ServiceResult<SearchResponse<ExpenseResponse>>.Success(new SearchResponse<ExpenseResponse>
            {
                SearchResult = filteredQuery.ToList(),
                TotalItemCount = query.Count()
            });
        }
    }
}

