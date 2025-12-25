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
    public interface IExpenseCategoryService
    {
        Task<ServiceResult<CreateOrEditResponse>> CreateOrEdit(ExpenseCategoryDto request);
        Task<ServiceResult<SearchResponse<ExpenseCategoryResponse>>> Search(ExpenseCategorySearch request);
        Task<ServiceResult<DetailResponse<ExpenseCategoryResponse>>> GetExpenseCategoryById(int id);
        Task<bool> DeleteExpenseCategoryAsync(int id);
    }
}

