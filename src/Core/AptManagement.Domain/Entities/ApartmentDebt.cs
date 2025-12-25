using AptManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AptManagement.Domain.Entities
{
    public class ApartmentDebt : BaseEntity
    {
        public int ApartmentId { get; set; }
        public Apartment Apartment { get; set; } = null!;
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public string? Description { get; set; }
        public decimal PaidAmount { get; set; }
        public bool IsClosed { get; set; } //ödenip ödenmediğini kontrol edebiliriz
    }
}
    