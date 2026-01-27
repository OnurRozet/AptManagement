using AptManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AptManagement.Domain.Entities
{
    public class IncomeDebtAllocation : BaseEntity
    {
        public int IncomeId { get; set; }
        public Income Income { get; set; } = null!;

        public int ApartmentDebtId { get; set; }
        public ApartmentDebt ApartmentDebt { get; set; } = null!;

        // Bu ödemeden, bu borca aktarılan tutar
        public decimal AllocatedAmount { get; set; }
    }
}
