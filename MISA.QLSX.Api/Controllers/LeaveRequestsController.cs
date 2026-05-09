using Microsoft.AspNetCore.Mvc;
using MISA.QLSX.Api.Authorization;
using MISA.QLSX.Core.DTOs.Requests;
using MISA.QLSX.Core.DTOs.Responses;
using MISA.QLSX.Core.Entities;
using MISA.QLSX.Core.Exceptions;
using MISA.QLSX.Core.Interfaces.Service;

namespace MISA.QLSX.Api.Controllers
{
    /// <summary>
    /// Controller quản lý đơn xin nghỉ phép.
    /// </summary>
    [RequireRole("ADMIN", "HR", "MANAGER", "EMPLOYEE")]
    public class LeaveRequestsController : BaseController<LeaveRequest>
    {
        private readonly ILeaveRequestService _leaveRequestService;
        private readonly IApprovalRequestService _approvalRequestService;

        /// <summary>
        /// Khởi tạo controller đơn nghỉ phép.
        /// </summary>
        /// <param name="leaveRequestService">Service đơn nghỉ phép.</param>
        /// <param name="approvalRequestService">Service phê duyệt.</param>
        public LeaveRequestsController(
            ILeaveRequestService leaveRequestService,
            IApprovalRequestService approvalRequestService
        )
            : base(leaveRequestService)
        {
            _leaveRequestService = leaveRequestService;
            _approvalRequestService = approvalRequestService;
        }

        /// <summary>
        /// Tạo đơn nghỉ phép mới.
        /// </summary>
        /// <param name="entity">Dữ liệu đơn nghỉ phép.</param>
        /// <returns>ID đơn nghỉ phép vừa tạo.</returns>
        [HttpPost]
        public override async Task<IActionResult> Create([FromBody] LeaveRequest entity)
        {
            entity ??= new LeaveRequest();

            var employeeId = GetCurrentEmployeeId();
            if (employeeId == Guid.Empty)
            {
                throw new ForbiddenException(
                    "Không xác định được nhân viên hiện tại",
                    "Phiên đăng nhập không hợp lệ"
                );
            }

            entity.EmployeeId = employeeId;
            entity.CreatedBy = employeeId;
            entity.UpdatedBy = employeeId;

            var id = await _leaveRequestService.CreateAsync(entity);
            return StatusCode(201, new { success = true, data = id });
        }

        /// <summary>
        /// Lấy dữ liệu phân trang, EMPLOYEE chỉ xem đơn của mình.
        /// </summary>
        /// <param name="request">Yêu cầu phân trang.</param>
        /// <returns>Dữ liệu phân trang theo quyền truy cập.</returns>
        [HttpPost("paging")]
        public override async Task<PagingResponse<LeaveRequest>> GetPaging([FromBody] QueryRequest request)
        {
            request ??= new QueryRequest();
            request.Filters ??= new List<FilterCondition>();

            var role = GetCurrentRole();
            var employeeId = GetCurrentEmployeeId();

            if (role == "EMPLOYEE")
            {
                if (employeeId == Guid.Empty)
                {
                    throw new ForbiddenException(
                        "Không xác định được nhân viên hiện tại",
                        "Phiên đăng nhập không hợp lệ"
                    );
                }

                request.Filters.RemoveAll(f => string.Equals(f.Field, "employeeId", StringComparison.OrdinalIgnoreCase));
                request.Filters.Add(new FilterCondition { Field = "employeeId", Operator = "eq", Value = employeeId });
            }

            return await _leaveRequestService.QueryPagingAsync(request);
        }

        /// <summary>
        /// Lấy chi tiết đơn nghỉ phép.
        /// </summary>
        /// <param name="id">ID đơn nghỉ phép.</param>
        /// <returns>Chi tiết đơn nghỉ phép và dữ liệu phê duyệt.</returns>
        public override async Task<IActionResult> GetById(Guid id)
        {
            var leaveRequest = await _leaveRequestService.GetByIdAsync(id);
            var role = GetCurrentRole();
            var employeeId = GetCurrentEmployeeId();

            if (role == "EMPLOYEE")
            {
                if (!leaveRequest.EmployeeId.HasValue || leaveRequest.EmployeeId.Value != employeeId)
                {
                    throw new ForbiddenException(
                        "EMPLOYEE chỉ được xem đơn nghỉ phép của chính mình",
                        "Bạn không có quyền xem đơn nghỉ phép của nhân viên khác"
                    );
                }
            }

            object? approvalRequest = null;
            List<ApprovalStep>? steps = null;

            if (leaveRequest.ApprovalRequestId.HasValue)
            {
                approvalRequest = await _approvalRequestService.GetByIdAsync(
                    leaveRequest.ApprovalRequestId.Value
                );
                steps = await _approvalRequestService.GetStepsAsync(leaveRequest.ApprovalRequestId.Value);
            }

            return Ok(new
            {
                data = new
                {
                    leaveRequest,
                    approvalRequest,
                    steps,
                },
            });
        }

        /// <summary>
        /// Thu hồi đơn nghỉ phép.
        /// </summary>
        /// <param name="id">ID đơn nghỉ phép.</param>
        /// <returns>Kết quả thu hồi.</returns>
        [HttpPost("{id}/withdraw")]
        public async Task<IActionResult> Withdraw(Guid id)
        {
            var employeeId = GetCurrentEmployeeId();
            if (employeeId == Guid.Empty)
            {
                throw new ForbiddenException(
                    "Không xác định được nhân viên hiện tại",
                    "Phiên đăng nhập không hợp lệ"
                );
            }

            var result = await _leaveRequestService.WithdrawAsync(id, employeeId);
            return Ok(new { success = true, data = result });
        }

        /// <summary>
        /// Lấy role hiện tại từ session.
        /// </summary>
        /// <returns>Mã role hiện tại.</returns>
        private string GetCurrentRole()
        {
            return HttpContext.Session.GetString("role_code")?.ToUpperInvariant() ?? string.Empty;
        }

        /// <summary>
        /// Lấy ID nhân viên hiện tại từ session.
        /// </summary>
        /// <returns>Employee ID hoặc Guid.Empty nếu không có.</returns>
        private Guid GetCurrentEmployeeId()
        {
            var employeeId = HttpContext.Session.GetString("employee_id");
            return Guid.TryParse(employeeId, out var parsedId) ? parsedId : Guid.Empty;
        }
    }
}