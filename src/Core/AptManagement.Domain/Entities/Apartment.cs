using AptManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AptManagement.Domain.Entities
{
    public class Apartment : BaseEntity
    {
        public required string Label { get; set; }
        public required string OwnerName { get; set; }
        public  string? TenantName { get; set; }
        public decimal Balance { get; set; } = 0;
        public ICollection<Income> Incomes { get; set; } = [];
        public ICollection<ApartmentDebt> Debts { get; set; } = [];
        // Genelde bir dairede birden fazla kişi (AppUser) tanımlı olabilir (Eşler vb.)
        public ICollection<AppUser> Residents { get; set; }
    }
}
