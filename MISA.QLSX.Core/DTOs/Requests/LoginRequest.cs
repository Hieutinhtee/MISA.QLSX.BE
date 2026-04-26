namespace MISA.QLSX.Core.DTOs.Requests
{
    /// <summary>
    /// DTO cho yêu cầu đăng nhập
    /// </summary>
    public class LoginRequest
    {
        /// <summary>
        /// Tên đăng nhập
        /// </summary>
        public string? Username { get; set; }

        /// <summary>
        /// Mật khẩu
        /// </summary>
        public string? Password { get; set; }
    }
}
