using MISA.QLSX.Core.Entities;
using MISA.QLSX.Core.Interfaces.Service;

namespace MISA.QLSX.Api.Controllers
{
    public class DeductionPoliciesController : BaseController<DeductionPolicy>
    {
        public DeductionPoliciesController(IDeductionPolicyService service)
            : base(service) { }
    }
}
