using MISA.QLSX.Core.Entities;
using MISA.QLSX.Core.Interfaces.Repository;
using MISA.QLSX.Core.DTOs.Requests;
using MISA.QLSX.Infrastructure.Connection;
using Dapper;

namespace MISA.QLSX.Infrastructure.Repositories
{
    public class AllowanceRepository : BaseRepository<Allowance>, IAllowanceRepository
    {
        /// <summary>
        /// Khởi tạo repository phụ cấp với factory tạo kết nối MySQL.
        /// </summary>
        /// <param name="factory">Factory tạo kết nối cơ sở dữ liệu.</param>
        public AllowanceRepository(MySqlConnectionFactory factory)
            : base(factory) { }

        /// <summary>
        /// Lấy danh sách phụ cấp theo danh sách ID.
        /// </summary>
        /// <param name="ids">Danh sách ID phụ cấp.</param>
        /// <returns>Danh sách phụ cấp tương ứng.</returns>
        public async Task<List<Allowance>> GetByIdsAsync(List<Guid> ids)
        {
            if (ids == null || ids.Count == 0)
                return new List<Allowance>();

            var paramNames = string.Join(",", ids.Select((_, i) => $"@p{i}"));
            var sql = $@"
                SELECT * FROM allowance
                WHERE allowance_id IN ({paramNames})
            ";

            var dynamicParams = new DynamicParameters();
            for (int i = 0; i < ids.Count; i++)
            {
                dynamicParams.Add($"p{i}", ids[i]);
            }

            using (var connection = _factory.CreateConnection())
            {
                var result = await connection.QueryAsync<Allowance>(sql, dynamicParams);
                return result.ToList();
            }
        }

        /// <summary>
        /// Trả về tập cột được hỗ trợ cho tìm kiếm nhanh phụ cấp.
        /// </summary>
        /// <returns>Tập tên cột dùng cho tìm kiếm.</returns>
        protected override HashSet<string> GetSearchFields()
        {
            return new HashSet<string>
            {
                "allowance_code",
                "allowance_name",
                "calculation_type",
            };
        }

        protected override Dictionary<string, FieldMapItem> FieldMap =>
            new()
            {
                ["allowanceCode"] = new() { Column = "allowance_code", DataType = typeof(string), Operators = new() { "eq", "contains", "starts", "ends", "neq", "notcontains" } },
                ["allowanceName"] = new() { Column = "allowance_name", DataType = typeof(string), Operators = new() { "eq", "contains", "starts", "ends", "neq", "notcontains" } },
                ["calculationType"] = new() { Column = "calculation_type", DataType = typeof(string), Operators = new() { "eq", "contains", "starts", "ends", "neq", "notcontains" } },
                ["amount"] = new() { Column = "amount", DataType = typeof(decimal), Operators = new() { "eq", "lt", "lte", "gt", "gte", "neq", "isnull", "notnull" } },
                ["percent"] = new() { Column = "percent", DataType = typeof(decimal), Operators = new() { "eq", "lt", "lte", "gt", "gte", "neq", "isnull", "notnull" } },
                ["createdAt"] = new() { Column = "created_at", DataType = typeof(DateTime), Operators = new() { "eq", "lt", "lte", "gt", "gte" } },
                ["updatedAt"] = new() { Column = "updated_at", DataType = typeof(DateTime), Operators = new() { "eq", "lt", "lte", "gt", "gte" } },
            };
    }
}
