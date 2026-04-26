using MISA.QLSX.Core.Entities;
using MISA.QLSX.Core.Interfaces.Repository;
using MISA.QLSX.Core.Interfaces.Service;

namespace MISA.QLSX.Core.Services
{
    public class AllowanceService : BaseServices<Allowance>, IAllowanceService
    {
        /// <summary>
        /// Khởi tạo dịch vụ phụ cấp.
        /// </summary>
        /// <param name="repo">Repository xử lý dữ liệu phụ cấp.</param>
        public AllowanceService(IAllowanceRepository repo)
            : base(repo) { }
    }
}
