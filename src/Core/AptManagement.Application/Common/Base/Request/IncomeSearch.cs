using AptManagement.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AptManagement.Application.Common.Base.Request
{
    public class IncomeSearch : SearchRequest
    {
        public decimal? Amount { get; set; }
        public string? Title { get; set; }
        public DateTime? IncomeDate { get; set; }
        public PaymentCategory? PaymentCategory { get; set; }
        public int? IncomeCategoryId { get; set; }
    }
}

