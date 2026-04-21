using MISA.QLSX.Core.Entities;
using MISA.QLSX.Core.Interfaces.Service;

namespace MISA.QLSX.Api.Controllers
{
    public class SalaryPeriodsController : BaseController<SalaryPeriod>
    {
        public SalaryPeriodsController(ISalaryPeriodService service)
            : base(service) { }
    }
}