namespace MISA.QLSX.Core.DTOs.Responses
{
    /// <summary>
    /// DTO trung gian chứa dữ liệu account/role/employee phục vụ đăng nhập.
    /// </summary>
    public class AuthAccountInfo
    {
        /// <summary>
        /// ID tài khoản.
        /// </summary>
        public Guid? AccountId { get; set; }

        /// <summary>
        /// Username đăng nhập.
        /// </summary>
        public string? Username { get; set; }

        /// <summary>
        /// Chuỗi hash mật khẩu.
        /// </summary>
        public string? PasswordHash { get; set; }

        /// <summary>
        /// Trạng thái active của tài khoản.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Mã vai trò.
        /// </summary>
        public string? RoleCode { get; set; }

        /// <summary>
        /// Tên vai trò.
        /// </summary>
        public string? RoleName { get; set; }

        /// <summary>
        /// ID nhân viên liên kết.
        /// </summary>
        public Guid? EmployeeId { get; set; }

        /// <summary>
        /// Tên nhân viên.
        /// </summary>
        public string? FullName { get; set; }

        /// <summary>
        /// ID phòng ban của nhân viên.
        /// </summary>
        public Guid? DepartmentId { get; set; }
    }
}
