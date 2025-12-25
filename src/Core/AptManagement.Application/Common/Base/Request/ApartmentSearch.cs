using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AptManagement.Application.Common.Base.Request
{
    public class ApartmentSearch : SearchRequest
    {
        public string TenantName { get; set; } = string.Empty;
    }
}
