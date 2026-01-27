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
    public interface IIncomeDebtAllocationService
    {
        Task<ServiceResult<CreateOrEditResponse>> CreateOrEdit(IncomeDebtAllocationDto request);
        Task<ServiceResult<SearchResponse<IncomeDebtAllocationResponse>>> Search(IncomeDebtAllocationSearch request);
        Task<ServiceResult<DetailResponse<IncomeDebtAllocationResponse>>> GetIncomeDebtAllocationById(int id);
        Task<ServiceResult<bool>> DeleteIncomeDebtAllocationAsync(int id);
    }
}

