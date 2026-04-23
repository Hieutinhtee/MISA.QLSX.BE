using MISA.QLSX.Core.Entities;

namespace MISA.QLSX.Core.Interfaces.Repository
{
    /// <summary>
    /// Định nghĩa truy cập dữ liệu chính sách lương.
    /// </summary>
    public interface ISalaryPolicyRepository : IBaseRepository<SalaryPolicy>
    {
        /// <summary>
        /// Lấy chính sách lương hiệu lực tại một thời điểm.
        /// </summary>
        /// <param name="atDate">Thời điểm cần tra cứu hiệu lực.</param>
        /// <returns>Chính sách lương hiệu lực gần nhất; null nếu không tìm thấy.</returns>
        Task<SalaryPolicy?> GetEffectiveAtAsync(DateTime atDate);
    }
}
