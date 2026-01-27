using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AptManagement.Application.Common.Base.Response
{
    public class ManagementPeriodResponse
    {
        public int Id { get; set; }
        public int ApartmentId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsExemptFromDues { get; set; }
        public string? ApartmentLabel { get; set; }
        public string? ApartmentOwnerName { get; set; }
    }
}

