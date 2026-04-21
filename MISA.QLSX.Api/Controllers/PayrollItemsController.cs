using MISA.QLSX.Core.Entities;
using MISA.QLSX.Core.Interfaces.Service;

namespace MISA.QLSX.Api.Controllers
{
    public class PayrollItemsController : BaseController<PayrollItem>
    {
        public PayrollItemsController(IPayrollItemService service)
            : base(service) { }
    }
}