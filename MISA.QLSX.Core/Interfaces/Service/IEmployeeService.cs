using MISA.QLSX.Core.Entities;

namespace MISA.QLSX.Core.Interfaces.Service
{
    /// <summary>
    /// Service nghiệp vụ cho nhân viên.
    /// </summary>
    public interface IEmployeeService : IBaseService<Employee>
    {
        /// <summary>
        /// Lấy danh sách nhân viên chưa được gán hợp đồng.
        /// </summary>
        /// <returns>Danh sách nhân viên có ContractId = null.</returns>
        Task<List<Employee>> GetEmployeesWithoutContractAsync();
    }
}
