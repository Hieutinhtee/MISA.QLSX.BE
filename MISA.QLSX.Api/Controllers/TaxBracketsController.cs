using MISA.QLSX.Core.Entities;
using MISA.QLSX.Core.Interfaces.Service;

namespace MISA.QLSX.Api.Controllers
{
    public class TaxBracketsController : BaseController<TaxBracket>
    {
        public TaxBracketsController(ITaxBracketService service)
            : base(service) { }
    }
}
