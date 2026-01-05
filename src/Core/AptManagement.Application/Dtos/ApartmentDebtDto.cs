using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AptManagement.Application.Dtos
{
    public class ApartmentDebtDto
    {
        public int Id { get; set; }
        public int ApartmentId { get; set; }
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public string? Description { get; set; }

        public decimal PaidAmount { get; set; }
        public bool IsClosed { get; set; }
    }
}

