using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using MISA.QLSX.Core.DTOs.Requests;
using MISA.QLSX.Core.Entities;
using MISA.QLSX.Core.Interfaces.Repository;
using MISA.QLSX.Infrastructure.Connection;
using MySqlConnector;

namespace MISA.QLSX.Infrastructure.Repositories
{
    /// <summary>
    /// Repository xử lý các thao tác tương tác với Database của Entity Customer
    /// <para/>Triển khai interface ICustomerRepository, kế thừa các thao tác CRUD cơ bản từ BaseRepository
    /// </summary>
    /// Created by TMHieu - 28/2/2026
    public class ShiftRepository : BaseRepository<Shift>, IShiftRepository
    {
        #region Constructor

        /// <summary>
        /// Hàm khởi tạo Repository
        /// </summary>
        /// <param name="factory">Factory tạo kết nối MySQL (được tiêm vào)</param>
        /// Created by TMHieu - 28/2/2026
        public ShiftRepository(MySqlConnectionFactory factory)
            : base(factory) { }

        #endregion Constructor

        #region Method


        /// <summary>
        /// Định nghĩa danh sách các trường (cột) cho phép tìm kiếm
        /// </summary>
        /// <returns>HashSet chứa tên các cột cho phép tìm kiếm</returns>
        /// Created by TMHieu - 28/2/2026
        protected override HashSet<string> GetSearchFields()
        {
            return new HashSet<string>
            {
                "production_shift_code",
                "production_shift_name",
                "production_shift_modified_by",
                "production_shift_created_by",
            };
        }

        protected override Dictionary<string, FieldMapItem> FieldMap =>
            new()
            {
                ["shiftCode"] = new()
                {
                    Column = "production_shift_code",
                    DataType = typeof(string),
                    Operators = new() { "eq", "contains", "starts", "ends", "neq" },
                },

                ["shiftName"] = new()
                {
                    Column = "production_shift_name",
                    DataType = typeof(string),
                    Operators = new() { "eq", "contains", "starts", "ends", "neq" },
                },

                ["shiftBeginTime"] = new()
                {
                    Column = "production_shift_begin_time",
                    DataType = typeof(TimeSpan),
                    Operators = new() { "eq", "lt", "lte", "gt", "gte" },
                },

                ["shiftEndTime"] = new()
                {
                    Column = "production_shift_end_time",
                    DataType = typeof(TimeSpan),
                    Operators = new() { "eq", "lt", "lte", "gt", "gte" },
                },

                ["beginBreakTime"] = new()
                {
                    Column = "production_shift_begin_break_time",
                    DataType = typeof(TimeSpan),
                    Operators = new() { "eq", "lt", "lte", "gt", "gte" },
                },

                ["endBreakTime"] = new()
                {
                    Column = "production_shift_end_break_time",
                    DataType = typeof(TimeSpan),
                    Operators = new() { "eq", "lt", "lte", "gt", "gte" },
                },

                ["workingTime"] = new()
                {
                    Column = "production_shift_working_time",
                    DataType = typeof(decimal),
                    Operators = new() { "eq", "lt", "lte", "gt", "gte" },
                },

                ["breakTime"] = new()
                {
                    Column = "production_shift_break_time",
                    DataType = typeof(decimal),
                    Operators = new() { "eq", "lt", "lte", "gt", "gte" },
                },

                ["isActive"] = new()
                {
                    Column = "production_shift_is_active",
                    DataType = typeof(bool),
                    Operators = new() { "eq", "active", "inactive" },
                },

                ["createdDate"] = new()
                {
                    Column = "production_shift_created_date",
                    DataType = typeof(DateTime),
                    Operators = new() { "eq", "lt", "lte", "gt", "gte" },
                },

                ["modifiedDate"] = new()
                {
                    Column = "production_shift_modified_date",
                    DataType = typeof(DateTime),
                    Operators = new() { "eq", "lt", "lte", "gt", "gte" },
                },
            };

        #endregion Method
    }
}
