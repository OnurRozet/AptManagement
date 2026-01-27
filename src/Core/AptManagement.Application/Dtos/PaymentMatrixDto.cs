using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AptManagement.Application.Dtos
{
    public class PaymentMatrixDto
    {
        public int ApartmentId { get; set; }
        public string ApartmentLabel { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public bool IsManager { get; set; } = false;
        public decimal Amount { get; set; }

        // Her ay için ödenen toplam tutar
        public decimal Jan { get; set; }
        public decimal Feb { get; set; }
        public decimal Mar { get; set; }
        public decimal Apr { get; set; }
        public decimal May { get; set; }
        public decimal Jun { get; set; }
        public decimal Jul { get; set; }
        public decimal Aug { get; set; }
        public decimal Sep { get; set; }
        public decimal Oct { get; set; }
        public decimal Nov { get; set; }
        public decimal Dec { get; set; }

        public decimal TotalYearlyPaid => Jan + Feb + Mar + Apr + May + Jun + Jul + Aug + Sep + Oct + Nov + Dec;
        public decimal TotalYearlyDebt { get; set; }
        public decimal TransferredDebt { get; set; }  // Geçmişten gelen toplam borç
    }
}
