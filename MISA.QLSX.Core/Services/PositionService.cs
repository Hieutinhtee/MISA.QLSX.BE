using MISA.QLSX.Core.Entities;
using MISA.QLSX.Core.Interfaces.Repository;
using MISA.QLSX.Core.Interfaces.Service;

namespace MISA.QLSX.Core.Services
{
    /// <summary>
    /// Dịch vụ nghiệp vụ chức vụ.
    /// </summary>
    public class PositionService : BaseServices<Position>, IPositionService
    {
        public PositionService(IPositionRepository repo) : base(repo) { }
    }
}
