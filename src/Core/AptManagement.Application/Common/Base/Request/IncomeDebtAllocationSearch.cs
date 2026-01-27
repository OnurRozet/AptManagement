using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AptManagement.Application.Common.Base.Request
{
    public class IncomeDebtAllocationSearch : SearchRequest
    {
        public int? IncomeId { get; set; }
        public int? ApartmentDebtId { get; set; }
        public decimal? AllocatedAmount { get; set; }
        public string? Keyword { get; set; }
    }
}

