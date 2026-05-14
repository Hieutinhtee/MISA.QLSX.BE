using MISA.QLSX.Core.Entities;
using MISA.QLSX.Core.Interfaces.Service;

namespace MISA.QLSX.Core.Interfaces.Service
{
    public interface IDependentService : IBaseService<Dependent>
    {
        /// <summary>
        /// Lấy danh sách người phụ thuộc theo ID nhân viên
        /// </summary>
        /// <param name="employeeId">ID nhân viên</param>
        /// <returns>Danh sách người phụ thuộc</returns>
        Task<List<Dependent>> GetByEmployeeIdAsync(Guid employeeId);
    }
}
