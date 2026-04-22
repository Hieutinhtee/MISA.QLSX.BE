using MISA.QLSX.Core.DTOs.Requests;
using MISA.QLSX.Core.Entities;
using MISA.QLSX.Core.Interfaces.Repository;
using MISA.QLSX.Infrastructure.Connection;
using Dapper;

namespace MISA.QLSX.Infrastructure.Repositories
{
    /// <summary>
    /// Cung cấp truy cập dữ liệu bảng lương và các truy vấn theo kỳ lương phục vụ xử lý nghiệp vụ lương.
    /// </summary>
    public class PayrollRepository : BaseRepository<Payroll>, IPayrollRepository
    {
        /// <summary>
        /// Khởi tạo repository bảng lương với factory tạo kết nối MySQL.
        /// </summary>
        /// <param name="factory">Factory tạo kết nối cơ sở dữ liệu.</param>
        public PayrollRepository(MySqlConnectionFactory factory)
            : base(factory) { }

        /// <summary>
        /// Trả về danh sách các cột được dùng cho tìm kiếm nhanh bảng lương.
        /// </summary>
        /// <returns>Tập tên cột hỗ trợ tìm kiếm.</returns>
        protected override HashSet<string> GetSearchFields()
        {
            return new HashSet<string> { "payroll_code", "status" };
        }

        protected override Dictionary<string, FieldMapItem> FieldMap =>
            new()
            {
                ["payrollCode"] = new() { Column = "payroll_code", DataType = typeof(string), Operators = new() { "eq", "neq", "contains", "notcontains", "starts", "ends" } },
                ["salaryPeriodId"] = new() { Column = "salary_period_id", DataType = typeof(Guid), Operators = new() { "eq", "neq", "isnull", "notnull" } },
                ["employeeId"] = new() { Column = "employee_id", DataType = typeof(Guid), Operators = new() { "eq", "neq", "isnull", "notnull" } },
                ["status"] = new() { Column = "status", DataType = typeof(string), Operators = new() { "eq", "neq", "contains", "notcontains", "starts", "ends" } },
                ["grossSalary"] = new() { Column = "gross_salary", DataType = typeof(decimal), Operators = new() { "eq", "lt", "lte", "gt", "gte", "neq" } },
                ["netSalary"] = new() { Column = "net_salary", DataType = typeof(decimal), Operators = new() { "eq", "lt", "lte", "gt", "gte", "neq" } },
                ["taxableSalary"] = new() { Column = "taxable_salary", DataType = typeof(decimal), Operators = new() { "eq", "lt", "lte", "gt", "gte", "neq" } },
                ["pitTaxAmount"] = new() { Column = "pit_tax_amount", DataType = typeof(decimal), Operators = new() { "eq", "lt", "lte", "gt", "gte", "neq" } },
                ["insuranceDeduction"] = new() { Column = "insurance_deduction", DataType = typeof(decimal), Operators = new() { "eq", "lt", "lte", "gt", "gte", "neq" } },
                ["workingDaysActual"] = new() { Column = "working_days_actual", DataType = typeof(decimal), Operators = new() { "eq", "lt", "lte", "gt", "gte", "neq" } },
                ["workingDaysStandard"] = new() { Column = "working_days_standard", DataType = typeof(decimal), Operators = new() { "eq", "lt", "lte", "gt", "gte", "neq" } },
                ["totalAllowance"] = new() { Column = "total_allowance", DataType = typeof(decimal), Operators = new() { "eq", "lt", "lte", "gt", "gte", "neq" } },
                ["totalAddition"] = new() { Column = "total_addition", DataType = typeof(decimal), Operators = new() { "eq", "lt", "lte", "gt", "gte", "neq" } },
                ["totalDeduction"] = new() { Column = "total_deduction", DataType = typeof(decimal), Operators = new() { "eq", "lt", "lte", "gt", "gte", "neq" } },
                ["lockedAt"] = new() { Column = "locked_at", DataType = typeof(DateTime), Operators = new() { "eq", "lt", "lte", "gt", "gte", "isnull", "notnull" } },
                ["paidAt"] = new() { Column = "paid_at", DataType = typeof(DateTime), Operators = new() { "eq", "lt", "lte", "gt", "gte", "isnull", "notnull" } },
            };

        /// <summary>
        /// Lấy danh sách bảng lương theo kỳ lương và tùy chọn lọc theo nhân viên.
        /// </summary>
        /// <param name="salaryPeriodId">Định danh kỳ lương cần truy vấn.</param>
        /// <param name="employeeId">Định danh nhân viên cần lọc, để trống nếu lấy toàn bộ trong kỳ.</param>
        /// <returns>Danh sách bảng lương thỏa điều kiện lọc.</returns>
        public async Task<List<Payroll>> GetBySalaryPeriodAsync(Guid salaryPeriodId, Guid? employeeId = null)
        {
            using var conn = Connection;
            var sql =
                $@"SELECT *
                    FROM {_tableName}
                    WHERE salary_period_id = @SalaryPeriodId
                      AND (@EmployeeId IS NULL OR employee_id = @EmployeeId)
                    ORDER BY updated_at DESC";

            var data = await conn.QueryAsync<Payroll>(
                sql,
                new { SalaryPeriodId = salaryPeriodId, EmployeeId = employeeId }
            );

            return data.ToList();
        }
    }
}