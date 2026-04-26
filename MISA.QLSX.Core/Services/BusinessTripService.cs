using MISA.QLSX.Core.Entities;
using MISA.QLSX.Core.Interfaces.Repository;
using MISA.QLSX.Core.Interfaces.Service;

namespace MISA.QLSX.Core.Services
{
    public class BusinessTripService : BaseServices<BusinessTrip>, IBusinessTripService
    {
        /// <summary>
        /// Khởi tạo dịch vụ công tác.
        /// </summary>
        /// <param name="repo">Repository xử lý dữ liệu công tác.</param>
        public BusinessTripService(IBusinessTripRepository repo)
            : base(repo) { }
    }
}
