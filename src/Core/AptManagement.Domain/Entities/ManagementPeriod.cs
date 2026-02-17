using AptManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AptManagement.Domain.Entities
{
    public class ManagementPeriod : BaseEntity
    {
        public int ApartmentId { get; set; }
        public Apartment Apartment { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; } // Null ise yıl sonuna kadar muaf
        public bool IsExemptFromDues { get; set; } = true; // Aidattan muaf mı?
        public bool IsActive { get; set; } = true;
    }
}
