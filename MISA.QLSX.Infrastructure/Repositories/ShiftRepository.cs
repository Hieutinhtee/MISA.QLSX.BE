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
                "shift_code",
                "shift_name",
                "updated_by",
                "created_by",
            };
        }

        protected override Dictionary<string, FieldMapItem> FieldMap =>
            new()
            {
                ["shiftCode"] = new()
                {
                    Column = "shift_code",
                    DataType = typeof(string),
                    Operators = new() { "eq", "notcontains", "contains", "starts", "ends", "neq" },
                },

                ["shiftName"] = new()
                {
                    Column = "shift_name",
                    DataType = typeof(string),
                    Operators = new() { "eq", "notcontains", "contains", "starts", "ends", "neq" },
                },

                ["startTime"] = new()
                {
                    Column = "start_time",
                    DataType = typeof(TimeSpan),
                    Operators = new() { "eq", "lt", "lte", "gt", "gte" },
                },

                ["endTime"] = new()
                {
                    Column = "end_time",
                    DataType = typeof(TimeSpan),
                    Operators = new() { "eq", "lt", "lte", "gt", "gte" },
                },

                ["breakStartTime"] = new()
                {
                    Column = "break_start_time",
                    DataType = typeof(TimeSpan),
                    Operators = new() { "eq", "lt", "lte", "gt", "gte" },
                },

                ["breakEndTime"] = new()
                {
                    Column = "break_end_time",
                    DataType = typeof(TimeSpan),
                    Operators = new() { "eq", "lt", "lte", "gt", "gte" },
                },

                ["workingHours"] = new()
                {
                    Column = "working_hours",
                    DataType = typeof(decimal),
                    Operators = new() { "eq", "lt", "lte", "gt", "gte" },
                },

                ["breakHours"] = new()
                {
                    Column = "break_hours",
                    DataType = typeof(decimal),
                    Operators = new() { "eq", "lt", "lte", "gt", "gte" },
                },

                ["isActive"] = new()
                {
                    Column = "is_active",
                    DataType = typeof(bool),
                    Operators = new() { "eq", "active", "inactive" },
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
                ["createdBy"] = new()
                {
                    Column = "created_by",
                    DataType = typeof(string),
                    Operators = new() { "eq", "notcontains", "contains", "starts", "ends", "neq" },
                },

                ["updatedBy"] = new()
                {
                    Column = "updated_by",
                    DataType = typeof(string),
                    Operators = new() { "eq", "notcontains", "contains", "starts", "ends", "neq" },
                },
            };

        #endregion Method
    }
}
