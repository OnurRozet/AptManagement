using AptManagement.Application.Common.Base.Request;
using AptManagement.Application.Common.Base.Response;
using AptManagement.Application.Common;
using AptManagement.Application.Dtos;
using AptManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AptManagement.Application.Interfaces
{
    public interface IDuesSettingService
    {
        Task<ServiceResult<CreateOrEditResponse>> CreateOrEdit(DuesSettingDto request);
        Task<ServiceResult<SearchResponse<DuesSettingResponse>>> Search(DuesSettingSearch request);
        Task<ServiceResult<DetailResponse<DuesSettingResponse>>> GetDuesSettingById(int id);
        Task<bool> DeleteDuesSettingAsync(int id);
    }
}
