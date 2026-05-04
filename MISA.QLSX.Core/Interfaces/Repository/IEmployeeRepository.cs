using MISA.QLSX.Core.Entities;

namespace MISA.QLSX.Core.Interfaces.Repository
{
    public interface IEmployeeRepository : IBaseRepository<Employee>
    {
        /// <summary>
        /// Lấy danh sách nhân viên theo mã vai trò.
        /// </summary>
        /// <param name="roleCode">Mã vai trò (VD: HR, ADMIN, MANAGER).</param>
        /// <returns>Danh sách nhân viên.</returns>
        Task<List<Employee>> GetEmployeesByRoleAsync(string roleCode);
    }
}
