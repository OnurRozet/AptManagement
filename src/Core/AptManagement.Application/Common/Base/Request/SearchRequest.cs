using AptManagement.Application.Common.Enums;
using AptManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AptManagement.Application.Common.Base.Request
{
    public class SearchRequest
    {
        public int? ApartmentId { get; set; }
        public int? IncomeId { get; set; }
        public int? ExpenseId { get; set; }
        public int Page { get; set; }
        public PageSizeEnums PageSize { get; set; }
    }
}
