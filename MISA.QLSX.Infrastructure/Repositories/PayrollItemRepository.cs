using MISA.QLSX.Core.DTOs.Requests;
using MISA.QLSX.Core.Entities;
using MISA.QLSX.Core.Interfaces.Repository;
using MISA.QLSX.Infrastructure.Connection;
using Dapper;

namespace MISA.QLSX.Infrastructure.Repositories
{
    /// <summary>
    /// Cung cấp truy cập dữ liệu chi tiết thành phần lương và các truy vấn theo danh sách bảng lương.
    /// </summary>
    public class PayrollItemRepository : BaseRepository<PayrollItem>, IPayrollItemRepository
    {
        /// <summary>
        /// Khởi tạo repository chi tiết thành phần lương với factory tạo kết nối MySQL.
        /// </summary>
        /// <param name="factory">Factory tạo kết nối cơ sở dữ liệu.</param>
        public PayrollItemRepository(MySqlConnectionFactory factory)
            : base(factory) { }

        /// <summary>
        /// Trả về tập cột được dùng cho tìm kiếm nhanh chi tiết thành phần lương.
        /// </summary>
        /// <returns>Tập tên cột hỗ trợ tìm kiếm.</returns>
        protected override HashSet<string> GetSearchFields()
        {
            return new HashSet<string>
            {
                "payroll_item_code",
                "item_name",
                "item_type",
                "formula_component",
                "source_table",
                "note",
            };
        }

        protected override Dictionary<string, FieldMapItem> FieldMap =>
            new()
            {
                ["payrollItemCode"] = new() { Column = "payroll_item_code", DataType = typeof(string), Operators = new() { "eq", "neq", "contains", "notcontains", "starts", "ends" } },
                ["payrollId"] = new() { Column = "payroll_id", DataType = typeof(Guid), Operators = new() { "eq", "neq" } },
                ["itemType"] = new() { Column = "item_type", DataType = typeof(string), Operators = new() { "eq", "neq", "contains", "notcontains", "starts", "ends" } },
                ["itemName"] = new() { Column = "item_name", DataType = typeof(string), Operators = new() { "eq", "neq", "contains", "notcontains", "starts", "ends" } },
                ["formulaComponent"] = new() { Column = "formula_component", DataType = typeof(string), Operators = new() { "eq", "neq", "contains", "notcontains", "starts", "ends" } },
                ["amount"] = new() { Column = "amount", DataType = typeof(decimal), Operators = new() { "eq", "lt", "lte", "gt", "gte", "neq" } },
                ["sourceTable"] = new() { Column = "source_table", DataType = typeof(string), Operators = new() { "eq", "neq", "contains", "notcontains", "starts", "ends" } },
                ["sourceId"] = new() { Column = "source_id", DataType = typeof(Guid), Operators = new() { "eq", "neq", "isnull", "notnull" } },
                ["note"] = new() { Column = "note", DataType = typeof(string), Operators = new() { "eq", "neq", "contains", "notcontains", "starts", "ends" } },
            };

        /// <summary>
        /// Lấy danh sách chi tiết thành phần lương theo danh sách bảng lương.
        /// </summary>
        /// <param name="payrollIds">Danh sách định danh bảng lương.</param>
        /// <returns>Danh sách bản ghi chi tiết lương thuộc các bảng lương được chỉ định.</returns>
        public async Task<List<PayrollItem>> GetByPayrollIdsAsync(List<Guid> payrollIds)
        {
            if (payrollIds == null || payrollIds.Count == 0)
                return new List<PayrollItem>();

            using var conn = Connection;
            var sql =
                @"SELECT *
                  FROM payroll_item
                  WHERE payroll_id IN @PayrollIds";

            var data = await conn.QueryAsync<PayrollItem>(sql, new { PayrollIds = payrollIds });
            return data.ToList();
        }
    }
}