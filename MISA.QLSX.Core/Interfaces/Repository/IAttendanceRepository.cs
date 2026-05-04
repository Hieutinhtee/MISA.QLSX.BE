using MISA.QLSX.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        Task<List<Attendance>> GetByEmployeesAndDateRangeAsync(List<Guid> employeeIds, DateTime periodStart, DateTime periodEnd);

        /// <summary>
        /// Lấy danh sách chấm công trong một ngày (tất cả nhân viên)
        /// </summary>
        Task<List<Attendance>> GetAttendancesByDateAsync(DateTime date);

        /// <summary>
        /// Lấy danh sách chấm công của một nhân viên trong tháng
        /// </summary>
        Task<List<Attendance>> GetAttendancesByEmployeeInMonthAsync(Guid employeeId, int month, int year);

        /// <summary>
        /// Lấy danh sách nhân viên vắng mặt hôm nay
        /// </summary>
        Task<IEnumerable<dynamic>> GetAbsentEmployeesTodayAsync(DateTime date);

        /// <summary>
        /// Lấy bảng xếp hạng đi muộn trong tháng
        /// </summary>
        Task<IEnumerable<dynamic>> GetLateRankingsAsync(int month, int year);
    }
}