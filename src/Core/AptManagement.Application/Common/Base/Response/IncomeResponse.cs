using AptManagement.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AptManagement.Application.Common.Base.Response
{
    public class IncomeResponse
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string? Title { get; set; }
        public DateTime IncomeDate { get; set; }
        public PaymentCategory PaymentCategory { get; set; }
        public int IncomeCategoryId { get; set; }

        public string IncomeCategory { get; set; } = string.Empty;
        public int ApartmentId { get; set; }
        public string ApartmentNo { get; set; } = string.Empty;
    }
}

