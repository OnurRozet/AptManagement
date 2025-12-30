using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AptManagement.Application.Dtos.Reports
{
    public class IncomeSummaryDto
    {
        public decimal TotalIncome { get; set; }
        public decimal TotalIncomeByCurrentMonth { get; set; }
        public int TotalItemCount { get; set; }
        public TopPayerDto HighestApartmentFeeRevenue { get; set; } = new TopPayerDto();
        public List<TopPayerDto> MostRegularPayer { get; set; } = [];
    }
}
