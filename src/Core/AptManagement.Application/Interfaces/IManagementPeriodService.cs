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
    public interface IManagementPeriodService
    {
        Task<ServiceResult<CreateOrEditResponse>> CreateOrEdit(ManagementPeriodDto request);
        Task<ServiceResult<SearchResponse<ManagementPeriodResponse>>> Search(ManagementPeriodSearch request);
        Task<ServiceResult<DetailResponse<ManagementPeriodResponse>>> GetManagementPeriodById(int id);
        Task<ServiceResult<bool>> DeleteManagementPeriodAsync(int id);
    }
}

