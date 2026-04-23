using MISA.QLSX.Core.DTOs.Requests;
using MISA.QLSX.Core.Entities;
using MISA.QLSX.Core.Interfaces.Repository;
using MISA.QLSX.Infrastructure.Connection;
using Dapper;

namespace MISA.QLSX.Infrastructure.Repositories
{
    /// <summary>
    /// Cung cấp truy cập dữ liệu chính sách lương và truy vấn chính sách hiệu lực theo thời điểm.
    /// </summary>
    public class SalaryPolicyRepository : BaseRepository<SalaryPolicy>, ISalaryPolicyRepository
    {
        /// <summary>
        /// Khởi tạo repository chính sách lương với factory tạo kết nối MySQL.
        /// </summary>
        /// <param name="factory">Factory tạo kết nối cơ sở dữ liệu.</param>
        public SalaryPolicyRepository(MySqlConnectionFactory factory)
            : base(factory) { }

        /// <summary>
        /// Trả về tập cột được dùng cho tìm kiếm nhanh chính sách lương.
        /// </summary>
        /// <returns>Tập tên cột hỗ trợ tìm kiếm.</returns>
        protected override HashSet<string> GetSearchFields()
        {
            return new HashSet<string>
            {
                "policy_code",
                "policy_name",
                "description",
            };
        }

        protected override Dictionary<string, FieldMapItem> FieldMap =>
            new()
            {
                ["policyCode"] = new() { Column = "policy_code", DataType = typeof(string), Operators = new() { "eq", "contains", "starts", "ends", "neq", "notcontains" } },
                ["policyName"] = new() { Column = "policy_name", DataType = typeof(string), Operators = new() { "eq", "contains", "starts", "ends", "neq", "notcontains" } },
                ["standardWorkdays"] = new() { Column = "standard_workdays", DataType = typeof(decimal), Operators = new() { "eq", "lt", "lte", "gt", "gte", "neq" } },
                ["effectiveFrom"] = new() { Column = "effective_from", DataType = typeof(DateTime), Operators = new() { "eq", "lt", "lte", "gt", "gte" } },
                ["effectiveTo"] = new() { Column = "effective_to", DataType = typeof(DateTime), Operators = new() { "eq", "lt", "lte", "gt", "gte", "isnull", "notnull" } },
                ["isActive"] = new() { Column = "is_active", DataType = typeof(bool), Operators = new() { "eq", "active", "inactive" } },
            };

        /// <summary>
        /// Lấy chính sách lương hiệu lực tại thời điểm chỉ định.
        /// Ưu tiên bản ghi có effective_from gần nhất trong khoảng hiệu lực.
        /// </summary>
        /// <param name="atDate">Thời điểm cần tra cứu hiệu lực.</param>
        /// <returns>Chính sách lương hiệu lực gần nhất; null nếu không tìm thấy.</returns>
        public async Task<SalaryPolicy?> GetEffectiveAtAsync(DateTime atDate)
        {
            using var conn = Connection;
            var sql =
                    @"SELECT *
                                    FROM salary_policy
                                    WHERE (is_active IS NULL OR is_active = 1)
                                        AND effective_from IS NOT NULL
                                        AND DATE(effective_from) <= DATE(@AtDate)
                                        AND (effective_to IS NULL OR DATE(effective_to) >= DATE(@AtDate))
                                    ORDER BY effective_from DESC
                                    LIMIT 1";

            return await conn.QueryFirstOrDefaultAsync<SalaryPolicy>(sql, new { AtDate = atDate });
        }
    }
}
