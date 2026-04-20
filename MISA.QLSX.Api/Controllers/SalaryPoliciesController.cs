using MISA.QLSX.Core.Entities;
using MISA.QLSX.Core.Interfaces.Service;

namespace MISA.QLSX.Api.Controllers
{
    public class SalaryPoliciesController : BaseController<SalaryPolicy>
    {
        public SalaryPoliciesController(ISalaryPolicyService service)
            : base(service) { }
    }
}
