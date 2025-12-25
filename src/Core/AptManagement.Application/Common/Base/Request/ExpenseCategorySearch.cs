using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AptManagement.Application.Common.Base.Request
{
    public class ExpenseCategorySearch : SearchRequest
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
    }
}

