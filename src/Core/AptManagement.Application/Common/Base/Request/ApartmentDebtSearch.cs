using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AptManagement.Application.Common.Base.Request
{
    public class ApartmentDebtSearch : SearchRequest
    {
        public decimal? Amount { get; set; }
        public DateTime? DueDate { get; set; }
        public string? Description { get; set; }
        public bool? IsClosed { get; set; }
    }
}

