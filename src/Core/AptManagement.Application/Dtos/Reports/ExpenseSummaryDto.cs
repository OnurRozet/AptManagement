using AptManagement.Application.Common.Base.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AptManagement.Application.Dtos.Reports
{
    public class ExpenseSummaryDto
    {
        public decimal TotalExpense { get; set; }
        public decimal TotalExpenseByCurrentMonth { get; set; }
        public int TotalItemCount { get; set; }
        public HighestFeeCategoryItem HighestFeeCategory { get; set; } = new HighestFeeCategoryItem();
    }
}
