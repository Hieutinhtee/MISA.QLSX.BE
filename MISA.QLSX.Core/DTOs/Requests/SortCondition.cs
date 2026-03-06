using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MISA.QLSX.Core.DTOs.Requests
{
    public class SortCondition
    {
        public string Field { get; set; } = "";
        public string Direction { get; set; } = "ASC";
    }
}
