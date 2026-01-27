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
using Microsoft.EntityFrameworkCore;
using X.PagedList.Extensions;

namespace AptManagement.Application.Services
{
    public class IncomeDebtAllocationService(
        IRepository<IncomeDebtAllocation> repository,
        IRepository<Income> incomeRepo,
        IRepository<ApartmentDebt> debtRepo,
        IMapper mapper,
        IValidator<IncomeDebtAllocation> validator,
        IUnitOfWork unitOfWork) : IIncomeDebtAllocationService
    {
        public async Task<ServiceResult<CreateOrEditResponse>> CreateOrEdit(IncomeDebtAllocationDto request)
        {
            var allocation = mapper.Map<IncomeDebtAllocation>(request);

            // Validasyon
            var validationResult = validator.Validate(allocation);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                return ServiceResult<CreateOrEditResponse>.Error(errors);
            }

            return await unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                // Güncelleme senaryosu
                if (allocation.Id > 0)
                {
                    var existingAllocation = await repository.GetByIdAsync(allocation.Id);
                    if (existingAllocation == null)
                        return ServiceResult<CreateOrEditResponse>.Error("Kayıt bulunamadı.");

                    mapper.Map(request, existingAllocation);
                    repository.Update(existingAllocation);

                    return ServiceResult<CreateOrEditResponse>.Success(
                        new CreateOrEditResponse { ID = existingAllocation.Id },
                        "Başarılı şekilde güncellenmiştir.");
                }

                // Yeni kayıt senaryosu
                await repository.CreateAsync(allocation);

                return ServiceResult<CreateOrEditResponse>.Success(
                    new CreateOrEditResponse { ID = allocation.Id },
                    "Başarılı şekilde oluşturulmuştur.");
            });
        }

        public async Task<ServiceResult<bool>> DeleteIncomeDebtAllocationAsync(int id)
        {
            return await unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var allocation = await repository.GetByIdAsync(id);
                if (allocation == null)
                    return ServiceResult<bool>.Error("Silinecek tahsisat kaydı bulunamadı");

                repository.Delete(allocation);

                return ServiceResult<bool>.Success(true);
            });
        }

        public async Task<ServiceResult<DetailResponse<IncomeDebtAllocationResponse>>> GetIncomeDebtAllocationById(int id)
        {
            var allocation = await repository.GetAll(
                x => x.Income,
                x => x.ApartmentDebt,
                x => x.ApartmentDebt.Apartment)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (allocation == null)
                return ServiceResult<DetailResponse<IncomeDebtAllocationResponse>>.Error(
                    "Belirtilen id ye sahip bir tahsisat bulunamadı");

            var allocationResponse = mapper.Map<IncomeDebtAllocationResponse>(allocation);
            allocationResponse.IncomeTitle = allocation.Income?.Title;
            allocationResponse.IncomeAmount = allocation.Income?.Amount ?? 0;
            allocationResponse.IncomeDate = allocation.Income?.IncomeDate;
            allocationResponse.ApartmentLabel = allocation.ApartmentDebt?.Apartment?.Label;
            allocationResponse.DebtDueDate = allocation.ApartmentDebt?.DueDate;
            allocationResponse.DebtAmount = allocation.ApartmentDebt?.Amount;

            return ServiceResult<DetailResponse<IncomeDebtAllocationResponse>>.Success(
                new DetailResponse<IncomeDebtAllocationResponse> { Detail = allocationResponse });
        }

        public async Task<ServiceResult<SearchResponse<IncomeDebtAllocationResponse>>> Search(IncomeDebtAllocationSearch request)
        {
            var query = repository.GetAll(
                x => x.Income,
                x => x.ApartmentDebt,
                x => x.ApartmentDebt.Apartment);

            var filteredQuery = query
                .WhereIf(request.IncomeId.HasValue && request.IncomeId.Value > 0, 
                    x => x.IncomeId == request.IncomeId.Value)
                .WhereIf(request.ApartmentDebtId.HasValue && request.ApartmentDebtId.Value > 0, 
                    x => x.ApartmentDebtId == request.ApartmentDebtId.Value)
                .WhereIf(request.AllocatedAmount.HasValue && request.AllocatedAmount.Value > 0, 
                    x => x.AllocatedAmount == request.AllocatedAmount.Value)
                .WhereIf(!string.IsNullOrEmpty(request.Keyword), 
                    x => x.Income != null && (x.Income.Title != null && x.Income.Title.Contains(request.Keyword)) ||
                         x.ApartmentDebt != null && x.ApartmentDebt.Apartment != null && 
                         x.ApartmentDebt.Apartment.Label.Contains(request.Keyword))
                .Select(x => new IncomeDebtAllocationResponse()
                {
                    Id = x.Id,
                    IncomeId = x.IncomeId,
                    ApartmentDebtId = x.ApartmentDebtId,
                    AllocatedAmount = x.AllocatedAmount,
                    IncomeTitle = x.Income != null ? x.Income.Title : null,
                    IncomeAmount = x.Income != null ? x.Income.Amount : 0,
                    IncomeDate = x.Income != null ? x.Income.IncomeDate : (DateTime?)null,
                    ApartmentLabel = x.ApartmentDebt != null && x.ApartmentDebt.Apartment != null 
                        ? x.ApartmentDebt.Apartment.Label : null,
                    DebtDueDate = x.ApartmentDebt != null ? x.ApartmentDebt.DueDate : (DateTime?)null,
                    DebtAmount = x.ApartmentDebt != null ? x.ApartmentDebt.Amount : (decimal?)null
                })
                .OrderByDescending(x => x.Id)
                .ToPagedList(request.Page, (int)request.PageSize);

            return ServiceResult<SearchResponse<IncomeDebtAllocationResponse>>.Success(
                new SearchResponse<IncomeDebtAllocationResponse>
                {
                    SearchResult = filteredQuery.ToList(),
                    TotalItemCount = query.Count()
                });
        }
    }
}

