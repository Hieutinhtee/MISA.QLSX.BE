using MISA.QLSX.Core.Entities;
using MISA.QLSX.Core.Interfaces.Repository;
using MISA.QLSX.Core.DTOs.Requests;
using MISA.QLSX.Infrastructure.Connection;

namespace MISA.QLSX.Infrastructure.Repositories
{
    public class EvaluationRepository : BaseRepository<Evaluation>, IEvaluationRepository
    {
        private const string ReadViewName = "vw_evaluation_detail";

        /// <summary>
        /// Khởi tạo repository đánh giá với factory tạo kết nối MySQL.
        /// </summary>
        /// <param name="factory">Factory tạo kết nối cơ sở dữ liệu.</param>
        public EvaluationRepository(MySqlConnectionFactory factory)
            : base(factory) { }

        protected override string GetReadTableName() => ReadViewName;

        /// <summary>
        /// Trả về tập cột được hỗ trợ cho tìm kiếm nhanh đánh giá.
        /// </summary>
        /// <returns>Tập tên cột dùng cho tìm kiếm.</returns>
        protected override HashSet<string> GetSearchFields()
        {
            return new HashSet<string>
            {
                "evaluation_code",
                "employee_code",
                "employee_name",
                "evaluation_type",
                "reason",
            };
        }

        protected override Dictionary<string, FieldMapItem> FieldMap =>
            new()
            {
                ["evaluationCode"] = new() { Column = "evaluation_code", DataType = typeof(string), Operators = new() { "eq", "contains", "starts", "ends", "neq", "notcontains" } },
                ["employeeId"] = new() { Column = "employee_id", DataType = typeof(Guid), Operators = new() { "eq", "neq", "isnull", "notnull" } },
                ["employeeCode"] = new() { Column = "employee_code", DataType = typeof(string), Operators = new() { "eq", "contains", "starts", "ends", "neq", "notcontains" } },
                ["employeeName"] = new() { Column = "employee_name", DataType = typeof(string), Operators = new() { "eq", "contains", "starts", "ends", "neq", "notcontains" } },
                ["evaluationType"] = new() { Column = "evaluation_type", DataType = typeof(string), Operators = new() { "eq", "contains", "starts", "ends", "neq", "notcontains" } },
                ["reason"] = new() { Column = "reason", DataType = typeof(string), Operators = new() { "eq", "contains", "starts", "ends", "neq", "notcontains" } },
                ["amount"] = new() { Column = "amount", DataType = typeof(decimal), Operators = new() { "eq", "lt", "lte", "gt", "gte", "neq", "isnull", "notnull" } },
                ["evaluationDate"] = new() { Column = "evaluation_date", DataType = typeof(DateTime), Operators = new() { "eq", "lt", "lte", "gt", "gte", "isnull", "notnull" } },
                ["createdAt"] = new() { Column = "created_at", DataType = typeof(DateTime), Operators = new() { "eq", "lt", "lte", "gt", "gte" } },
                ["updatedAt"] = new() { Column = "updated_at", DataType = typeof(DateTime), Operators = new() { "eq", "lt", "lte", "gt", "gte" } },
            };
    }
}

