using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AptManagement.Application.Common.Base.Response
{
    public class HighestFeeCategoryItem
    {
        public decimal TotalAmount { get; set; }
        public string? ExpenseCategoryName { get; set; }
    }
}
