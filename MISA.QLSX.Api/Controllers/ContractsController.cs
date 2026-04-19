using MISA.QLSX.Core.Entities;
using MISA.QLSX.Core.Interfaces.Service;

namespace MISA.QLSX.Api.Controllers
{
    public class ContractsController : BaseController<Contract>
    {
        public ContractsController(IContractService contractService)
            : base(contractService) { }
    }
}
