using MISA.QLSX.Core.DTOs.Requests;
using MISA.QLSX.Core.Entities;
using MISA.QLSX.Core.Interfaces.Repository;
using MISA.QLSX.Infrastructure.Connection;

namespace MISA.QLSX.Infrastructure.Repositories
{
    /// <summary>
    /// Repository chức vụ.
    /// </summary>
    public class PositionRepository : BaseRepository<Position>, IPositionRepository
    {
        public PositionRepository(MySqlConnectionFactory factory)
            : base(factory) { }

        protected override HashSet<string> GetSearchFields()
        {
            return new HashSet<string> { "position_code", "position_name" };
        }

        protected override Dictionary<string, FieldMapItem> FieldMap =>
            new()
            {
                ["positionCode"] = new()
                {
                    Column = "position_code",
                    DataType = typeof(string),
                    Operators = new() { "eq", "contains", "starts", "ends", "neq", "notcontains" },
                },
                ["positionName"] = new()
                {
                    Column = "position_name",
                    DataType = typeof(string),
                    Operators = new() { "eq", "contains", "starts", "ends", "neq", "notcontains" },
                },
                ["allowance"] = new()
                {
                    Column = "allowance",
                    DataType = typeof(decimal),
                    Operators = new() { "eq", "lt", "lte", "gt", "gte", "neq" },
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
