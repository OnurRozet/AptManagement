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
    public interface IIncomeCategoryService
    {
        Task<ServiceResult<CreateOrEditResponse>> CreateOrEdit(IncomeCategoryDto request);
        Task<ServiceResult<SearchResponse<IncomeCategoryResponse>>> Search(IncomeCategorySearch request);
        Task<ServiceResult<DetailResponse<IncomeCategoryResponse>>> GetIncomeCategoryById(int id);
        Task<bool> DeleteIncomeCategoryAsync(int id);
    }
}

