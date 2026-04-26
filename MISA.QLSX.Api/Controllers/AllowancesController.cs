using Microsoft.AspNetCore.Mvc;
using MISA.QLSX.Core.Entities;
using MISA.QLSX.Core.Interfaces.Service;

namespace MISA.QLSX.Api.Controllers
{
    public class AllowancesController : BaseController<Allowance>
    {
        private readonly IAllowanceService _allowanceService;

        public AllowancesController(IAllowanceService allowanceService)
            : base(allowanceService)
        {
            _allowanceService = allowanceService;
        }
    }
}
