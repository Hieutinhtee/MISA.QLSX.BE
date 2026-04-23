using MISA.QLSX.Core.Entities;

namespace MISA.QLSX.Core.Interfaces.Repository
{
    /// <summary>
    /// Định nghĩa truy cập dữ liệu chính sách giảm trừ và bảo hiểm.
    /// </summary>
    public interface IDeductionPolicyRepository : IBaseRepository<DeductionPolicy>
    {
        /// <summary>
        /// Lấy chính sách giảm trừ/bảo hiểm hiệu lực tại một thời điểm.
        /// </summary>
        /// <param name="atDate">Thời điểm cần tra cứu hiệu lực.</param>
        /// <returns>Chính sách giảm trừ hiệu lực gần nhất; null nếu không tìm thấy.</returns>
        Task<DeductionPolicy?> GetEffectiveAtAsync(DateTime atDate);
    }
}
