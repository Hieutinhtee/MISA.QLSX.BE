using Microsoft.AspNetCore.Mvc;
using MISA.QLSX.Api.Authorization;
using MISA.QLSX.Core.DTOs.Requests;
using MISA.QLSX.Core.DTOs.Responses;
using MISA.QLSX.Core.Entities;
using MISA.QLSX.Core.Interfaces.Service;

namespace MISA.QLSX.Api.Controllers
{
    [RequireRole("ADMIN", "HR", "MANAGER", "EMPLOYEE")]
    public class PayrollsController : BaseController<Payroll>
    {
        private readonly IPayrollService _payrollService;

        public PayrollsController(IPayrollService service)
            : base(service)
        {
            _payrollService = service;
        }

        /// <summary>
        /// Lấy toàn bộ bảng lương, EMPLOYEE không được gọi endpoint này.
        /// </summary>
        /// <returns>Danh sách bảng lương.</returns>
        [RequireRole("ADMIN", "HR", "MANAGER")]
        public override async Task<IActionResult> GetAll()
        {
            return await base.GetAll();
        }

        /// <summary>
        /// Lấy danh sách bảng lương theo phân trang với ràng buộc EMPLOYEE chỉ xem bản thân.
        /// </summary>
        /// <param name="request">Yêu cầu phân trang/tìm kiếm/lọc.</param>
        /// <returns>Dữ liệu bảng lương theo quyền truy cập.</returns>
        [HttpPost("paging")]
        public override async Task<PagingResponse<Payroll>> GetPaging([FromBody] QueryRequest request)
        {
            request ??= new QueryRequest();
            request.Filters ??= new List<FilterCondition>();

            var role = HttpContext.Session.GetString("role_code")?.ToUpperInvariant();
            var employeeId = HttpContext.Session.GetString("employee_id");

            if (role == "EMPLOYEE" && Guid.TryParse(employeeId, out var parsedEmployeeId))
            {
                request.Filters.RemoveAll(f => f.Field == "employeeId");
                request.Filters.Add(
                    new FilterCondition { Field = "employeeId", Operator = "eq", Value = parsedEmployeeId }
                );
            }

            return await _payrollService.QueryPagingAsync(request);
        }

        /// <summary>
        /// Sinh bảng lương nháp theo kỳ lương.
        /// </summary>
        /// <param name="salaryPeriodId">ID kỳ lương cần sinh bảng lương.</param>
        /// <returns>Số lượng bảng lương được tạo.</returns>
        [RequireRole("ADMIN", "HR")]
        [HttpPost("periods/{salaryPeriodId:guid}/generate")]
        public async Task<IActionResult> Generate(Guid salaryPeriodId)
        {
            var totalCreated = await _payrollService.GenerateDraftPayrollsAsync(salaryPeriodId);
            return Ok(new { totalCreated, message = "Generate payroll thành công" });
        }

        /// <summary>
        /// Tính lương theo kỳ lương.
        /// </summary>
        /// <param name="salaryPeriodId">ID kỳ lương cần tính lương.</param>
        /// <param name="employeeId">ID nhân viên cần tính lương (tùy chọn).</param>
        /// <returns>Số lượng bảng lương đã tính.</returns>
        [RequireRole("ADMIN", "HR")]
        [HttpPost("periods/{salaryPeriodId:guid}/calculate")]
        public async Task<IActionResult> Calculate(Guid salaryPeriodId, [FromQuery] Guid? employeeId = null)
        {
            var totalCalculated = await _payrollService.CalculatePayrollsAsync(salaryPeriodId, employeeId);
            return Ok(new { totalCalculated, message = "Tính payroll thành công" });
        }

        /// <summary>
        /// Khóa kỳ lương, chỉ ADMIN được phép thực hiện.
        /// </summary>
        /// <param name="salaryPeriodId">ID kỳ lương cần khóa.</param>
        /// <returns>Số lượng bảng lương đã khóa.</returns>
        [RequireRole("ADMIN")]
        [HttpPost("periods/{salaryPeriodId:guid}/lock")]
        public async Task<IActionResult> Lock(Guid salaryPeriodId)
        {
            var totalLocked = await _payrollService.LockPayrollsAsync(salaryPeriodId);
            return Ok(new { totalLocked, message = "Khóa payroll thành công" });
        }

        /// <summary>
        /// Đánh dấu đã chi trả bảng lương theo kỳ.
        /// </summary>
        /// <param name="salaryPeriodId">ID kỳ lương cần chi trả.</param>
        /// <returns>Số lượng bảng lương đã chi trả.</returns>
        [RequireRole("ADMIN", "HR")]
        [HttpPost("periods/{salaryPeriodId:guid}/pay")]
        public async Task<IActionResult> Pay(Guid salaryPeriodId)
        {
            var totalPaid = await _payrollService.MarkPayrollsPaidAsync(salaryPeriodId);
            return Ok(new { totalPaid, message = "Chi trả payroll thành công" });
        }
    }
}