using MISA.QLSX.Core.DTOs.Requests;
using MISA.QLSX.Core.Entities;
using MISA.QLSX.Core.Interfaces.Repository;
using MISA.QLSX.Infrastructure.Connection;

namespace MISA.QLSX.Infrastructure.Repositories
{
    public class SalaryPeriodRepository : BaseRepository<SalaryPeriod>, ISalaryPeriodRepository
    {
        public SalaryPeriodRepository(MySqlConnectionFactory factory)
            : base(factory) { }

        protected override HashSet<string> GetSearchFields()
        {
            return new HashSet<string> { "status" };
        }

        protected override Dictionary<string, FieldMapItem> FieldMap =>
            new()
            {
                ["startDate"] = new() { Column = "start_date", DataType = typeof(DateTime), Operators = new() { "eq", "lt", "lte", "gt", "gte" } },
                ["endDate"] = new() { Column = "end_date", DataType = typeof(DateTime), Operators = new() { "eq", "lt", "lte", "gt", "gte" } },
                ["status"] = new() { Column = "status", DataType = typeof(string), Operators = new() { "eq", "neq", "contains", "notcontains", "starts", "ends" } },
            };
    }
}