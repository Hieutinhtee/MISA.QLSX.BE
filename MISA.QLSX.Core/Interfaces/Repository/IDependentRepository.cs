using MISA.QLSX.Core.Entities;

namespace MISA.QLSX.Core.Interfaces.Repository
{
    public interface IDependentRepository : IBaseRepository<Dependent>
    {
        /// <summary>
        /// Lấy danh sách người phụ thuộc của một nhân viên
        /// </summary>
        /// <param name="employeeId">ID nhân viên</param>
        /// <returns>Danh sách người phụ thuộc</returns>
        Task<List<Dependent>> GetByEmployeeIdAsync(Guid employeeId);
    }
}
