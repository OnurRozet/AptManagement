using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AptManagement.Application.Dtos
{
    public class BankTransactionDto
    {
        public DateTime Date { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; } // Orijinal tutar (- veya +)

        // İşlenmiş Veriler
        public string TransactionType { get; set; } // "Gelir" veya "Gider"
        public string? SuggestedCategory { get; set; } // Örn: "Elektrik", "Temizlik", "Aidat"
        public int? MatchedApartmentId { get; set; } // Gelir ise hangi daire?
        public bool IsProcessed { get; set; } = false; // Veritabanına kaydedildi mi?
    }
}
