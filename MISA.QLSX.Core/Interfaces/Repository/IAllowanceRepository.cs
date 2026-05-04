using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MISA.QLSX.Core.Entities;

namespace MISA.QLSX.Core.Interfaces.Repository
{
    /// <summary>
    /// Interface để truy vấn dữ liệu phụ cấp.
    /// </summary>
    public interface IAllowanceRepository : IBaseRepository<Allowance>
    {
        /// <summary>
        /// Lấy danh sách phụ cấp theo danh sách ID.
        /// </summary>
        /// <param name="ids">Danh sách ID phụ cấp.</param>
        /// <returns>Danh sách phụ cấp tương ứng.</returns>
        Task<List<Allowance>> GetByIdsAsync(List<Guid> ids);
    }
}
