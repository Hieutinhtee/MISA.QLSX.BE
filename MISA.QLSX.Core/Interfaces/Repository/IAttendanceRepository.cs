using MISA.QLSX.Core.Entities;

namespace MISA.QLSX.Core.Interfaces.Repository
{
    /// <summary>
    /// Định nghĩa truy cập dữ liệu chấm công.
    /// </summary>
    public interface IAttendanceRepository : IBaseRepository<Attendance>
    {
        /// <summary>
        /// Lấy bản ghi chấm công theo danh sách nhân viên trong khoảng ngày chỉ định.
        /// </summary>
        /// <param name="employeeIds">Danh sách định danh nhân viên cần truy vấn.</param>
        /// <param name="periodStart">Ngày bắt đầu khoảng lọc.</param>
        /// <param name="periodEnd">Ngày kết thúc khoảng lọc.</param>
        /// <returns>Danh sách bản ghi chấm công thỏa điều kiện lọc.</returns>
        Task<List<Attendance>> GetByEmployeesAndDateRangeAsync(
            List<Guid> employeeIds,
            DateTime periodStart,
            DateTime periodEnd
        );
    }
}