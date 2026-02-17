using AptManagement.Application.Common;
using AptManagement.Application.Common.Base.Request;
using AptManagement.Application.Common.Base.Response;
using AptManagement.Application.Dtos;
using AptManagement.Application.Extensions;
using AptManagement.Application.Interfaces;
using AptManagement.Domain.Entities;
using AptManagement.Domain.Enums;
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
                    var existingPeriod = await repository.GetAll(x => x.Apartment)
                        .FirstOrDefaultAsync(x => x.Id == period.Id);
                    if (existingPeriod == null)
                        return ServiceResult<CreateOrEditResponse>.Error("Kayıt bulunamadı.");

                    var wasActive = existingPeriod.IsActive;
                    var wasExempt = existingPeriod.IsExemptFromDues;
                    var oldEndDate = existingPeriod.EndDate;

                    mapper.Map(request, existingPeriod);
                    repository.Update(existingPeriod);

                    if (wasActive && !existingPeriod.IsActive)
                    {
                        await SetApartmentIsManagerAsync(existingPeriod.ApartmentId, false);
                        if (wasExempt)
                            await ResetFutureDebts(existingPeriod.ApartmentId, DateTime.Now);
                    }
                    else if (!wasActive && existingPeriod.IsActive && existingPeriod.IsExemptFromDues)
                    {
                        await ExemptFutureDebts(existingPeriod);
                    }
                    else if (wasActive && existingPeriod.IsActive)
                    {
                        // Aktif kalıyor; tarih veya muafiyet değiştiyse ApartmentDebts senkronize et
                        await SyncDebtsWithPeriodAsync(existingPeriod, wasExempt, oldEndDate);
                    }

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

                var apartmentId = period.ApartmentId;
                var wasExempt = period.IsExemptFromDues;

                repository.Delete(period);

                await SetApartmentIsManagerAsync(apartmentId, false);
                if (wasExempt)
                    await ResetFutureDebts(apartmentId, DateTime.Now);

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
                    x => x.EndDate != null && x.EndDate.Value.Date == request.EndDate!.Value.Date)
                .WhereIf(request.IsExemptFromDues.HasValue,
                    x => x.IsExemptFromDues == request.IsExemptFromDues.Value)
                .WhereIf(request.IsActive.HasValue,
                    x => x.IsActive == request.IsActive.Value)
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
                    IsActive = x.IsActive,
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
            var deactivationDate = DateTime.Now;

            var currentManagerApartment = await apartmentRepo.GetAll()
                .FirstOrDefaultAsync(x => x.IsManager && x.Id != period.ApartmentId);
            var currentActiveManager = currentManagerApartment != null
                ? await repository.GetAll()
                    .FirstOrDefaultAsync(x => x.ApartmentId == currentManagerApartment.Id && x.IsActive)
                : null;

            if (currentActiveManager != null)
            {
                currentActiveManager.EndDate = deactivationDate;
                currentActiveManager.IsActive = false;
                repository.Update(currentActiveManager);

                await ResetFutureDebts(currentActiveManager.ApartmentId, deactivationDate);
                await SetApartmentIsManagerAsync(currentActiveManager.ApartmentId, false);
            }

            var apartment = await apartmentRepo.GetAll().FirstOrDefaultAsync(x => x.Id == period.ApartmentId);
            if (apartment != null)
            {
                apartment.IsManager = true;
                apartmentRepo.Update(apartment);
            }
        }

        private async Task ExemptFutureDebts(ManagementPeriod period)
        {
            if (!period.IsExemptFromDues) return;

            // EndDate'in olduğu ay dahil muaf: EndDate 15 Haziran ise Haziran ayı tamamen muaf olmalı
            var effectiveEndDate = period.EndDate.HasValue
                ? new DateTime(period.EndDate.Value.Year, period.EndDate.Value.Month, DateTime.DaysInMonth(period.EndDate.Value.Year, period.EndDate.Value.Month))
                : new DateTime(Math.Max(period.StartDate.Year, DateTime.Now.Year), 12, 31);

            var futureDebts = await debtsRepo.GetAll()
                .Where(x => x.ApartmentId == period.ApartmentId &&
                            !x.IsClosed &&
                            x.DebtType != DebtType.TransferFromPast &&
                            x.DueDate >= period.StartDate &&
                            x.DueDate <= effectiveEndDate)
                .ToListAsync();

            foreach (var debt in futureDebts)
            {
                debt.Amount = 0;
                debt.Description = "Yönetici Muafiyeti (Otomatik)";
                debtsRepo.Update(debt);
            }
        }

        private async Task ResetFutureDebts(int apartmentId, DateTime fromDate)
        {
            var apartment = await apartmentRepo.GetByIdAsync(apartmentId);
            if (apartment == null) return;

            var futureDebts = await debtsRepo.GetAll()
                .Where(x => x.ApartmentId == apartmentId &&
                            !x.IsClosed &&
                            x.DueDate >= fromDate &&
                            x.DebtType != DebtType.TransferFromPast)
                .ToListAsync();

            var allDues = await dueSettingRepo.GetAll()
                .Where(x => x.IsActive)
                .ToListAsync();

            foreach (var debt in futureDebts)
            {
                var due = allDues.FirstOrDefault(d => d.StartDate <= debt.DueDate && d.EndDate >= debt.DueDate)
                    ?? allDues.FirstOrDefault(d => d.StartDate.Year == debt.DueDate.Year);
                if (due != null)
                {
                    debt.Amount = due.Amount;
                    debt.Description = "Aidat Borcu (Yöneticilik Sona Erdi)";
                    debtsRepo.Update(debt);
                }
            }
        }

        private async Task SetApartmentIsManagerAsync(int apartmentId, bool isManager)
        {
            var apartment = await apartmentRepo.GetByIdAsync(apartmentId);
            if (apartment == null) return;

            apartment.IsManager = isManager;
            apartmentRepo.Update(apartment);
        }

        /// <summary>
        /// Yönetim dönemi tarih/muafiyet değiştiğinde ApartmentDebts ile senkronize eder.
        /// Örn: EndDate Haziran'dan Aralık'a uzatıldı → Tem-Ara borçları muaf yapılır.
        /// EndDate Aralık'tan Haziran'a kısaltıldı → Tem-Ara borçları normale döner.
        /// </summary>
        private async Task SyncDebtsWithPeriodAsync(ManagementPeriod period, bool wasExempt, DateTime? oldEndDate)
        {
            if (period.IsExemptFromDues)
            {
                // Dönem aralığındaki borçları muaf yap (0 TL)
                await ExemptFutureDebts(period);

                // Dönem bitişinden sonraki borçları normale çevir (EndDate uzatıldığında önceden muaf olan aylar artık dönem dışı kalabilir)
                if (period.EndDate.HasValue)
                {
                    var firstDayAfterPeriod = period.EndDate.Value.Date.AddMonths(1);
                    var firstOfNextMonth = new DateTime(firstDayAfterPeriod.Year, firstDayAfterPeriod.Month, 1);
                    await ResetFutureDebts(period.ApartmentId, firstOfNextMonth);
                }
            }
            else
            {
                // Muafiyet kaldırıldı; dönem içindeki tüm borçları normale çevir
                await ResetFutureDebts(period.ApartmentId, period.StartDate);
            }
        }
    }
}

