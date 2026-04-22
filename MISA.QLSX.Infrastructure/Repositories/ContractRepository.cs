using MISA.QLSX.Core.DTOs.Requests;
using MISA.QLSX.Core.Entities;
using MISA.QLSX.Core.Interfaces.Repository;
using MISA.QLSX.Infrastructure.Connection;
using Dapper;

namespace MISA.QLSX.Infrastructure.Repositories
{
    /// <summary>
    /// Cung cấp truy cập dữ liệu hợp đồng, bao gồm truy vấn theo view đọc và truy vấn hợp đồng hiệu lực theo kỳ lương.
    /// </summary>
    public class ContractRepository : BaseRepository<Contract>, IContractRepository
    {
        private const string ReadViewName = "vw_contract_detail";

        /// <summary>
        /// Khởi tạo repository hợp đồng với factory tạo kết nối MySQL.
        /// </summary>
        /// <param name="factory">Factory tạo kết nối cơ sở dữ liệu.</param>
        public ContractRepository(MySqlConnectionFactory factory)
            : base(factory) { }

        /// <summary>
        /// Lấy tên nguồn đọc dữ liệu hợp đồng dùng cho truy vấn danh sách/chi tiết.
        /// </summary>
        /// <returns>Tên view đọc dữ liệu hợp đồng.</returns>
        protected override string GetReadTableName() => ReadViewName;

        /// <summary>
        /// Trả về tập cột được dùng cho tìm kiếm toàn văn trong danh sách hợp đồng.
        /// </summary>
        /// <returns>Tập tên cột được hỗ trợ tìm kiếm.</returns>
        protected override HashSet<string> GetSearchFields()
        {
            return new HashSet<string>
            {
                "contract_code",
                "template_code",
                "template_name",
                "contract_type",
                "company_representative_name",
                "company_signer_title",
                "employee_code",
                "employee_name",
                "summary",
            };
        }

        protected override Dictionary<string, FieldMapItem> FieldMap =>
            new()
            {
                ["contractCode"] = new() { Column = "contract_code", DataType = typeof(string), Operators = new() { "eq", "notcontains", "contains", "starts", "ends", "neq" } },
                ["templateId"] = new() { Column = "template_id", DataType = typeof(Guid), Operators = new() { "eq", "neq", "isnull", "notnull" } },
                ["templateCode"] = new() { Column = "template_code", DataType = typeof(string), Operators = new() { "eq", "notcontains", "contains", "starts", "ends", "neq" } },
                ["templateName"] = new() { Column = "template_name", DataType = typeof(string), Operators = new() { "eq", "notcontains", "contains", "starts", "ends", "neq" } },
                ["contractType"] = new() { Column = "contract_type", DataType = typeof(string), Operators = new() { "eq", "notcontains", "contains", "starts", "ends", "neq" } },
                ["companyRepresentativeId"] = new() { Column = "company_representative_id", DataType = typeof(Guid), Operators = new() { "eq", "neq", "isnull", "notnull" } },
                ["companyRepresentativeName"] = new() { Column = "company_representative_name", DataType = typeof(string), Operators = new() { "eq", "notcontains", "contains", "starts", "ends", "neq" } },
                ["companySignerTitle"] = new() { Column = "company_signer_title", DataType = typeof(string), Operators = new() { "eq", "notcontains", "contains", "starts", "ends", "neq" } },
                ["employeeId"] = new() { Column = "employee_id", DataType = typeof(Guid), Operators = new() { "eq", "neq", "isnull", "notnull" } },
                ["employeeCode"] = new() { Column = "employee_code", DataType = typeof(string), Operators = new() { "eq", "notcontains", "contains", "starts", "ends", "neq" } },
                ["employeeName"] = new() { Column = "employee_name", DataType = typeof(string), Operators = new() { "eq", "notcontains", "contains", "starts", "ends", "neq" } },
                ["effectiveDate"] = new() { Column = "effective_date", DataType = typeof(DateTime), Operators = new() { "eq", "lt", "lte", "gt", "gte" } },
                ["termMonths"] = new() { Column = "term_months", DataType = typeof(int), Operators = new() { "eq", "lt", "lte", "gt", "gte", "neq", "isnull", "notnull" } },
                ["baseSalary"] = new() { Column = "base_salary", DataType = typeof(decimal), Operators = new() { "eq", "lt", "lte", "gt", "gte" } },
                ["insuranceSalary"] = new() { Column = "insurance_salary", DataType = typeof(decimal), Operators = new() { "eq", "lt", "lte", "gt", "gte" } },
                ["salaryRatio"] = new() { Column = "salary_ratio", DataType = typeof(decimal), Operators = new() { "eq", "lt", "lte", "gt", "gte" } },
                ["summary"] = new() { Column = "summary", DataType = typeof(string), Operators = new() { "eq", "notcontains", "contains", "starts", "ends", "neq" } },
                ["attachmentLink"] = new() { Column = "attachment_link", DataType = typeof(string), Operators = new() { "eq", "notcontains", "contains", "starts", "ends", "neq" } },
                ["isSigned"] = new() { Column = "is_signed", DataType = typeof(bool), Operators = new() { "eq", "active", "inactive" } },
                ["signedAt"] = new() { Column = "signed_at", DataType = typeof(DateTime), Operators = new() { "eq", "lt", "lte", "gt", "gte", "isnull", "notnull" } },
                ["createdAt"] = new() { Column = "created_at", DataType = typeof(DateTime), Operators = new() { "eq", "lt", "lte", "gt", "gte" } },
                ["updatedAt"] = new() { Column = "updated_at", DataType = typeof(DateTime), Operators = new() { "eq", "lt", "lte", "gt", "gte" } },
            };

        /// <summary>
        /// Lấy danh sách hợp đồng hiệu lực của các nhân viên trong khoảng thời gian kỳ lương.
        /// </summary>
        /// <param name="employeeIds">Danh sách định danh nhân viên cần truy vấn hợp đồng.</param>
        /// <param name="periodStart">Ngày bắt đầu kỳ lương.</param>
        /// <param name="periodEnd">Ngày kết thúc kỳ lương.</param>
        /// <returns>Danh sách hợp đồng có hiệu lực trong kỳ lương theo từng nhân viên.</returns>
        public async Task<List<Contract>> GetEffectiveByEmployeesAsync(
            List<Guid> employeeIds,
            DateTime periodStart,
            DateTime periodEnd
        )
        {
            if (employeeIds == null || employeeIds.Count == 0)
                return new List<Contract>();

            using var conn = Connection;
            var sql =
                @"SELECT *
                  FROM contract
                  WHERE employee_id IN @EmployeeIds
                    AND effective_date IS NOT NULL
                    AND effective_date <= @PeriodEnd
                    AND (end_date IS NULL OR end_date >= @PeriodStart)
                  ORDER BY employee_id, effective_date";

            var data = await conn.QueryAsync<Contract>(
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
