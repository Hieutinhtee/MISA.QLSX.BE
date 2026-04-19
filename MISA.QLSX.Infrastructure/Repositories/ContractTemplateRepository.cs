using MISA.QLSX.Core.DTOs.Requests;
using MISA.QLSX.Core.Entities;
using MISA.QLSX.Core.Interfaces.Repository;
using MISA.QLSX.Infrastructure.Connection;

namespace MISA.QLSX.Infrastructure.Repositories
{
    public class ContractTemplateRepository : BaseRepository<ContractTemplate>, IContractTemplateRepository
    {
        public ContractTemplateRepository(MySqlConnectionFactory factory)
            : base(factory) { }

        protected override HashSet<string> GetSearchFields()
        {
            return new HashSet<string>
            {
                "template_code",
                "template_name",
                "contract_type",
                "content",
            };
        }

        protected override Dictionary<string, FieldMapItem> FieldMap =>
            new()
            {
                ["templateCode"] = new() { Column = "template_code", DataType = typeof(string), Operators = new() { "eq", "notcontains", "contains", "starts", "ends", "neq" } },
                ["templateName"] = new() { Column = "template_name", DataType = typeof(string), Operators = new() { "eq", "notcontains", "contains", "starts", "ends", "neq" } },
                ["contractType"] = new() { Column = "contract_type", DataType = typeof(string), Operators = new() { "eq", "notcontains", "contains", "starts", "ends", "neq" } },
                ["content"] = new() { Column = "content", DataType = typeof(string), Operators = new() { "eq", "notcontains", "contains", "starts", "ends", "neq" } },
                ["version"] = new() { Column = "version", DataType = typeof(int), Operators = new() { "eq", "lt", "lte", "gt", "gte", "neq" } },
                ["isActive"] = new() { Column = "is_active", DataType = typeof(bool), Operators = new() { "eq", "active", "inactive" } },
                ["createdAt"] = new() { Column = "created_at", DataType = typeof(DateTime), Operators = new() { "eq", "lt", "lte", "gt", "gte" } },
                ["updatedAt"] = new() { Column = "updated_at", DataType = typeof(DateTime), Operators = new() { "eq", "lt", "lte", "gt", "gte" } },
            };
    }
}
