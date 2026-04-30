using MISA.QLSX.Core.Entities;
using MISA.QLSX.Core.Interfaces.Repository;
using MISA.QLSX.Core.DTOs.Requests;
using MISA.QLSX.Infrastructure.Connection;

namespace MISA.QLSX.Infrastructure.Repositories
{
    public class BusinessTripRepository : BaseRepository<BusinessTrip>, IBusinessTripRepository
    {
        private const string ReadViewName = "vw_business_trip_detail";

        /// <summary>
        /// Khởi tạo repository công tác với factory tạo kết nối MySQL.
        /// </summary>
        /// <param name="factory">Factory tạo kết nối cơ sở dữ liệu.</param>
        public BusinessTripRepository(MySqlConnectionFactory factory)
            : base(factory) { }

        protected override string GetReadTableName() => ReadViewName;

        /// <summary>
        /// Trả về tập cột được hỗ trợ cho tìm kiếm nhanh công tác.
        /// </summary>
        /// <returns>Tập tên cột dùng cho tìm kiếm.</returns>
        protected override HashSet<string> GetSearchFields()
        {
            return new HashSet<string>
            {
                "business_trip_code",
                "employee_code",
                "employee_name",
                "location",
                "purpose",
            };
        }

        protected override Dictionary<string, FieldMapItem> FieldMap =>
            new()
            {
                ["businessTripCode"] = new() { Column = "business_trip_code", DataType = typeof(string), Operators = new() { "eq", "contains", "starts", "ends", "neq", "notcontains" } },
                ["employeeId"] = new() { Column = "employee_id", DataType = typeof(Guid), Operators = new() { "eq", "neq", "isnull", "notnull" } },
                ["employeeCode"] = new() { Column = "employee_code", DataType = typeof(string), Operators = new() { "eq", "contains", "starts", "ends", "neq", "notcontains" } },
                ["employeeName"] = new() { Column = "employee_name", DataType = typeof(string), Operators = new() { "eq", "contains", "starts", "ends", "neq", "notcontains" } },
                ["startDate"] = new() { Column = "start_date", DataType = typeof(DateTime), Operators = new() { "eq", "lt", "lte", "gt", "gte", "isnull", "notnull" } },
                ["endDate"] = new() { Column = "end_date", DataType = typeof(DateTime), Operators = new() { "eq", "lt", "lte", "gt", "gte", "isnull", "notnull" } },
                ["location"] = new() { Column = "location", DataType = typeof(string), Operators = new() { "eq", "contains", "starts", "ends", "neq", "notcontains" } },
                ["purpose"] = new() { Column = "purpose", DataType = typeof(string), Operators = new() { "eq", "contains", "starts", "ends", "neq", "notcontains" } },
                ["supportAmount"] = new() { Column = "support_amount", DataType = typeof(decimal), Operators = new() { "eq", "lt", "lte", "gt", "gte", "neq", "isnull", "notnull" } },
                ["createdAt"] = new() { Column = "created_at", DataType = typeof(DateTime), Operators = new() { "eq", "lt", "lte", "gt", "gte" } },
                ["updatedAt"] = new() { Column = "updated_at", DataType = typeof(DateTime), Operators = new() { "eq", "lt", "lte", "gt", "gte" } },
            };
    }
}

