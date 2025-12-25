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
    public interface IIncomeService
    {
        Task<ServiceResult<CreateOrEditResponse>> CreateOrEdit(IncomeDto request);
        Task<ServiceResult<SearchResponse<IncomeResponse>>> Search(IncomeSearch request);
        Task<ServiceResult<DetailResponse<IncomeResponse>>> GetIncomeById(int id);
        Task<bool> DeleteIncomeAsync(int id);
    }
}

