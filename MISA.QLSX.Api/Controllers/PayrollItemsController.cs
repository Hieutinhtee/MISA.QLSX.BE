using Microsoft.AspNetCore.Mvc;
using MISA.QLSX.Core.Entities;
using MISA.QLSX.Core.Interfaces.Service;

namespace MISA.QLSX.Api.Controllers
{
    public class PayrollItemsController : BaseController<PayrollItem>
    {
        private readonly IPayrollItemService _service;

        public PayrollItemsController(IPayrollItemService service)
            : base(service)
        {
            _service = service;
        }

        [HttpGet("payroll/{payrollId}")]
        public async Task<IActionResult> GetByPayrollId(Guid payrollId)
        {
            var result = await _service.GetByPayrollIdAsync(payrollId);
            return Ok(result);
        }
    }
}
