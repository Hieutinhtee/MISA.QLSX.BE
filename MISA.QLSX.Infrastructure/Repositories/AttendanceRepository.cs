using MISA.QLSX.Core.DTOs.Requests;
using MISA.QLSX.Core.Entities;
using MISA.QLSX.Core.Interfaces.Repository;
using MISA.QLSX.Infrastructure.Connection;
using Dapper;

namespace MISA.QLSX.Infrastructure.Repositories
{
    /// <summary>
    /// Cung cấp truy cập dữ liệu chấm công và các truy vấn theo nhân viên, khoảng thời gian.
    /// </summary>
    public class AttendanceRepository : BaseRepository<Attendance>, IAttendanceRepository
    {
        /// <summary>
        /// Khởi tạo repository chấm công với factory tạo kết nối MySQL.
        /// </summary>
        /// <param name="factory">Factory tạo kết nối cơ sở dữ liệu.</param>
        public AttendanceRepository(MySqlConnectionFactory factory)
            : base(factory) { }

        /// <summary>
        /// Trả về danh sách các cột được dùng cho tìm kiếm nhanh dữ liệu chấm công.
        /// </summary>
        /// <returns>Tập tên cột hỗ trợ tìm kiếm.</returns>
        protected override HashSet<string> GetSearchFields()
        {
            return new HashSet<string> { "attendance_code" };
        }

        protected override Dictionary<string, FieldMapItem> FieldMap =>
            new()
            {
                ["attendanceCode"] = new() { Column = "attendance_code", DataType = typeof(string), Operators = new() { "eq", "neq", "contains", "notcontains", "starts", "ends" } },
                ["employeeId"] = new() { Column = "employee_id", DataType = typeof(Guid), Operators = new() { "eq", "neq" } },
                ["shiftId"] = new() { Column = "shift_id", DataType = typeof(Guid), Operators = new() { "eq", "neq", "isnull", "notnull" } },
                ["attendanceDate"] = new() { Column = "attendance_date", DataType = typeof(DateTime), Operators = new() { "eq", "lt", "lte", "gt", "gte" } },
                ["workingHours"] = new() { Column = "working_hours", DataType = typeof(decimal), Operators = new() { "eq", "lt", "lte", "gt", "gte", "neq" } },
                ["overtimeHours"] = new() { Column = "overtime_hours", DataType = typeof(decimal), Operators = new() { "eq", "lt", "lte", "gt", "gte", "neq" } },
                ["penaltyAmount"] = new() { Column = "penalty_amount", DataType = typeof(decimal), Operators = new() { "eq", "lt", "lte", "gt", "gte", "neq" } },
                ["bonusAmount"] = new() { Column = "bonus_amount", DataType = typeof(decimal), Operators = new() { "eq", "lt", "lte", "gt", "gte", "neq" } },
                ["netIncome"] = new() { Column = "net_income", DataType = typeof(decimal), Operators = new() { "eq", "lt", "lte", "gt", "gte", "neq" } },
            };

        /// <summary>
        /// Lấy bản ghi chấm công theo danh sách nhân viên trong khoảng ngày chỉ định.
        /// </summary>
        /// <param name="employeeIds">Danh sách định danh nhân viên cần truy vấn.</param>
        /// <param name="periodStart">Ngày bắt đầu khoảng lọc.</param>
        /// <param name="periodEnd">Ngày kết thúc khoảng lọc.</param>
        /// <returns>Danh sách bản ghi chấm công thỏa điều kiện lọc.</returns>
        public async Task<List<Attendance>> GetByEmployeesAndDateRangeAsync(
            List<Guid> employeeIds,
            DateTime periodStart,
            DateTime periodEnd
        )
        {
            if (employeeIds == null || employeeIds.Count == 0)
                return new List<Attendance>();

            using var conn = Connection;
            var sql =
                @"SELECT *
                  FROM attendance
                  WHERE employee_id IN @EmployeeIds
                    AND attendance_date >= @PeriodStart
                    AND attendance_date <= @PeriodEnd";

            var data = await conn.QueryAsync<Attendance>(
                sql,
                new
                {
                    EmployeeIds = employeeIds,
                    PeriodStart = periodStart,
                    PeriodEnd = periodEnd,
                }
            );

            return data.ToList();
        }
    }
}