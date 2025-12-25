using AptManagement.Application.Common;
using AptManagement.Application.Common.Base.Request;
using AptManagement.Application.Common.Base.Response;
using AptManagement.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AptManagement.Application.Interfaces
{
    public interface IApartmentDebtService
    {
        Task<ServiceResult<CreateOrEditResponse>> CreateOrEdit(ApartmentDebtDto request);
        Task<ServiceResult<SearchResponse<ApartmentDebtResponse>>> Search(ApartmentDebtSearch request);
        Task<ServiceResult<DetailResponse<ApartmentDebtResponse>>> GetApartmentDebtById(int id);
        Task<bool> DeleteApartmentDebtAsync(int id);
    }
}

