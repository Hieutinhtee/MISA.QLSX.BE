namespace MISA.QLSX.Core.Exceptions
{
    /// <summary>
    /// 401 Chưa đăng nhập hoặc thông tin xác thực không hợp lệ.
    /// </summary>
    public class UnauthorizedException : BaseException
    {
        /// <summary>
        /// Khởi tạo ngoại lệ 401.
        /// </summary>
        /// <param name="devMsg">Thông điệp kỹ thuật để debug.</param>
        /// <param name="userMsg">Thông điệp thân thiện hiển thị cho người dùng.</param>
        public UnauthorizedException(string devMsg, string? userMsg = null)
            : base(devMsg, userMsg ?? "Bạn cần đăng nhập để tiếp tục.") { }
    }
}
