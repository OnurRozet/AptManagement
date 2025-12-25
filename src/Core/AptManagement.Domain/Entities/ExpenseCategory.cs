using AptManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AptManagement.Domain.Entities
{
    public class ExpenseCategory : BaseEntity
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public ICollection<Expense> Expenses { get; set; } = [];
    }
}
