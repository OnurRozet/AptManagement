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
        DashboardSummaryDto GetGeneralStatusCardsAsync();
        Task<List<MonthlyTrendDto>> GetMonthlyTrendAsync();
        List<ExpenseDistributionDto> GetExpenseDistributionAsync();
    }
}
