using System;
using System.Collections.Generic;
using MISA.QLSX.Core.DTOs.Requests;
using MISA.QLSX.Core.Entities;
using MISA.QLSX.Core.Interfaces.Repository;
using MISA.QLSX.Infrastructure.Connection;

namespace MISA.QLSX.Infrastructure.Repositories
{
    /// <summary>
    /// Repository truy cập dữ liệu danh mục bằng cấp.
    /// </summary>
    public class DegreeRepository : BaseRepository<Degree>, IDegreeRepository
    {
        /// <summary>
        /// Khởi tạo repository bằng cấp.
        /// </summary>
        /// <param name="factory">Factory tạo kết nối cơ sở dữ liệu.</param>
        public DegreeRepository(MySqlConnectionFactory factory)
            : base(factory) { }

        /// <summary>
        /// Danh sách cột cho phép tìm kiếm nhanh bằng từ khóa.
        /// </summary>
        /// <returns>Tập cột được phép tìm kiếm.</returns>
        protected override HashSet<string> GetSearchFields()
        {
            return new HashSet<string>
            {
                "degree_code",
                "degree_name",
                "description",
            };
        }

        /// <summary>
        /// Cấu hình map field từ FE sang cột DB cho filter/sort.
        /// </summary>
        protected override Dictionary<string, FieldMapItem> FieldMap =>
            new()
            {
                ["degreeCode"] = new() { Column = "degree_code", DataType = typeof(string), Operators = new() { "eq", "contains", "starts", "ends", "neq", "notcontains" } },
                ["degreeName"] = new() { Column = "degree_name", DataType = typeof(string), Operators = new() { "eq", "contains", "starts", "ends", "neq", "notcontains" } },
                ["description"] = new() { Column = "description", DataType = typeof(string), Operators = new() { "eq", "contains", "starts", "ends", "neq", "notcontains", "isnull", "notnull" } },
                ["createdAt"] = new() { Column = "created_at", DataType = typeof(DateTime), Operators = new() { "eq", "lt", "lte", "gt", "gte" } },
                ["updatedAt"] = new() { Column = "updated_at", DataType = typeof(DateTime), Operators = new() { "eq", "lt", "lte", "gt", "gte" } },
            };
    }
}
