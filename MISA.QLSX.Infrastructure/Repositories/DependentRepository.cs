using Dapper;
using MISA.QLSX.Core.DTOs.Requests;
using MISA.QLSX.Core.Entities;
using MISA.QLSX.Core.Interfaces.Repository;
using MISA.QLSX.Infrastructure.Connection;

namespace MISA.QLSX.Infrastructure.Repositories
{
    public class DependentRepository : BaseRepository<Dependent>, IDependentRepository
    {
        public DependentRepository(MySqlConnectionFactory factory) : base(factory)
        {
        }

        public async Task<List<Dependent>> GetByEmployeeIdAsync(Guid employeeId)
        {
            var sql = $"SELECT * FROM {GetReadTableName()} WHERE employee_id = @EmployeeId AND is_active = 1 ORDER BY created_at DESC";
            using var connection = Connection;
            var result = await connection.QueryAsync<Dependent>(sql, new { EmployeeId = employeeId });
            return result.ToList();
        }

        protected override HashSet<string> GetSearchFields()
        {
            return new HashSet<string> { "full_name", "tax_code", "identity_number" };
        }

        protected override Dictionary<string, FieldMapItem> FieldMap => new Dictionary<string, FieldMapItem>
        {
            { "fullName", new FieldMapItem { Column = "full_name", DataType = typeof(string), Operators = new HashSet<string> { "contains", "eq" } } },
            { "relationship", new FieldMapItem { Column = "relationship", DataType = typeof(string), Operators = new HashSet<string> { "eq" } } },
            { "isActive", new FieldMapItem { Column = "is_active", DataType = typeof(bool), Operators = new HashSet<string> { "eq" } } },
            { "employeeId", new FieldMapItem { Column = "employee_id", DataType = typeof(Guid), Operators = new HashSet<string> { "eq" } } }
        };
    }
}
