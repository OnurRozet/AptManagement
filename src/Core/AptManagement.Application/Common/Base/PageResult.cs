using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AptManagement.Application.Common.Base
{
    public class PageResult<T>
    {
        public List<T>? ResultObject { get; set; }
        public PaginationModel? Paginator { get; set; }
        public static PageResult<T> Success(List<T> result, int page, int size, int total) => new() { ResultObject = result, Paginator = new() { PageNumber = page, PageSize = size, TotalItem = total } };
    }
}
