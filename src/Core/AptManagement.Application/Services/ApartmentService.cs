using AptManagement.Application.Common;
using AptManagement.Application.Common.Base;
using AptManagement.Application.Common.Base.Request;
using AptManagement.Application.Common.Base.Response;
using AptManagement.Application.Dtos;
using AptManagement.Application.Extensions;
using AptManagement.Application.Interfaces;
using AptManagement.Domain.Entities;
using AptManagement.Domain.Interfaces;
using AutoMapper;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using X.PagedList.Extensions;

namespace AptManagement.Application.Services
{
    public class ApartmentService(IRepository<Apartment> repository, IMapper mapper, IValidator<Apartment> validator) : IApartmentService
    {
        public async Task<ServiceResult<CreateOrEditResponse>> CreateOrEdit(ApartmentDto apartment)
        {
            Apartment newApartment = mapper.Map<Apartment>(apartment);
            //validasyon ekle
            var validationResult = validator.Validate(newApartment);

            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                return ServiceResult<CreateOrEditResponse>.Error(errors);
            }

            if (newApartment == null) return ServiceResult<CreateOrEditResponse>.Error();

            if (newApartment.Id > 0)
            {
                repository.Update(newApartment);
                return ServiceResult<CreateOrEditResponse>.Success(new CreateOrEditResponse { ID = newApartment.Id }, "Başarılı şekilde güncellenmiştir.");
            }

            await repository.CreateAsync(newApartment);

            return ServiceResult<CreateOrEditResponse>.Success(new CreateOrEditResponse { ID = newApartment.Id }, "Başarılı şekilde oluşturulmuştur.");
        }

        public async Task<bool> DeleteApartmentAsync(int id)
        {
            var apartment = await repository.GetByIdAsync(id);
            if (apartment == null) return false;
            repository.Delete(apartment);
            return true;
        }

        public async Task<ServiceResult<SearchResponse<ApartmentResponse>>> Search(ApartmentSearch request)
        {
            var query = repository.GetAll();
            var filteredQuery = query.WhereIf(request.ApartmentId.HasValue, x => x.Id == request.ApartmentId.Value)
                .WhereIf(!string.IsNullOrEmpty(request.TenantName), x => x.TenantName == request.TenantName)
                .Select(x => new ApartmentResponse()
                {
                    Id = x.Id,
                    Label = x.Label,
                    OwnerName = x.OwnerName,
                    TenantName = x.TenantName,
                    Balance = x.Balance,
                })
                .OrderBy(x => x.Id)
                .ToPagedList(request.Page, (int)request.PageSize);

            return ServiceResult<SearchResponse<ApartmentResponse>>.Success(new SearchResponse<ApartmentResponse>
            {
                SearchResult = filteredQuery.ToList(),
                TotalItemCount = query.Count()
            });
        }

        public async Task<ServiceResult<DetailResponse<Apartment>>> GetApartmentById(int id)
        {
            var apartment = await repository.GetByIdAsync(id);

            if (apartment == null) return ServiceResult<DetailResponse<Apartment>>.Error("Belirtilen id ye sahip bir daire bulunamadı");

            return ServiceResult<DetailResponse<Apartment>>.Success(new DetailResponse<Apartment> { Detail = apartment });
        }

        //public Task<ServiceResult<PageResult<Apartment>>> GetApartmentWithPagination(int page, int pageSize)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
