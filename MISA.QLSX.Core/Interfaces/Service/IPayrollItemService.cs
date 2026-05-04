using MISA.QLSX.Core.Entities;

namespace MISA.QLSX.Core.Interfaces.Service
{
    public interface IPayrollItemService : IBaseService<PayrollItem>
    {
        /// <summary>
        /// Lấy danh sách các khoản mục lương theo định danh bảng lương.
        /// </summary>
        /// <param name="payrollId">Định danh bảng lương.</param>
        /// <returns>Danh sách khoản mục lương thuộc bảng lương.</returns>
        Task<List<PayrollItem>> GetByPayrollIdAsync(Guid payrollId);
    }
}