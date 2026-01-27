using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AptManagement.Application.Common.Base.Request
{
    public class ManagementPeriodSearch : SearchRequest
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool? IsExemptFromDues { get; set; }
        public string? Keyword { get; set; }
    }
}

