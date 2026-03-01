using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MISA.QLSX.Core.Exceptions
{
    /// <summary>
    /// 409 Trùng dữ liệu nào đó phải unique
    /// Created: TMHieu(04/12/2025)
    /// </summary>
    public class DuplicateException : BaseException
    {
        public DuplicateException(string devMsg, string? userMsg = null)
            : base(devMsg, userMsg ?? "Dữ liệu đã tồn tại trong hệ thống.") { }
    }
}
