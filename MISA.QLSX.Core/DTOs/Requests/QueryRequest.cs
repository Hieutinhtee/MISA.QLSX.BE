using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MISA.QLSX.Core.DTOs.Requests
{
    public class QueryRequest
    {
        public int? Page { get; set; }
        public int? PageSize { get; set; }

        public string? Search { get; set; }

        public List<FilterCondition>? Filters { get; set; }
        public List<SortCondition>? Sorts { get; set; }
    }
}
