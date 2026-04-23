using MISA.QLSX.Core.Entities;

namespace MISA.QLSX.Core.Interfaces.Repository
{
    /// <summary>
    /// Định nghĩa truy cập dữ liệu snapshot bảng lương.
    /// </summary>
    public interface IPayrollSnapshotRepository : IBaseRepository<PayrollSnapshot>
    {
        /// <summary>
        /// Lấy snapshot theo danh sách định danh bảng lương.
        /// </summary>
        /// <param name="payrollIds">Danh sách định danh bảng lương.</param>
        /// <returns>Danh sách snapshot thuộc các bảng lương được chỉ định.</returns>
        Task<List<PayrollSnapshot>> GetByPayrollIdsAsync(List<Guid> payrollIds);
    }
}