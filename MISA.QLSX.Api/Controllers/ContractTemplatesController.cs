using MISA.QLSX.Core.Entities;
using MISA.QLSX.Core.Interfaces.Service;

namespace MISA.QLSX.Api.Controllers
{
    public class ContractTemplatesController : BaseController<ContractTemplate>
    {
        public ContractTemplatesController(IContractTemplateService contractTemplateService)
            : base(contractTemplateService) { }
    }
}
