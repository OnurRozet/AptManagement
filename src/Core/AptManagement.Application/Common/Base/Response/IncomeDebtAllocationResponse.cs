using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AptManagement.Application.Common.Base.Response
{
    public class IncomeDebtAllocationResponse
    {
        public int Id { get; set; }
        public int IncomeId { get; set; }
        public int ApartmentDebtId { get; set; }
        public decimal AllocatedAmount { get; set; }
        public string? IncomeTitle { get; set; }
        public decimal IncomeAmount { get; set; }
        public DateTime? IncomeDate { get; set; }
        public string? ApartmentLabel { get; set; }
        public DateTime? DebtDueDate { get; set; }
        public decimal? DebtAmount { get; set; }
    }
}

