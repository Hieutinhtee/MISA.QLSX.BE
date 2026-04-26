using Microsoft.AspNetCore.Mvc;
using MISA.QLSX.Core.DTOs.Requests;
using MISA.QLSX.Core.Interfaces.Service;

namespace MISA.QLSX.Api.Controllers
{
    /// <summary>
    /// Controller xử lý xác thực đăng nhập/đăng xuất.
    /// </summary>
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAccountService _accountService;

        /// <summary>
        /// Khởi tạo AuthController.
        /// </summary>
        /// <param name="accountService">Service xử lý nghiệp vụ tài khoản.</param>
        public AuthController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        /// <summary>
        /// Đăng nhập hệ thống bằng username/password.
        /// </summary>
        /// <param name="request">Thông tin đăng nhập.</param>
        /// <returns>Thông tin người dùng sau khi đăng nhập thành công.</returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _accountService.LoginAsync(request);

            HttpContext.Session.SetString("account_id", result?.AccountId?.ToString() ?? string.Empty);
            HttpContext.Session.SetString("role_code", result?.Role ?? string.Empty);
            HttpContext.Session.SetString("employee_id", result?.EmployeeId?.ToString() ?? string.Empty);
            HttpContext.Session.SetString(
                "department_id",
                result?.DepartmentId?.ToString() ?? string.Empty
            );

            return Ok(
                new
                {
                    role = result?.Role,
                    name = result?.Name,
                    employee_id = result?.EmployeeId,
                    department_id = result?.DepartmentId,
                }
            );
        }

        /// <summary>
        /// Đăng xuất và xóa dữ liệu phiên hiện tại.
        /// </summary>
        /// <returns>Kết quả đăng xuất.</returns>
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Ok(new { message = "Đăng xuất thành công" });
        }
    }
}
