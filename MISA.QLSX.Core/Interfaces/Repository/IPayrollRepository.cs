using MISA.QLSX.Core.Entities;

namespace MISA.QLSX.Core.Interfaces.Repository
{
    /// <summary>
    /// Định nghĩa truy cập dữ liệu bảng lương.
    /// </summary>
    public interface IPayrollRepository : IBaseRepository<Payroll>
    {
        /// <summary>
        /// Lấy danh sách bảng lương theo kỳ lương và tùy chọn lọc theo nhân viên.
        /// </summary>
        /// <param name="salaryPeriodId">Định danh kỳ lương cần truy vấn.</param>
        /// <param name="employeeId">Định danh nhân viên cần lọc, để trống nếu lấy toàn bộ nhân viên trong kỳ.</param>
        /// <returns>Danh sách bảng lương thỏa điều kiện lọc.</returns>
        Task<List<Payroll>> GetBySalaryPeriodAsync(Guid salaryPeriodId, Guid? employeeId = null);
    }
}