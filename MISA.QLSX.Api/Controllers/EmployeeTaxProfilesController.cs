using MISA.QLSX.Core.Entities;
using MISA.QLSX.Core.Interfaces.Service;

namespace MISA.QLSX.Api.Controllers
{
    public class EmployeeTaxProfilesController : BaseController<EmployeeTaxProfile>
    {
        public EmployeeTaxProfilesController(IEmployeeTaxProfileService service)
            : base(service) { }
    }
}
