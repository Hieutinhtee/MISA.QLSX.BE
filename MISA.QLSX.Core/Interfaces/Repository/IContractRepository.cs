using MISA.QLSX.Core.Entities;

namespace MISA.QLSX.Core.Interfaces.Repository
{
    /// <summary>
    /// Định nghĩa truy cập dữ liệu hợp đồng lao động.
    /// </summary>
    public interface IContractRepository : IBaseRepository<Contract>
    {
        /// <summary>
        /// Lấy các hợp đồng còn hiệu lực theo danh sách nhân viên trong một khoảng thời gian kỳ lương.
        /// </summary>
        /// <param name="employeeIds">Danh sách định danh nhân viên.</param>
        /// <param name="periodStart">Ngày bắt đầu kỳ lương.</param>
        /// <param name="periodEnd">Ngày kết thúc kỳ lương.</param>
        /// <returns>Danh sách hợp đồng hiệu lực trong kỳ lương.</returns>
        Task<List<Contract>> GetEffectiveByEmployeesAsync(
            List<Guid> employeeIds,
            DateTime periodStart,
            DateTime periodEnd
        );

        Task<List<Allowance>> GetAllowancesByContractIdAsync(Guid contractId);
    }
}
