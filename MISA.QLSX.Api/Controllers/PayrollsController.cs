using Microsoft.AspNetCore.Mvc;
using MISA.QLSX.Core.Entities;
using MISA.QLSX.Core.Interfaces.Service;

namespace MISA.QLSX.Api.Controllers
{
    public class PayrollsController : BaseController<Payroll>
    {
        private readonly IPayrollService _payrollService;

        public PayrollsController(IPayrollService service)
            : base(service)
        {
            _payrollService = service;
        }

        [HttpPost("periods/{salaryPeriodId:guid}/generate")]
        public async Task<IActionResult> Generate(Guid salaryPeriodId)
        {
            var totalCreated = await _payrollService.GenerateDraftPayrollsAsync(salaryPeriodId);
            return Ok(new { totalCreated, message = "Generate payroll thành công" });
        }

        [HttpPost("periods/{salaryPeriodId:guid}/calculate")]
        public async Task<IActionResult> Calculate(Guid salaryPeriodId, [FromQuery] Guid? employeeId = null)
        {
            var totalCalculated = await _payrollService.CalculatePayrollsAsync(salaryPeriodId, employeeId);
            return Ok(new { totalCalculated, message = "Tính payroll thành công" });
        }

        [HttpPost("periods/{salaryPeriodId:guid}/lock")]
        public async Task<IActionResult> Lock(Guid salaryPeriodId)
        {
            var totalLocked = await _payrollService.LockPayrollsAsync(salaryPeriodId);
            return Ok(new { totalLocked, message = "Khóa payroll thành công" });
        }

        [HttpPost("periods/{salaryPeriodId:guid}/pay")]
        public async Task<IActionResult> Pay(Guid salaryPeriodId)
        {
            var totalPaid = await _payrollService.MarkPayrollsPaidAsync(salaryPeriodId);
            return Ok(new { totalPaid, message = "Chi trả payroll thành công" });
        }
    }
}