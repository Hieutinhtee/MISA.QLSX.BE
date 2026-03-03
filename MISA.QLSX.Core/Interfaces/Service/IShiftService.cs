using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MISA.QLSX.Core.Entities;

namespace MISA.QLSX.Core.Interfaces.Service
{
    public interface IShiftService : IBaseService<Shift>
    {
        /// <summary>
        /// Thực hiện cập nhật hàng loạt trạng thái ca làm việc
        /// </summary>
        /// <param name="ids">Danh sách ID (Guid) của các bản ghi ca làm việc</param>
        /// Created by TMHieu - 7/12/2025
        Task<int> UpdateIsActiveMany(List<Guid> ids, bool isActive);
    }
}
