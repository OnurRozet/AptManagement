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
    public class ManagementPeriodService(
        IRepository<ManagementPeriod> repository,
        IRepository<Apartment> apartmentRepo,
        IRepository<ApartmentDebt> debtsRepo,
        IRepository<DuesSetting> dueSettingRepo,
        IMapper mapper,
        IValidator<ManagementPeriod> validator,
        IUnitOfWork unitOfWork) : IManagementPeriodService
    {
        public async Task<ServiceResult<CreateOrEditResponse>> CreateOrEdit(ManagementPeriodDto request)
        {
            var period = mapper.Map<ManagementPeriod>(request);

            // Validasyon
            var validationResult = validator.Validate(period);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                return ServiceResult<CreateOrEditResponse>.Error(errors);
            }

            return await unitOfWork.ExecuteInTransactionAsync(async () =>
            {

                await RetrievalOldManager(period);

                // Güncelleme senaryosu
                if (period.Id > 0)
                {
                    var existingPeriod = await repository.GetByIdAsync(period.Id);
                    if (existingPeriod == null)
                        return ServiceResult<CreateOrEditResponse>.Error("Kayıt bulunamadı.");

                    mapper.Map(request, existingPeriod);
                    repository.Update(existingPeriod);

                    return ServiceResult<CreateOrEditResponse>.Success(
                        new CreateOrEditResponse { ID = existingPeriod.Id },
                        "Başarılı şekilde güncellenmiştir.");
                }

                // Yeni kayıt senaryosu
                await repository.CreateAsync(period);

                //Yeni yöneticinin gelecekteki borçlarını 0 TL yap
                await ExemptFutureDebts(period);

                return ServiceResult<CreateOrEditResponse>.Success(
                    new CreateOrEditResponse { ID = period.Id },
                    "Başarılı şekilde oluşturulmuştur.");
            });
        }

        public async Task<ServiceResult<bool>> DeleteManagementPeriodAsync(int id)
        {
            return await unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var period = await repository.GetByIdAsync(id);
                if (period == null)
                    return ServiceResult<bool>.Error("Silinecek yönetim dönemi kaydı bulunamadı");

                repository.Delete(period);

                return ServiceResult<bool>.Success(true);
            });
        }

        public async Task<ServiceResult<DetailResponse<ManagementPeriodResponse>>> GetManagementPeriodById(int id)
        {
            var period = await repository.GetAll(
                x => x.Apartment)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (period == null)
                return ServiceResult<DetailResponse<ManagementPeriodResponse>>.Error(
                    "Belirtilen id ye sahip bir yönetim dönemi bulunamadı");

            var periodResponse = mapper.Map<ManagementPeriodResponse>(period);
            periodResponse.ApartmentLabel = period.Apartment?.Label;
            periodResponse.ApartmentOwnerName = period.Apartment?.OwnerName;

            return ServiceResult<DetailResponse<ManagementPeriodResponse>>.Success(
                new DetailResponse<ManagementPeriodResponse> { Detail = periodResponse });
        }

        public async Task<ServiceResult<SearchResponse<ManagementPeriodResponse>>> Search(ManagementPeriodSearch request)
        {
            var query = repository.GetAll(
                x => x.Apartment);

            var filteredQuery = query
                .WhereIf(request.ApartmentId.HasValue && request.ApartmentId.Value > 0,
                    x => x.ApartmentId == request.ApartmentId.Value)
                .WhereIf(request.StartDate.HasValue,
                    x => x.StartDate.Date == request.StartDate.Value.Date)
                .WhereIf(request.EndDate.HasValue,
                    x => x.EndDate.Date == request.EndDate.Value.Date)
                .WhereIf(request.IsExemptFromDues.HasValue,
                    x => x.IsExemptFromDues == request.IsExemptFromDues.Value)
                .WhereIf(!string.IsNullOrEmpty(request.Keyword),
                    x => x.Apartment != null && (
                        (x.Apartment.Label != null && x.Apartment.Label.Contains(request.Keyword)) ||
                        (x.Apartment.OwnerName != null && x.Apartment.OwnerName.Contains(request.Keyword))))
                .Select(x => new ManagementPeriodResponse()
                {
                    Id = x.Id,
                    ApartmentId = x.ApartmentId,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    IsExemptFromDues = x.IsExemptFromDues,
                    ApartmentLabel = x.Apartment != null ? x.Apartment.Label : null,
                    ApartmentOwnerName = x.Apartment != null ? x.Apartment.OwnerName : null
                })
                .OrderByDescending(x => x.Id)
                .ToPagedList(request.Page, (int)request.PageSize);

            return ServiceResult<SearchResponse<ManagementPeriodResponse>>.Success(
                new SearchResponse<ManagementPeriodResponse>
                {
                    SearchResult = filteredQuery.ToList(),
                    TotalItemCount = query.Count()
                });
        }

        private async Task RetrievalOldManager(ManagementPeriod period)
        {
            var currentActiveManager = await repository.GetAll()
             .FirstOrDefaultAsync(x => x.EndDate == null);

            if (currentActiveManager != null)
            {
                currentActiveManager.EndDate = DateTime.Now;
                repository.Update(currentActiveManager);

                await ResetFutureDebts(currentActiveManager.ApartmentId,period);
            }

            //Yeni yöneticinin dairesini güncelle
            //Apartmentiçindeki IsManager ı güncelle
            var apartment = await apartmentRepo.GetAll().FirstOrDefaultAsync(x => x.Id == period.ApartmentId);
            apartment.IsManager = true;
            apartmentRepo.Update(apartment);

        }

        private async Task ExemptFutureDebts(ManagementPeriod period)
        {
            // Başlangıç ve bitiş tarihi arasındaki henüz KAPANMAMIŞ (ödenmemiş) borçları bul
            var futureDebts = await debtsRepo.GetAll()
                .Where(x => x.ApartmentId == period.ApartmentId &&
                            !x.IsClosed &&
                            x.DueDate >= period.StartDate && x.DueDate <= period.EndDate)
                .ToListAsync();

            foreach (var debt in futureDebts)
            {
                debt.Amount = 0; // Yönetici olduğu için borcu sıfırla
                debt.Description = "Yönetici Muafiyeti (Otomatik)";
                debtsRepo.Update(debt);
            }
        }

        private async Task ResetFutureDebts(int apartmentId, ManagementPeriod period)
        {
            // Önce dairenin standart aidat tutarını alalım
            var apartment = await apartmentRepo.GetByIdAsync(apartmentId);
            if (apartment == null) return;

            // Bugünden sonraki ve henüz KAPANMAMIŞ borçları bul
            var futureDebts = await debtsRepo.GetAll()
                .Where(x => x.ApartmentId == apartmentId &&
                            !x.IsClosed &&
                            x.DueDate >= period.StartDate)
                .ToListAsync();

            //Aidat miktraını alalım
            var due = await dueSettingRepo.GetAll().Where(x => x.IsActive).FirstOrDefaultAsync();

            foreach (var debt in futureDebts)
            {
                // Artık yönetici değil, normal aidat tutarına geri çek
                debt.Amount = due.Amount;
                debt.Description = "Aidat Borcu (Yöneticilik Sona Erdi)";
                debtsRepo.Update(debt);
            }
        }
    }
}

