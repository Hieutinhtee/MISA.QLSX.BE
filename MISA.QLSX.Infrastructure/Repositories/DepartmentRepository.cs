using MISA.QLSX.Core.DTOs.Requests;
using MISA.QLSX.Core.Entities;
using MISA.QLSX.Core.Interfaces.Repository;
using MISA.QLSX.Infrastructure.Connection;

namespace MISA.QLSX.Infrastructure.Repositories
{
    /// <summary>
    /// Repository phòng ban.
    /// </summary>
    public class DepartmentRepository : BaseRepository<Department>, IDepartmentRepository
    {
        public DepartmentRepository(MySqlConnectionFactory factory)
            : base(factory) { }

        protected override HashSet<string> GetSearchFields()
        {
            return new HashSet<string> { "department_code", "department_name" };
        }

        protected override Dictionary<string, FieldMapItem> FieldMap =>
            new()
            {
                ["departmentCode"] = new()
                {
                    Column = "department_code",
                    DataType = typeof(string),
                    Operators = new() { "eq", "contains", "starts", "ends", "neq", "notcontains" },
                },
                ["departmentName"] = new()
                {
                    Column = "department_name",
                    DataType = typeof(string),
                    Operators = new() { "eq", "contains", "starts", "ends", "neq", "notcontains" },
                },
                ["createdAt"] = new()
                {
                    Column = "created_at",
                    DataType = typeof(DateTime),
                    Operators = new() { "eq", "lt", "lte", "gt", "gte" },
                },
                ["updatedAt"] = new()
                {
                    Column = "updated_at",
                    DataType = typeof(DateTime),
                    Operators = new() { "eq", "lt", "lte", "gt", "gte" },
                },
            };
    }
}
