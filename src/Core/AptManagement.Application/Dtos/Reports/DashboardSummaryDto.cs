using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AptManagement.Application.Dtos.Reports
{
    public class DashboardSummaryDto
    {
        public decimal TotalIncome { get; set; } // Kasadaki Net Para
        public decimal ExpectedIncome { get; set; }    // Beklenen Toplam Alacak
        public decimal TotalExpense { get; set; }      // Gecikmiş (Vadesi Geçmiş) Borç
        public int ActiveDebtorsCount { get; set; }     // Borcu olan daire sayısı
    }
}
