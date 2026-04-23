using MISA.QLSX.Core.DTOs.Requests;
using MISA.QLSX.Core.Entities;
using MISA.QLSX.Core.Interfaces.Repository;
using MISA.QLSX.Infrastructure.Connection;
using Dapper;

namespace MISA.QLSX.Infrastructure.Repositories
{
    /// <summary>
    /// Cung cấp truy cập dữ liệu bậc thuế thu nhập cá nhân và truy vấn hiệu lực theo thời điểm.
    /// </summary>
    public class TaxBracketRepository : BaseRepository<TaxBracket>, ITaxBracketRepository
    {
        /// <summary>
        /// Khởi tạo repository bậc thuế với factory tạo kết nối MySQL.
        /// </summary>
        /// <param name="factory">Factory tạo kết nối cơ sở dữ liệu.</param>
        public TaxBracketRepository(MySqlConnectionFactory factory)
            : base(factory) { }

        /// <summary>
        /// Trả về tập cột được dùng cho tìm kiếm nhanh bậc thuế.
        /// </summary>
        /// <returns>Tập tên cột hỗ trợ tìm kiếm.</returns>
        protected override HashSet<string> GetSearchFields()
        {
            return new HashSet<string>
            {
                "bracket_code",
                "bracket_name",
                "description",
            };
        }

        protected override Dictionary<string, FieldMapItem> FieldMap =>
            new()
            {
                ["bracketCode"] = new() { Column = "bracket_code", DataType = typeof(string), Operators = new() { "eq", "contains", "starts", "ends", "neq", "notcontains" } },
                ["bracketName"] = new() { Column = "bracket_name", DataType = typeof(string), Operators = new() { "eq", "contains", "starts", "ends", "neq", "notcontains" } },
                ["lowerBound"] = new() { Column = "lower_bound", DataType = typeof(decimal), Operators = new() { "eq", "lt", "lte", "gt", "gte", "neq" } },
                ["upperBound"] = new() { Column = "upper_bound", DataType = typeof(decimal), Operators = new() { "eq", "lt", "lte", "gt", "gte", "neq", "isnull", "notnull" } },
                ["taxRate"] = new() { Column = "tax_rate", DataType = typeof(decimal), Operators = new() { "eq", "lt", "lte", "gt", "gte", "neq" } },
                ["effectiveFrom"] = new() { Column = "effective_from", DataType = typeof(DateTime), Operators = new() { "eq", "lt", "lte", "gt", "gte" } },
                ["effectiveTo"] = new() { Column = "effective_to", DataType = typeof(DateTime), Operators = new() { "eq", "lt", "lte", "gt", "gte", "isnull", "notnull" } },
                ["isActive"] = new() { Column = "is_active", DataType = typeof(bool), Operators = new() { "eq", "active", "inactive" } },
            };

        /// <summary>
        /// Lấy danh sách bậc thuế hiệu lực tại thời điểm chỉ định.
        /// </summary>
        /// <param name="atDate">Thời điểm cần tra cứu hiệu lực.</param>
        /// <returns>Danh sách bậc thuế hiệu lực, sắp theo cận dưới tăng dần.</returns>
        public async Task<List<TaxBracket>> GetEffectiveAtAsync(DateTime atDate)
        {
            using var conn = Connection;
            var sql =
                    @"SELECT *
                                    FROM tax_bracket
                                    WHERE (is_active IS NULL OR is_active = 1)
                                        AND effective_from IS NOT NULL
                                        AND DATE(effective_from) <= DATE(@AtDate)
                                        AND (effective_to IS NULL OR DATE(effective_to) >= DATE(@AtDate))
                                    ORDER BY lower_bound ASC";

            var data = await conn.QueryAsync<TaxBracket>(sql, new { AtDate = atDate });
            return data.ToList();
        }
    }
}
