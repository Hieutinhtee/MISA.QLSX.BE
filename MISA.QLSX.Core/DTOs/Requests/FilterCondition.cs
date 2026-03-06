using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MISA.QLSX.Core.DTOs.Requests
{
    public class FilterCondition
    {
        public string Field { get; set; } = "";
        public string Operator { get; set; } = "";
        public object? Value { get; set; }
    }
}
