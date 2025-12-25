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
    public class ApartmentDebtService(IRepository<ApartmentDebt> repository, IMapper mapper, IValidator<ApartmentDebt> validator) : IApartmentDebtService
    {
        public async Task<ServiceResult<CreateOrEditResponse>> CreateOrEdit(ApartmentDebtDto request)
        {
            var apartmentDebt = mapper.Map<ApartmentDebt>(request);
            //validasyon ekle
            var validationResult = validator.Validate(apartmentDebt);

            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                return ServiceResult<CreateOrEditResponse>.Error(errors);
            }

            if (apartmentDebt == null) return ServiceResult<CreateOrEditResponse>.Error();

            if (apartmentDebt.Id > 0) repository.Update(apartmentDebt);

            await repository.CreateAsync(apartmentDebt);

            return ServiceResult<CreateOrEditResponse>.Success(new CreateOrEditResponse { ID = apartmentDebt.Id }, "Başarılı şekilde oluşturulmuştur.");
        }

        public async Task<bool> DeleteApartmentDebtAsync(int id)
        {
            var apartmentDebt = await repository.GetByIdAsync(id);
            if (apartmentDebt == null) return false;
            repository.Delete(apartmentDebt);
            return true;
        }

        public async Task<ServiceResult<DetailResponse<ApartmentDebtResponse>>> GetApartmentDebtById(int id)
        {
            var apartmentDebt = await repository.GetByIdAsync(id);

            if (apartmentDebt == null)
                return ServiceResult<DetailResponse<ApartmentDebtResponse>>.Error("Belirtilen id ye sahip bir daire borcu bulunamadı");

            var apartmentDebtResponse = mapper.Map<ApartmentDebtResponse>(apartmentDebt);

            return ServiceResult<DetailResponse<ApartmentDebtResponse>>.Success(new DetailResponse<ApartmentDebtResponse> { Detail = apartmentDebtResponse });
        }

        public async Task<ServiceResult<SearchResponse<ApartmentDebtResponse>>> Search(ApartmentDebtSearch request)
        {
            var query = repository.GetAll();
            var filteredQuery = query.WhereIf(request.ApartmentId.HasValue && request.ApartmentId.Value > 0, x => x.ApartmentId == request.ApartmentId.Value)
                .WhereIf(request.Amount.HasValue && request.Amount.Value > 0, x => x.Amount == request.Amount.Value)
                .WhereIf(request.DueDate.HasValue, x => x.DueDate.Date == request.DueDate.Value.Date)
                .WhereIf(!string.IsNullOrEmpty(request.Description), x => x.Description == request.Description)
                .WhereIf(request.IsClosed.HasValue, x => x.IsClosed == request.IsClosed.Value)
                .Select(x => new ApartmentDebtResponse()
                {
                    Id = x.Id,
                    ApartmentId = x.ApartmentId,
                    Amount = x.Amount,
                    DueDate = x.DueDate,
                    Description = x.Description,
                    IsClosed = x.IsClosed
                })
                .OrderBy(x => x.Id)
                .ToPagedList(request.Page, (int)request.PageSize);

            return ServiceResult<SearchResponse<ApartmentDebtResponse>>.Success(new SearchResponse<ApartmentDebtResponse>
            {
                SearchResult = filteredQuery.ToList(),
                TotalItemCount = query.Count()
            });
        }
    }
}

