using MISA.QLSX.Core.Entities;
using MISA.QLSX.Core.Interfaces.Repository;
using MISA.QLSX.Core.Interfaces.Service;

namespace MISA.QLSX.Core.Services
{
    /// <summary>
    /// Dịch vụ nghiệp vụ phòng ban.
    /// </summary>
    public class DepartmentService : BaseServices<Department>, IDepartmentService
    {
        public DepartmentService(IDepartmentRepository repo) : base(repo) { }
    }
}
