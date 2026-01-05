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
    public class DuesSettingService(
        IRepository<DuesSetting> repository,
        IRepository<ApartmentDebt> debtRepo,
        IMapper mapper,
        IValidator<DuesSetting> validator,
        IUnitOfWork unitOfWork) : IDuesSettingService
    {
        public async Task<ServiceResult<CreateOrEditResponse>> CreateOrEdit(DuesSettingDto request)
        {
            var duesSetting = mapper.Map<DuesSetting>(request);
            //validasyon ekle
            var validationResult = validator.Validate(duesSetting);

            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                return ServiceResult<CreateOrEditResponse>.Error(errors);
            }

            if (duesSetting == null) return ServiceResult<CreateOrEditResponse>.Error();

            return await unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                if (duesSetting.Id > 0)
                {
                    repository.Update(duesSetting);
                    await ApartmentDebtsUpdate(duesSetting);
                    return ServiceResult<CreateOrEditResponse>.Success(new CreateOrEditResponse { ID = duesSetting.Id }, "Başarılı şekilde güncellenmiştir.");
                }

                await repository.CreateAsync(duesSetting);
                await ApartmentDebtsUpdate(duesSetting);

                return ServiceResult<CreateOrEditResponse>.Success(new CreateOrEditResponse { ID = duesSetting.Id }, "Başarılı şekilde oluşturulmuştur.");
            });
        }

        public async Task<bool> DeleteDuesSettingAsync(int id)
        {
            await unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var duesSetting = await repository.GetByIdAsync(id);
                if (duesSetting == null) return false;
                repository.Delete(duesSetting);
                return true;

            });
            return true;
        }

        public async Task<ServiceResult<DetailResponse<DuesSettingResponse>>> GetDuesSettingById(int id)
        {
            var duesSetting = await repository.GetByIdAsync(id);

            if (duesSetting == null)
                return ServiceResult<DetailResponse<DuesSettingResponse>>.Error("Belirtilen id ye sahip bir daire bulunamadı");

            var duesSettingResponse = mapper.Map<DuesSettingResponse>(duesSetting);

            return ServiceResult<DetailResponse<DuesSettingResponse>>.Success(new DetailResponse<DuesSettingResponse> { Detail = duesSettingResponse });
        }

        public async Task<ServiceResult<SearchResponse<DuesSettingResponse>>> Search(DuesSettingSearch request)
        {
            var query = repository.GetAll();
            var filteredQuery = query.WhereIf(request.Amount > 0, x => x.Id == request.Amount)
                .WhereIf(!string.IsNullOrEmpty(request.Description), x => x.Description == request.Description)
                .Select(x => new DuesSettingResponse()
                {
                    Id = x.Id,
                    Amount = x.Amount,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    IsActive = x.IsActive,
                    Description = x.Description
                })
                .OrderByDescending(x => x.StartDate)
                .ThenByDescending(x => x.Id)
                .ToPagedList(request.Page, (int)request.PageSize);

            return ServiceResult<SearchResponse<DuesSettingResponse>>.Success(new SearchResponse<DuesSettingResponse>
            {
                SearchResult = filteredQuery.ToList(),
                TotalItemCount = query.Count()
            });
        }

        private async Task ApartmentDebtsUpdate(DuesSetting duesSetting)
        {
            var currentYear = DateTime.Now.Year;
            var debts = await debtRepo.GetAll().Where(x => x.DueDate.Year == currentYear).ToListAsync();

            foreach (var item in debts)
            {
                item.Amount = duesSetting.Amount;
            }
        }
    }
}
