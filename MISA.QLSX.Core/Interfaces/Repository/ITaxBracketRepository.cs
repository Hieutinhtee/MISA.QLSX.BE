using MISA.QLSX.Core.Entities;

namespace MISA.QLSX.Core.Interfaces.Repository
{
    /// <summary>
    /// Định nghĩa truy cập dữ liệu bậc thuế thu nhập cá nhân.
    /// </summary>
    public interface ITaxBracketRepository : IBaseRepository<TaxBracket>
    {
        /// <summary>
        /// Lấy danh sách bậc thuế hiệu lực tại một thời điểm.
        /// </summary>
        /// <param name="atDate">Thời điểm cần tra cứu hiệu lực.</param>
        /// <returns>Danh sách bậc thuế hiệu lực, sắp theo cận dưới tăng dần.</returns>
        Task<List<TaxBracket>> GetEffectiveAtAsync(DateTime atDate);
    }
}
