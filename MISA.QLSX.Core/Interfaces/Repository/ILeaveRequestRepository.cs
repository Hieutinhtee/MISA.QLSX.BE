using MISA.QLSX.Core.Entities;

namespace MISA.QLSX.Core.Interfaces.Repository
{
    /// <summary>
    /// Repository cho đơn xin nghỉ phép.
    /// </summary>
    public interface ILeaveRequestRepository : IBaseRepository<LeaveRequest>
    {
        /// <summary>
        /// Kiểm tra đơn nghỉ phép có bị chồng khoảng ngày hay không.
        /// </summary>
        /// <param name="employeeId">ID nhân viên.</param>
        /// <param name="startDate">Ngày bắt đầu.</param>
        /// <param name="returnDate">Ngày kết thúc.</param>
        /// <param name="ignoreId">ID đơn cần bỏ qua.</param>
        /// <returns>True nếu có đơn bị chồng ngày.</returns>
        Task<bool> HasOverlappingRequestAsync(
            Guid employeeId,
            DateTime startDate,
            DateTime returnDate,
            Guid? ignoreId = null
        );

        /// <summary>
        /// Sinh mã đơn nghỉ phép tiếp theo.
        /// </summary>
        /// <returns>Mã đơn nghỉ phép.</returns>
        Task<string> GenerateLeaveRequestCodeAsync();
    }
}