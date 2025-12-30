using AptManagement.Application.Common;
using AptManagement.Application.Common.Base.Request;
using AptManagement.Application.Common.Base.Response;
using AptManagement.Application.Dtos;
using AptManagement.Application.Dtos.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AptManagement.Application.Interfaces
{
    public interface IExpenseService
    {
        Task<ServiceResult<CreateOrEditResponse>> CreateOrEdit(ExpenseDto request);
        Task<ServiceResult<SearchResponse<ExpenseResponse>>> Search(ExpenseSearch request);
        Task<ServiceResult<DetailResponse<ExpenseResponse>>> GetExpenseById(int id);
        Task<bool> DeleteExpenseAsync(int id); 
        ServiceResult<ExpenseSummaryDto> GetSummaryExpenseReport();

    }
}

