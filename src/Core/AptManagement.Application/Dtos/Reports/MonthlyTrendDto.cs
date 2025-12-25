using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AptManagement.Application.Dtos.Reports
{
    public class MonthlyTrendDto
    {
        public string MonthName { get; set; } = null!; // Örn: "Ocak 2025"
        public decimal TotalIncome { get; set; }
        public decimal TotalExpense { get; set; }
    }
}
