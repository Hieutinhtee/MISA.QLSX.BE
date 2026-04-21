using MISA.QLSX.Core.DTOs.Requests;
using MISA.QLSX.Core.Entities;
using MISA.QLSX.Core.Interfaces.Repository;
using MISA.QLSX.Infrastructure.Connection;

namespace MISA.QLSX.Infrastructure.Repositories
{
    public class PayrollItemRepository : BaseRepository<PayrollItem>, IPayrollItemRepository
    {
        public PayrollItemRepository(MySqlConnectionFactory factory)
            : base(factory) { }

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
    }
}