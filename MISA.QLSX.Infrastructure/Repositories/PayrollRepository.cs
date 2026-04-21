using MISA.QLSX.Core.DTOs.Requests;
using MISA.QLSX.Core.Entities;
using MISA.QLSX.Core.Interfaces.Repository;
using MISA.QLSX.Infrastructure.Connection;

namespace MISA.QLSX.Infrastructure.Repositories
{
    public class PayrollRepository : BaseRepository<Payroll>, IPayrollRepository
    {
        public PayrollRepository(MySqlConnectionFactory factory)
            : base(factory) { }

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
    }
}