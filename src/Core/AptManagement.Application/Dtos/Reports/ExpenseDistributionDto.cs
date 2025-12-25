using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AptManagement.Application.Dtos.Reports
{
    public class ExpenseDistributionDto
    {
        public string CategoryName { get; set; } = null!;
        public decimal TotalAmount { get; set; }
        public double Percentage { get; set; } // Yüzdelik dilim
    }
}
