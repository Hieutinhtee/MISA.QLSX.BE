using MISA.QLSX.Core.DTOs.Requests;
using MISA.QLSX.Core.Entities;
using MISA.QLSX.Core.Interfaces.Repository;
using MISA.QLSX.Infrastructure.Connection;
using Dapper;

namespace MISA.QLSX.Infrastructure.Repositories
{
    /// <summary>
    /// Cung cấp truy cập dữ liệu hồ sơ thuế nhân viên và truy vấn hồ sơ hiệu lực theo thời điểm.
    /// </summary>
    public class EmployeeTaxProfileRepository : BaseRepository<EmployeeTaxProfile>, IEmployeeTaxProfileRepository
    {
        /// <summary>
        /// Khởi tạo repository hồ sơ thuế nhân viên với factory tạo kết nối MySQL.
        /// </summary>
        /// <param name="factory">Factory tạo kết nối cơ sở dữ liệu.</param>
        public EmployeeTaxProfileRepository(MySqlConnectionFactory factory)
            : base(factory) { }

        /// <summary>
        /// Trả về tập cột được dùng cho tìm kiếm nhanh hồ sơ thuế nhân viên.
        /// </summary>
        /// <returns>Tập tên cột hỗ trợ tìm kiếm.</returns>
        protected override HashSet<string> GetSearchFields()
        {
            return new HashSet<string>
            {
                "tax_code",
            };
        }

        protected override Dictionary<string, FieldMapItem> FieldMap =>
            new()
            {
                ["employeeId"] = new() { Column = "employee_id", DataType = typeof(Guid), Operators = new() { "eq", "neq", "isnull", "notnull" } },
                ["taxCode"] = new() { Column = "tax_code", DataType = typeof(string), Operators = new() { "eq", "contains", "starts", "ends", "neq", "notcontains" } },
                ["dependentCount"] = new() { Column = "dependent_count", DataType = typeof(int), Operators = new() { "eq", "lt", "lte", "gt", "gte", "neq" } },
                ["isResident"] = new() { Column = "is_resident", DataType = typeof(bool), Operators = new() { "eq", "active", "inactive" } },
                ["effectiveFrom"] = new() { Column = "effective_from", DataType = typeof(DateTime), Operators = new() { "eq", "lt", "lte", "gt", "gte" } },
                ["effectiveTo"] = new() { Column = "effective_to", DataType = typeof(DateTime), Operators = new() { "eq", "lt", "lte", "gt", "gte", "isnull", "notnull" } },
                ["isActive"] = new() { Column = "is_active", DataType = typeof(bool), Operators = new() { "eq", "active", "inactive" } },
            };

        /// <summary>
        /// Lấy hồ sơ thuế hiệu lực của danh sách nhân viên tại thời điểm chỉ định.
        /// Với mỗi nhân viên, ưu tiên bản ghi có effective_from gần nhất trong khoảng hiệu lực.
        /// </summary>
        /// <param name="employeeIds">Danh sách định danh nhân viên.</param>
        /// <param name="atDate">Thời điểm cần tra cứu hiệu lực.</param>
        /// <returns>Danh sách hồ sơ thuế hiệu lực theo nhân viên.</returns>
        public async Task<List<EmployeeTaxProfile>> GetEffectiveByEmployeesAsync(List<Guid> employeeIds, DateTime atDate)
        {
            if (employeeIds == null || employeeIds.Count == 0)
                return new List<EmployeeTaxProfile>();

            using var conn = Connection;
            var sql =
                @"SELECT p.*
                  FROM employee_tax_profile p
                  INNER JOIN (
                      SELECT employee_id, MAX(effective_from) AS max_effective_from
                      FROM employee_tax_profile
                      WHERE employee_id IN @EmployeeIds
                        AND (is_active IS NULL OR is_active = 1)
                        AND effective_from IS NOT NULL
                        AND DATE(effective_from) <= DATE(@AtDate)
                        AND (effective_to IS NULL OR DATE(effective_to) >= DATE(@AtDate))
                      GROUP BY employee_id
                  ) x
                    ON p.employee_id = x.employee_id
                   AND p.effective_from = x.max_effective_from";

            var data = await conn.QueryAsync<EmployeeTaxProfile>(
                sql,
                new
                {
                    EmployeeIds = employeeIds,
                    AtDate = atDate,
                }
            );

            return data.ToList();
        }
    }
}
