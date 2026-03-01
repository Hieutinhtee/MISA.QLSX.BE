using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MISA.QLSX.Core.Exceptions
{
    /// <summary>
    /// 400 Dữ liệu không hợp lệ
    /// Created by: TMHieu(04/12/2025)
    /// </summary>
    public class ValidateException : BaseException
    {
        public ValidateException(string devMsg, string? userMsg = null)
            : base(devMsg, userMsg ?? "Dữ liệu bạn nhập chưa hợp lệ.") { }
    }
}
