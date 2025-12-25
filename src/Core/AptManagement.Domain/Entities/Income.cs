using AptManagement.Domain.Common;
using AptManagement.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AptManagement.Domain.Entities
{
    public class Income : BaseEntity
    {
        public decimal Amount { get; set; }
        public string? Title { get; set; } // kısa açıklama 
        public DateTime IncomeDate { get; set; }
        public PaymentCategory PaymentCategory { get; set; }
        public int IncomeCategoryId { get; set; }
        public ICollection<IncomeCategory> IncomeCategories { get; set; } = [];
        public int ApartmentId { get; set; }
        public Apartment Apartment { get; set; } = null!;
    }
}
