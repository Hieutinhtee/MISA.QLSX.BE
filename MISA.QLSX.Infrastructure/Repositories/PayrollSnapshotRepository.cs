using MISA.QLSX.Core.DTOs.Requests;
using MISA.QLSX.Core.Entities;
using MISA.QLSX.Core.Interfaces.Repository;
using MISA.QLSX.Infrastructure.Connection;

namespace MISA.QLSX.Infrastructure.Repositories
{
    public class PayrollSnapshotRepository : BaseRepository<PayrollSnapshot>, IPayrollSnapshotRepository
    {
        public PayrollSnapshotRepository(MySqlConnectionFactory factory)
            : base(factory) { }

        protected override HashSet<string> GetSearchFields()
        {
            return new HashSet<string> { "payroll_id" };
        }

        protected override Dictionary<string, FieldMapItem> FieldMap =>
            new()
            {
                ["payrollId"] = new() { Column = "payroll_id", DataType = typeof(Guid), Operators = new() { "eq", "neq" } },
                ["snapshotAt"] = new() { Column = "snapshot_at", DataType = typeof(DateTime), Operators = new() { "eq", "lt", "lte", "gt", "gte" } },
            };
    }
}