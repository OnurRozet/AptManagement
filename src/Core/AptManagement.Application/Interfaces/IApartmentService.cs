using AptManagement.Application.Common;
using AptManagement.Application.Common.Base;
using AptManagement.Application.Common.Base.Request;
using AptManagement.Application.Common.Base.Response;
using AptManagement.Application.Dtos;
using AptManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AptManagement.Application.Interfaces
{
    public interface IApartmentService
    {
        Task<ServiceResult<CreateOrEditResponse>> CreateOrEdit(ApartmentDto request);
        Task<ServiceResult<SearchResponse<ApartmentResponse>>> Search(ApartmentSearch request);
        Task<ServiceResult<DetailResponse<Apartment>>> GetApartmentById(int id);
        Task<bool> DeleteApartmentAsync(int id);
        //Task<ServiceResult<PageResult<Apartment>>> GetApartmentWithPagination(int page, int pageSize);
    }
}
