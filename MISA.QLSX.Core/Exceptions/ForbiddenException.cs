namespace MISA.QLSX.Core.Exceptions
{
    /// <summary>
    /// 403 Không có quyền truy cập tài nguyên.
    /// </summary>
    public class ForbiddenException : BaseException
    {
        /// <summary>
        /// Khởi tạo ngoại lệ 403.
        /// </summary>
        /// <param name="devMsg">Thông điệp kỹ thuật để debug.</param>
        /// <param name="userMsg">Thông điệp thân thiện hiển thị cho người dùng.</param>
        public ForbiddenException(string devMsg, string? userMsg = null)
            : base(devMsg, userMsg ?? "Bạn không có quyền truy cập chức năng này.") { }
    }
}
