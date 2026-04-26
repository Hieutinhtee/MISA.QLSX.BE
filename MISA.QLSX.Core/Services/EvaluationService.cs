using MISA.QLSX.Core.Entities;
using MISA.QLSX.Core.Interfaces.Repository;
using MISA.QLSX.Core.Interfaces.Service;

namespace MISA.QLSX.Core.Services
{
    public class EvaluationService : BaseServices<Evaluation>, IEvaluationService
    {
        /// <summary>
        /// Khởi tạo dịch vụ đánh giá.
        /// </summary>
        /// <param name="repo">Repository xử lý dữ liệu đánh giá.</param>
        public EvaluationService(IEvaluationRepository repo)
            : base(repo) { }
    }
}
