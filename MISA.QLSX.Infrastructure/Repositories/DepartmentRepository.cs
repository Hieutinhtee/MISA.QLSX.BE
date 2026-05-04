using Dapper;
using MISA.QLSX.Core.DTOs.Requests;
using MISA.QLSX.Core.DTOs.Responses;
using MISA.QLSX.Core.Entities;
using MISA.QLSX.Core.Interfaces.Repository;
using MISA.QLSX.Infrastructure.Connection;

namespace MISA.QLSX.Infrastructure.Repositories
{
    /// <summary>
    /// Repository phòng ban.
    /// </summary>
    public class DepartmentRepository : BaseRepository<Department>, IDepartmentRepository
    {
        public DepartmentRepository(MySqlConnectionFactory factory)
            : base(factory) { }

        protected override HashSet<string> GetSearchFields()
        {
            return new HashSet<string> { "department_code", "department_name" };
        }

        protected override Dictionary<string, FieldMapItem> FieldMap =>
            new()
            {
                ["departmentCode"] = new()
                {
                    Column = "department_code",
                    DataType = typeof(string),
                    Operators = new() { "eq", "contains", "starts", "ends", "neq", "notcontains" },
                },
                ["departmentName"] = new()
                {
                    Column = "department_name",
                    DataType = typeof(string),
                    Operators = new() { "eq", "contains", "starts", "ends", "neq", "notcontains" },
                },
                ["createdAt"] = new()
                {
                    Column = "created_at",
                    DataType = typeof(DateTime),
                    Operators = new() { "eq", "lt", "lte", "gt", "gte" },
                },
                ["updatedAt"] = new()
                {
                    Column = "updated_at",
                    DataType = typeof(DateTime),
                    Operators = new() { "eq", "lt", "lte", "gt", "gte" },
                },
            };

        public override async Task<Department?> GetById(Guid id)
        {
            using var conn = Connection;
            var sql = $@"SELECT d.*, e.full_name AS ManagerEmployeeName 
                         FROM department d
                         LEFT JOIN employee e ON d.manager_employee_id = e.employee_id
                         WHERE d.department_id = @Id {SoftDeleteFilter("d.")}";
            return await conn.QueryFirstOrDefaultAsync<Department>(sql, new { Id = id });
        }

        public override async Task<PagingResponse<Department>> QueryPagingAsync(QueryRequest request)
        {
            using var conn = Connection;
            var (where, parameters) = BuildWhereClause(request.Filters, request.Search, "d.");
            var whereClause = string.IsNullOrEmpty(where) ? "" : "AND " + where;

            var sqlData = $@"SELECT d.*, e.full_name AS ManagerEmployeeName 
                             FROM department d
                             LEFT JOIN employee e ON d.manager_employee_id = e.employee_id
                             WHERE 1=1 {whereClause}
                             ORDER BY d.created_at DESC
                             LIMIT @Offset, @PageSize";

            var sqlTotal = $@"SELECT COUNT(*) FROM department d
                              WHERE 1=1 {whereClause}";

            parameters.Add("Offset", ((request.Page ?? 1) - 1) * (request.PageSize ?? 20));
            parameters.Add("PageSize", request.PageSize ?? 20);

            var data = await conn.QueryAsync<Department>(sqlData, parameters);
            var total = await conn.ExecuteScalarAsync<int>(sqlTotal, parameters);

            return new PagingResponse<Department>
            {
                Data = data.AsList(),
                Meta = new Meta
                {
                    Total = total,
                    Page = request.Page ?? 1,
                    PageSize = request.PageSize ?? 20
                }
            };
        }
    }
}
