namespace MISA.QLSX.Core.DTOs.Responses
{
    /// <summary>
    /// DTO cho phản hồi đăng nhập
    /// </summary>
    public class LoginResponse
    {
        /// <summary>
        /// ID tài khoản
        /// </summary>
        public Guid? AccountId { get; set; }

        /// <summary>
        /// Mã vai trò
        /// </summary>
        public string? Role { get; set; }

        /// <summary>
        /// Tên nhân viên
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// ID nhân viên
        /// </summary>
        public Guid? EmployeeId { get; set; }

        /// <summary>
        /// ID phòng ban
        /// </summary>
        public Guid? DepartmentId { get; set; }
    }
}
