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

            if (string.IsNullOrWhiteSpace(roleCode))
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
                return;
            }

            if (normalizedRole == "EMPLOYEE" && IsReadOnlyEmployeeController(context))
            {
                return;
            }

            if (normalizedRole == "EMPLOYEE" && IsReadOnlyEmployeeController(context) == false)
            {
                context.Result = new JsonResult(new { message = "EMPLOYEE chỉ được xem dữ liệu của bản thân" })
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                };
            }
        }

        /// <summary>
        /// Kiểm tra action hiện tại có thuộc nhóm controller chỉ cho EMPLOYEE truy cập các action đọc hay không.
        /// </summary>
        /// <param name="context">Ngữ cảnh authorization hiện tại.</param>
        /// <returns>True nếu action là read-only cho EMPLOYEE.</returns>
        private static bool IsReadOnlyEmployeeController(AuthorizationFilterContext context)
        {
            var controller = context.ActionDescriptor.RouteValues.TryGetValue("controller", out var controllerName)
                ? controllerName?.Trim().ToUpperInvariant()
                : null;

            var action = context.ActionDescriptor.RouteValues.TryGetValue("action", out var actionName)
                ? actionName?.Trim().ToUpperInvariant()
                : null;

            if (string.IsNullOrWhiteSpace(controller) || string.IsNullOrWhiteSpace(action))
            {
                return true;
            }

            return controller switch
            {
                "EMPLOYEES" => action is "GETALL" or "GETBYID" or "GETPAGING",
                "PAYROLLS" => action is "GETALL" or "GETBYID" or "GETPAGING",
                "ATTENDANCES" => action is "GETALL" or "GETBYID" or "GETPAGING" or "GETEMPLOYEECALENDAR",
                _ => true,
            };
        }
    }
}
