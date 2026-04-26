using Microsoft.AspNetCore.Mvc;

namespace MISA.QLSX.Api.Authorization
{
    /// <summary>
    /// Attribute yêu cầu người dùng đã đăng nhập và có role phù hợp.
    /// </summary>
    public class RequireRoleAttribute : TypeFilterAttribute
    {
        /// <summary>
        /// Khởi tạo attribute với danh sách role được phép.
        /// </summary>
        /// <param name="roles">Danh sách role code hợp lệ, ví dụ: ADMIN, HR.</param>
        public RequireRoleAttribute(params string[] roles)
            : base(typeof(RequireRoleFilter))
        {
            Arguments = new object[] { roles };
        }
    }
}
