using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MISA.QLSX.Core.DTOs.Requests
{
    public class FieldMapItem
    {
        public string Column { get; set; } = "";
        public Type DataType { get; set; } = typeof(string);
        public HashSet<string> Operators { get; set; } = new();
    }
}
