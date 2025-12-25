using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AptManagement.Application.Common.Base.Response
{
    public class ApartmentDebtResponse
    {
        public int Id { get; set; }
        public int ApartmentId { get; set; }
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public string? Description { get; set; }
        public bool IsClosed { get; set; }
    }
}

