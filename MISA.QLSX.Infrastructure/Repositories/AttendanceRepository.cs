using Dapper;
using MISA.QLSX.Core.DTOs.Requests;
using MISA.QLSX.Core.DTOs.Responses;
using MISA.QLSX.Core.Entities;
using MISA.QLSX.Core.Interfaces.Repository;
using MISA.QLSX.Infrastructure.Connection;

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
                ["attendanceCode"] = new()
                {
                    Column = "attendance_code",
                    DataType = typeof(string),
                    Operators = new() { "eq", "neq", "contains", "notcontains", "starts", "ends" },
                },
                ["employeeId"] = new()
                {
                    Column = "employee_id",
                    DataType = typeof(Guid),
                    Operators = new() { "eq", "neq" },
                },
                ["shiftId"] = new()
                {
                    Column = "shift_id",
                    DataType = typeof(Guid),
                    Operators = new() { "eq", "neq", "isnull", "notnull" },
                },
                ["attendanceDate"] = new()
                {
                    Column = "attendance_date",
                    DataType = typeof(DateTime),
                    Operators = new() { "eq", "lt", "lte", "gt", "gte" },
                },
                ["workingHours"] = new()
                {
                    Column = "working_hours",
                    DataType = typeof(decimal),
                    Operators = new() { "eq", "lt", "lte", "gt", "gte", "neq" },
                },
                ["overtimeHours"] = new()
                {
                    Column = "overtime_hours",
                    DataType = typeof(decimal),
                    Operators = new() { "eq", "lt", "lte", "gt", "gte", "neq" },
                },
                ["penaltyAmount"] = new()
                {
                    Column = "penalty_amount",
                    DataType = typeof(decimal),
                    Operators = new() { "eq", "lt", "lte", "gt", "gte", "neq" },
                },
                ["bonusAmount"] = new()
                {
                    Column = "bonus_amount",
                    DataType = typeof(decimal),
                    Operators = new() { "eq", "lt", "lte", "gt", "gte", "neq" },
                },
                ["netIncome"] = new()
                {
                    Column = "net_income",
                    DataType = typeof(decimal),
                    Operators = new() { "eq", "lt", "lte", "gt", "gte", "neq" },
                },
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

        public async Task<List<Attendance>> GetAttendancesByDateAsync(DateTime date)
        {
            using var conn = Connection;
            var sql = "SELECT * FROM attendance WHERE attendance_date = @Date";
            var data = await conn.QueryAsync<Attendance>(sql, new { Date = date.Date });
            return data.ToList();
        }

        public async Task<List<Attendance>> GetAttendancesByEmployeeInMonthAsync(
            Guid employeeId,
            int month,
            int year
        )
        {
            using var conn = Connection;
            var sql =
                @"SELECT a.*
                        FROM attendance a 
                        WHERE a.employee_id = @EmployeeId 
                        AND MONTH(a.attendance_date) = @Month 
                        AND YEAR(a.attendance_date) = @Year
                        ORDER BY a.attendance_date ASC";
            var data = await conn.QueryAsync<Attendance>(
                sql,
                new
                {
                    EmployeeId = employeeId,
                    Month = month,
                    Year = year,
                }
            );
            return data.ToList();
        }

        public async Task<IEnumerable<dynamic>> GetAbsentEmployeesTodayAsync(DateTime date)
        {
            using var conn = Connection;
            var sql =
                @"SELECT e.employee_id AS EmployeeId, e.employee_code AS EmployeeCode, e.full_name AS FullName, 
                               d.department_name AS DepartmentName, e.avatar_url AS AvatarUrl
                        FROM employee e
                        LEFT JOIN department d ON e.department_id = d.department_id
                        WHERE e.employee_id NOT IN (
                            SELECT employee_id FROM attendance WHERE attendance_date = @Date AND status != 'absent'
                        )";
            return await conn.QueryAsync(sql, new { Date = date.Date });
        }

        public async Task<IEnumerable<dynamic>> GetLateRankingsAsync(int month, int year)
        {
            using var conn = Connection;
            var sql =
                @"SELECT e.employee_id AS EmployeeId, e.employee_code AS EmployeeCode, e.full_name AS FullName, 
                               e.avatar_url AS AvatarUrl, COUNT(*) AS LateCount
                        FROM attendance a
                        JOIN employee e ON a.employee_id = e.employee_id
                        WHERE MONTH(a.attendance_date) = @Month AND YEAR(a.attendance_date) = @Year
                        AND a.status = 'late'
                        GROUP BY e.employee_id, e.employee_code, e.full_name, e.avatar_url
                        ORDER BY LateCount DESC
                        LIMIT 10";
            return await conn.QueryAsync(sql, new { Month = month, Year = year });
        }

        public override async Task<PagingResponse<Attendance>> QueryPagingAsync(
            QueryRequest request
        )
        {
            using var conn = Connection;
            var (where, parameters) = BuildWhereClause(request.Filters, request.Search, "a.");
            var whereClause = string.IsNullOrEmpty(where) ? "" : "AND " + where;

            var sqlData =
                $@"SELECT a.*, e.full_name AS FullName, e.employee_code AS EmployeeCode, d.department_name AS DepartmentName
                             FROM attendance a
                             JOIN employee e ON a.employee_id = e.employee_id
                             LEFT JOIN department d ON e.department_id = d.department_id
                             WHERE 1=1 {whereClause}
                             ORDER BY a.attendance_date DESC, a.created_at DESC
                             LIMIT @Offset, @PageSize";

            var sqlTotal =
                $@"SELECT COUNT(*) FROM attendance a
                              JOIN employee e ON a.employee_id = e.employee_id
                              WHERE 1=1 {whereClause}";

            parameters.Add("Offset", ((request.Page ?? 1) - 1) * (request.PageSize ?? 20));
            parameters.Add("PageSize", request.PageSize ?? 20);

            var data = await conn.QueryAsync<Attendance>(sqlData, parameters);
            var total = await conn.ExecuteScalarAsync<int>(sqlTotal, parameters);

            return new PagingResponse<Attendance>
            {
                Data = data.AsList(),
                Meta = new Meta
                {
                    Total = total,
                    Page = request.Page ?? 1,
                    PageSize = request.PageSize ?? 20,
                },
            };
        }
    }
}
