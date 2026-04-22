using MISA.QLSX.Core.Entities;

namespace MISA.QLSX.Core.Interfaces.Repository
{
    /// <summary>
    /// Định nghĩa truy cập dữ liệu chi tiết thành phần lương.
    /// </summary>
    public interface IPayrollItemRepository : IBaseRepository<PayrollItem>
    {
        /// <summary>
        /// Lấy danh sách chi tiết thành phần lương theo danh sách bảng lương.
        /// </summary>
        /// <param name="payrollIds">Danh sách định danh bảng lương.</param>
        /// <returns>Danh sách bản ghi chi tiết lương thuộc các bảng lương được chỉ định.</returns>
        Task<List<PayrollItem>> GetByPayrollIdsAsync(List<Guid> payrollIds);
    }
}