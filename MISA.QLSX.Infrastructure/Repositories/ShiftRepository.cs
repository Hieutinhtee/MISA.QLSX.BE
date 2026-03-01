using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
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

        #endregion Method
    }
}
