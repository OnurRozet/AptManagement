using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AptManagement.Application.Dtos
{
    public class TopPayerDto
    {
        public string ApartmentLabel { get; set; } = string.Empty; // Örn: Daire 7
        public string OwnerName { get; set; } = string.Empty;      // Örn: Emine Erten
        public decimal TotalAmount { get; set; }                   // Ödediği Toplam Tutar
        public int TransactionCount { get; set; }                  // Kaç kere ödeme yapmış?
    }
}
