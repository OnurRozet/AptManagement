using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AptManagement.Application.Common.Base.Response
{
    public class SearchResponse<T>
    {
        public List<T> SearchResult { get; set; }
        public int TotalItemCount { get; set; }

        public SearchResponse()
        {
            SearchResult = [];
        }
    }
}
