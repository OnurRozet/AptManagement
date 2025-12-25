using AptManagement.Domain.Common;
using AptManagement.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AptManagement.Domain.Entities
{
    public class Expense : BaseEntity
    {
        public string? Title { get; set; } //Kısa Açıklama
        public decimal Amount { get; set; }
        public DateTime ExpenseDate { get; set; }
        public PaymentCategory PaymentCategory { get; set; }
        public int ExpenseCategoryId { get; set; }
        public ExpenseCategory ExpenseCategory { get; set; } = null!;
    }
}
