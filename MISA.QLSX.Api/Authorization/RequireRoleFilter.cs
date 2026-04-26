using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MISA.QLSX.Api.Authorization
{
    /// <summary>
    /// Filter kiểm tra Session đăng nhập và quyền truy cập theo role.
    /// </summary>
    public class RequireRoleFilter : IAuthorizationFilter
    {
        private readonly HashSet<string> _allowedRoles;

        /// <summary>
        /// Khởi tạo filter với danh sách role được phép.
        /// </summary>
        /// <param name="allowedRoles">Danh sách role code hợp lệ.</param>
        public RequireRoleFilter(params string[] allowedRoles)
        {
            _allowedRoles = allowedRoles
                .Where(role => !string.IsNullOrWhiteSpace(role))
                .Select(role => role.Trim().ToUpperInvariant())
                .ToHashSet();
        }

        /// <summary>
        /// Kiểm tra quyền truy cập trước khi action được thực thi.
        /// </summary>
        /// <param name="context">Ngữ cảnh authorization hiện tại.</param>
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var roleCode = context.HttpContext.Session.GetString("role_code");
            var employeeId = context.HttpContext.Session.GetString("employee_id");

            if (string.IsNullOrWhiteSpace(roleCode) || string.IsNullOrWhiteSpace(employeeId))
            {
                context.Result = new JsonResult(
                    new { message = "Bạn không có quyền thực hiện chức năng này" }
                )
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                };
                return;
            }

            if (_allowedRoles.Count == 0)
            {
                return;
            }

            var normalizedRole = roleCode.Trim().ToUpperInvariant();
            if (!_allowedRoles.Contains(normalizedRole))
            {
                context.Result = new JsonResult(new { message = "Bạn không có quyền truy cập" })
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                };
            }
        }
    }
}
