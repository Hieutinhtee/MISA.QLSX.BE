using MISA.QLSX.Core.Entities;

namespace MISA.QLSX.Core.Interfaces.Service
{
    /// <summary>
    /// Service cho đơn xin nghỉ phép.
    /// </summary>
    public interface ILeaveRequestService : IBaseService<LeaveRequest>
    {
        /// <summary>
        /// Thu hồi đơn nghỉ phép.
        /// </summary>
        /// <param name="leaveRequestId">ID đơn nghỉ phép.</param>
        /// <param name="employeeId">ID nhân viên đang thực hiện thu hồi.</param>
        /// <returns>ID đơn nghỉ phép đã thu hồi.</returns>
        Task<Guid> WithdrawAsync(Guid leaveRequestId, Guid employeeId);
    }
}