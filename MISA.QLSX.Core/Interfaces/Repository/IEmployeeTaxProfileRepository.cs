using MISA.QLSX.Core.Entities;

namespace MISA.QLSX.Core.Interfaces.Repository
{
    /// <summary>
    /// Định nghĩa truy cập dữ liệu hồ sơ thuế nhân viên.
    /// </summary>
    public interface IEmployeeTaxProfileRepository : IBaseRepository<EmployeeTaxProfile>
    {
        /// <summary>
        /// Lấy hồ sơ thuế hiệu lực của danh sách nhân viên tại một thời điểm.
        /// </summary>
        /// <param name="employeeIds">Danh sách định danh nhân viên.</param>
        /// <param name="atDate">Thời điểm cần tra cứu hiệu lực.</param>
        /// <returns>Danh sách hồ sơ thuế hiệu lực theo nhân viên.</returns>
        Task<List<EmployeeTaxProfile>> GetEffectiveByEmployeesAsync(List<Guid> employeeIds, DateTime atDate);
    }
}
