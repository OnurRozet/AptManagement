using AptManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AptManagement.Domain.Entities
{
    public class IncomeCategory : BaseEntity
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public ICollection<Income> Incomes { get; set; } = [];
    }
}
