using Dapper;
using MISA.QLSX.Core.DTOs.Requests;
using MISA.QLSX.Core.DTOs.Responses;
using MISA.QLSX.Core.Entities;
using MISA.QLSX.Core.Interfaces.Repository;
using MISA.QLSX.Infrastructure.Connection;

namespace MISA.QLSX.Infrastructure.Repositories
{
    /// <summary>
    /// Repository truy cập dữ liệu đơn xin nghỉ phép.
    /// </summary>
    public class LeaveRequestRepository : BaseRepository<LeaveRequest>, ILeaveRequestRepository
    {
        /// <summary>
        /// Khởi tạo repository đơn nghỉ phép.
        /// </summary>
        /// <param name="factory">Factory kết nối MySQL.</param>
        public LeaveRequestRepository(MySqlConnectionFactory factory)
            : base(factory) { }

        /// <summary>
        /// Tập cột hỗ trợ tìm kiếm nhanh.
        /// </summary>
        /// <returns>Danh sách cột tìm kiếm.</returns>
        protected override HashSet<string> GetSearchFields()
        {
            return new HashSet<string>
            {
                "leave_request_code",
                "reason",
                "employee_code",
                "full_name",
            };
        }

        /// <inheritdoc />
        protected override Dictionary<string, FieldMapItem> FieldMap =>
            new()
            {
                ["leaveRequestCode"] = new()
                {
                    Column = "leave_request_code",
                    DataType = typeof(string),
                    Operators = new() { "eq", "contains", "starts", "ends", "neq", "notcontains" },
                },
                ["employeeId"] = new()
                {
                    Column = "employee_id",
                    DataType = typeof(Guid),
                    Operators = new() { "eq", "neq" },
                },
                ["employeeCode"] = new()
                {
                    Column = "employee_code",
                    DataType = typeof(string),
                    Operators = new() { "eq", "contains", "starts", "ends", "neq", "notcontains" },
                },
                ["employeeName"] = new()
                {
                    Column = "full_name",
                    DataType = typeof(string),
                    Operators = new() { "eq", "contains", "starts", "ends", "neq", "notcontains" },
                },
                ["startDate"] = new()
                {
                    Column = "start_date",
                    DataType = typeof(DateTime),
                    Operators = new() { "eq", "lt", "lte", "gt", "gte" },
                },
                ["returnDate"] = new()
                {
                    Column = "return_date",
                    DataType = typeof(DateTime),
                    Operators = new() { "eq", "lt", "lte", "gt", "gte" },
                },
                ["approvalStatus"] = new()
                {
                    Column = "approval_status",
                    DataType = typeof(int),
                    Operators = new() { "eq", "neq" },
                },
                ["createdAt"] = new()
                {
                    Column = "created_at",
                    DataType = typeof(DateTime),
                    Operators = new() { "eq", "lt", "lte", "gt", "gte" },
                },
            };

        /// <summary>
        /// Lấy chi tiết đơn nghỉ phép theo ID.
        /// </summary>
        /// <param name="id">ID đơn nghỉ phép.</param>
        /// <returns>Đơn nghỉ phép nếu tồn tại.</returns>
        public override async Task<LeaveRequest?> GetById(Guid id)
        {
            using var conn = Connection;
            var sql =
                @"SELECT lr.*, e.employee_code AS EmployeeCode, e.full_name AS EmployeeName
                        FROM leave_request lr
                        LEFT JOIN employee e ON lr.employee_id = e.employee_id
                        WHERE lr.leave_request_id = @Id";

            return await conn.QueryFirstOrDefaultAsync<LeaveRequest>(sql, new { Id = id });
        }

        /// <summary>
        /// Lấy danh sách phân trang cho đơn nghỉ phép.
        /// </summary>
        /// <param name="request">Yêu cầu phân trang.</param>
        /// <returns>Dữ liệu phân trang.</returns>
        public override async Task<PagingResponse<LeaveRequest>> QueryPagingAsync(QueryRequest request)
        {
            using var conn = Connection;
            var (where, parameters) = BuildWhereClause(request.Filters, request.Search, "lr.");
            var whereClause = string.IsNullOrEmpty(where) ? string.Empty : "AND " + where;

            var sqlData =
                $@"SELECT lr.*, e.employee_code AS EmployeeCode, e.full_name AS EmployeeName
                        FROM leave_request lr
                        LEFT JOIN employee e ON lr.employee_id = e.employee_id
                        WHERE 1=1 {whereClause}
                        ORDER BY lr.created_at DESC
                        LIMIT @Offset, @PageSize";

            var sqlTotal =
                $@"SELECT COUNT(*)
                        FROM leave_request lr
                        LEFT JOIN employee e ON lr.employee_id = e.employee_id
                        WHERE 1=1 {whereClause}";

            parameters.Add("Offset", ((request.Page ?? 1) - 1) * (request.PageSize ?? 20));
            parameters.Add("PageSize", request.PageSize ?? 20);

            var data = await conn.QueryAsync<LeaveRequest>(sqlData, parameters);
            var total = await conn.ExecuteScalarAsync<int>(sqlTotal, parameters);

            return new PagingResponse<LeaveRequest>
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

        /// <summary>
        /// Kiểm tra trùng khoảng ngày nghỉ theo nhân viên.
        /// </summary>
        /// <param name="employeeId">ID nhân viên.</param>
        /// <param name="startDate">Ngày bắt đầu.</param>
        /// <param name="returnDate">Ngày kết thúc.</param>
        /// <param name="ignoreId">ID đơn cần bỏ qua.</param>
        /// <returns>True nếu có đơn trùng khoảng ngày.</returns>
        public async Task<bool> HasOverlappingRequestAsync(
            Guid employeeId,
            DateTime startDate,
            DateTime returnDate,
            Guid? ignoreId = null
        )
        {
            using var conn = Connection;
            var sql =
                @"SELECT COUNT(*)
                        FROM leave_request
                        WHERE employee_id = @EmployeeId
                          AND approval_status IN (0, 1)
                          AND NOT (return_date < @StartDate OR start_date > @ReturnDate)
                          AND (@IgnoreId IS NULL OR leave_request_id <> @IgnoreId)";

            var total = await conn.ExecuteScalarAsync<int>(
                sql,
                new
                {
                    EmployeeId = employeeId,
                    StartDate = startDate.Date,
                    ReturnDate = returnDate.Date,
                    IgnoreId = ignoreId,
                }
            );

            return total > 0;
        }

        /// <summary>
        /// Sinh mã đơn nghỉ phép tiếp theo.
        /// </summary>
        /// <returns>Mã đơn nghỉ phép.</returns>
        public async Task<string> GenerateLeaveRequestCodeAsync()
        {
            using var conn = Connection;
            var sql = "SELECT COUNT(*) FROM leave_request";
            var count = await conn.ExecuteScalarAsync<int>(sql);
            return $"LR-{(count + 1):D5}";
        }
    }
}