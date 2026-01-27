using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AptManagement.Application.Dtos
{
    public class IncomeDebtAllocationDto
    {
        public int Id { get; set; }
        public int IncomeId { get; set; }
        public int ApartmentDebtId { get; set; }
        public decimal AllocatedAmount { get; set; }
    }
}

