using Microsoft.AspNetCore.Mvc;
using MISA.QLSX.Api.Authorization;
using MISA.QLSX.Core.Entities;
using MISA.QLSX.Core.Interfaces.Service;

namespace MISA.QLSX.Api.Controllers
{
    /// <summary>
    /// Controller quản lý API phê duyệt.
    /// Kế thừa BaseController cho CRUD cơ bản + custom endpoints cho approve/reject.
    /// </summary>
    [RequireRole("ADMIN", "HR", "MANAGER", "EMPLOYEE")]
    public class ApprovalRequestsController : BaseController<ApprovalRequest>
    {
        private readonly IApprovalRequestService _approvalService;

        public ApprovalRequestsController(IApprovalRequestService approvalService)
            : base(approvalService)
        {
            _approvalService = approvalService;
        }

        /// <summary>
        /// Tạo yêu cầu phê duyệt mới (tự tạo steps theo request_type).
        /// Override Create mặc định để dùng CreateRequestAsync.
        /// </summary>
        [HttpPost]
        public override async Task<IActionResult> Create([FromBody] ApprovalRequest entity)
        {
            var id = await _approvalService.CreateRequestAsync(entity);
            return StatusCode(201, new { success = true, data = id });
        }

        /// <summary>
        /// Phê duyệt 1 bước.
        /// POST /api/ApprovalRequests/{id}/approve/{stepId}
        /// </summary>
        [HttpPost("{id}/approve/{stepId}")]
        public async Task<IActionResult> ApproveStep(Guid id, Guid stepId, [FromBody] ApprovalActionRequest body)
        {
            var actedBy = GetCurrentUserId();
            var result = await _approvalService.ApproveStepAsync(id, stepId, body?.Comment, actedBy);
            return Ok(new { success = true, data = result });
        }

        /// <summary>
        /// Từ chối 1 bước.
        /// POST /api/ApprovalRequests/{id}/reject/{stepId}
        /// </summary>
        [HttpPost("{id}/reject/{stepId}")]
        public async Task<IActionResult> RejectStep(Guid id, Guid stepId, [FromBody] ApprovalActionRequest body)
        {
            var actedBy = GetCurrentUserId();
            var result = await _approvalService.RejectStepAsync(id, stepId, body?.Comment, actedBy);
            return Ok(new { success = true, data = result });
        }

        /// <summary>
        /// Lấy danh sách bước phê duyệt của 1 yêu cầu.
        /// GET /api/ApprovalRequests/{id}/steps
        /// </summary>
        [HttpGet("{id}/steps")]
        public async Task<IActionResult> GetSteps(Guid id)
        {
            var steps = await _approvalService.GetStepsAsync(id);
            return Ok(new { success = true, data = steps });
        }

        /// <summary>
        /// Lấy user id từ session hiện tại.
        /// </summary>
        private Guid GetCurrentUserId()
        {
            var userIdStr = HttpContext.Session.GetString("account_id");
            if (Guid.TryParse(userIdStr, out var userId))
                return userId;
            return Guid.Empty;
        }
    }

    /// <summary>
    /// DTO cho action approve/reject.
    /// </summary>
    public class ApprovalActionRequest
    {
        public string? Comment { get; set; }
    }
}
