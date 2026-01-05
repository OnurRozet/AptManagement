using AptManagement.Application.Common;
using AptManagement.Application.Dtos.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AptManagement.Application.Interfaces
{
    public interface IReportService
    {
        ServiceResult<DashboardSummaryDto> GetGeneralStatusCardsAsync();
        Task<ServiceResult<List<MonthlyTrendDto>>> GetMonthlyTrendAsync();
        ServiceResult<List<ExpenseDistributionDto>> GetExpenseDistributionAsync();
    }
}
