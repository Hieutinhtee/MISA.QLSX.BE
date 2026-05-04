using MISA.QLSX.Core.Entities;

namespace MISA.QLSX.Core.Interfaces.Service
{
    /// <summary>
    /// Service interface cho yêu cầu phê duyệt.
    /// </summary>
    public interface IApprovalRequestService : IBaseService<ApprovalRequest>
    {
        /// <summary>
        /// Tạo yêu cầu phê duyệt mới (tự tạo steps theo request_type).
        /// </summary>
        Task<Guid> CreateRequestAsync(ApprovalRequest request);

        /// <summary>
        /// Phê duyệt 1 bước.
        /// </summary>
        Task<bool> ApproveStepAsync(Guid requestId, Guid stepId, string? comment, Guid actedBy);

        /// <summary>
        /// Từ chối 1 bước.
        /// </summary>
        Task<bool> RejectStepAsync(Guid requestId, Guid stepId, string? comment, Guid actedBy);

        /// <summary>
        /// Lấy danh sách bước phê duyệt theo request ID.
        /// </summary>
        Task<List<ApprovalStep>> GetStepsAsync(Guid requestId);
    }
}
