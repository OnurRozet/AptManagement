using AptManagement.Application.Common;
using AptManagement.Application.Common.Base;
using AptManagement.Application.Common.Base.Request;
using AptManagement.Application.Common.Base.Response;
using AptManagement.Application.Dtos;
using AptManagement.Domain.Entities;
using Microsoft.AspNetCore.Http;
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
        Task<ServiceResult<CreateOrEditResponse>> CreateApartmentsBulkAsync(List<ApartmentDto> request);
        Task<ServiceResult<SearchResponse<ApartmentResponse>>> Search(ApartmentSearch request);
        Task<ServiceResult<DetailResponse<Apartment>>> GetApartmentById(int id);
        Task<bool> DeleteApartmentAsync(int id);
        Task<ServiceResult<bool>> SetOpeningBalanceAsync(int apartmentId, decimal amount);
        Task<ServiceResult<List<ApartmentDto>>> ParseApartmentExcelAsync(IFormFile file);
    }
}
