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

        /// <summary>
        /// Hủy yêu cầu phê duyệt nếu vẫn còn ở trạng thái chờ duyệt.
        /// </summary>
        /// <param name="requestId">ID yêu cầu phê duyệt.</param>
        /// <param name="actedBy">ID người thao tác.</param>
        /// <returns>True nếu hủy thành công.</returns>
        Task<bool> CancelRequestAsync(Guid requestId, Guid actedBy);
    }
}
