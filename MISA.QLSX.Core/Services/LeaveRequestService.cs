using System.Text.Json;
using MISA.QLSX.Core.Entities;
using MISA.QLSX.Core.Exceptions;
using MISA.QLSX.Core.Interfaces.Repository;
using MISA.QLSX.Core.Interfaces.Service;

namespace MISA.QLSX.Core.Services
{
    /// <summary>
    /// Nghiệp vụ đơn xin nghỉ phép.
    /// </summary>
    public class LeaveRequestService : BaseServices<LeaveRequest>, ILeaveRequestService
    {
        private readonly ILeaveRequestRepository _leaveRequestRepository;
        private readonly IApprovalRequestService _approvalRequestService;

        /// <summary>
        /// Khởi tạo service đơn nghỉ phép.
        /// </summary>
        /// <param name="leaveRequestRepository">Repository đơn nghỉ phép.</param>
        /// <param name="approvalRequestService">Service phê duyệt dùng để tạo luồng duyệt.</param>
        public LeaveRequestService(
            ILeaveRequestRepository leaveRequestRepository,
            IApprovalRequestService approvalRequestService
        )
            : base(leaveRequestRepository)
        {
            _leaveRequestRepository = leaveRequestRepository;
            _approvalRequestService = approvalRequestService;
        }

        /// <summary>
        /// Tạo mới đơn nghỉ phép và sinh yêu cầu phê duyệt tương ứng.
        /// </summary>
        /// <param name="entity">Dữ liệu đơn nghỉ phép.</param>
        /// <returns>ID đơn nghỉ phép vừa tạo.</returns>
        public override async Task<Guid> CreateAsync(LeaveRequest entity)
        {
            await ValidateLeaveRequestAsync(entity);

            entity.LeaveRequestId ??= Guid.NewGuid();
            entity.LeaveRequestCode = await _leaveRequestRepository.GenerateLeaveRequestCodeAsync();
            entity.ApprovalStatus = 0;

            await BeforeSaveAsync(entity, false);

            var leaveRequestId = entity.LeaveRequestId.Value;
            await _leaveRequestRepository.InsertAsync(entity);

            Guid? approvalRequestId = null;

            try
            {
                var approvalRequest = new ApprovalRequest
                {
                    RequestType = "leave_request",
                    Title = $"Đơn nghỉ phép {entity.LeaveRequestCode}",
                    Description = entity.Reason,
                    Payload = JsonSerializer.Serialize(
                        new
                        {
                            leaveRequestId,
                            entity.EmployeeId,
                            entity.StartDate,
                            entity.ReturnDate,
                            entity.Reason,
                        }
                    ),
                    EffectiveDate = entity.StartDate,
                    CreatedBy = entity.CreatedBy,
                };

                approvalRequestId = await _approvalRequestService.CreateRequestAsync(approvalRequest);
                entity.ApprovalRequestId = approvalRequestId;
                entity.UpdatedBy = entity.CreatedBy;
                await _leaveRequestRepository.UpdateAsync(leaveRequestId, entity);

                return leaveRequestId;
            }
            catch
            {
                if (approvalRequestId.HasValue)
                {
                    try
                    {
                        await _approvalRequestService.CancelRequestAsync(
                            approvalRequestId.Value,
                            entity.CreatedBy.GetValueOrDefault()
                        );
                    }
                    catch
                    {
                        // Bỏ qua lỗi bù trừ để vẫn đảm bảo rollback chính cho đơn nghỉ phép.
                    }
                }

                await _leaveRequestRepository.DeleteAsync(leaveRequestId);
                throw;
            }
        }

        /// <summary>
        /// Thu hồi đơn nghỉ phép khi yêu cầu còn ở trạng thái chờ duyệt.
        /// </summary>
        /// <param name="leaveRequestId">ID đơn nghỉ phép.</param>
        /// <param name="employeeId">ID nhân viên đang thu hồi.</param>
        /// <returns>ID đơn nghỉ phép đã thu hồi.</returns>
        public async Task<Guid> WithdrawAsync(Guid leaveRequestId, Guid employeeId)
        {
            var leaveRequest = await EnsureExistsAsync(leaveRequestId);
            if (leaveRequest == null)
            {
                throw new NotFoundException("Không tìm thấy đơn nghỉ phép", "Đơn nghỉ phép không tồn tại");
            }

            if (!leaveRequest.EmployeeId.HasValue || leaveRequest.EmployeeId.Value != employeeId)
            {
                throw new ForbiddenException(
                    "Chỉ nhân viên tạo đơn mới được thu hồi",
                    "Bạn không có quyền thu hồi đơn nghỉ phép này"
                );
            }

            if (leaveRequest.ApprovalStatus != 0)
            {
                throw new ValidateException(
                    "Đơn nghỉ phép đã được xử lý",
                    "Chỉ có thể thu hồi đơn đang chờ duyệt"
                );
            }

            if (!leaveRequest.ApprovalRequestId.HasValue)
            {
                throw new ValidateException(
                    "Thiếu liên kết phê duyệt",
                    "Đơn nghỉ phép chưa được gắn với yêu cầu phê duyệt"
                );
            }

            var approvalRequest = await _approvalRequestService.GetByIdAsync(
                leaveRequest.ApprovalRequestId.Value
            );

            if (approvalRequest == null)
            {
                throw new NotFoundException(
                    "Không tìm thấy yêu cầu phê duyệt",
                    "Yêu cầu phê duyệt của đơn nghỉ phép không tồn tại"
                );
            }

            if (!string.Equals(approvalRequest.Status, "pending", StringComparison.OrdinalIgnoreCase))
            {
                throw new ValidateException(
                    "Yêu cầu phê duyệt đã được xử lý",
                    "Chỉ có thể thu hồi khi yêu cầu còn ở trạng thái chờ duyệt"
                );
            }

            var linkedApprovalRequestId = approvalRequest.ApprovalRequestId.GetValueOrDefault();
            await _approvalRequestService.CancelRequestAsync(linkedApprovalRequestId, employeeId);

            leaveRequest.ApprovalStatus = 3;
            leaveRequest.UpdatedBy = employeeId;
            await _leaveRequestRepository.UpdateAsync(leaveRequestId, leaveRequest);

            return leaveRequestId;
        }

        /// <summary>
        /// Kiểm tra dữ liệu đầu vào của đơn nghỉ phép.
        /// </summary>
        /// <param name="entity">Đơn nghỉ phép cần kiểm tra.</param>
        /// <returns>Task hoàn thành sau khi kiểm tra.</returns>
        private async Task ValidateLeaveRequestAsync(LeaveRequest entity)
        {
            if (!entity.EmployeeId.HasValue || entity.EmployeeId.Value == Guid.Empty)
            {
                throw new ValidateException("Thiếu nhân viên", "Nhân viên không được để trống");
            }

            if (string.IsNullOrWhiteSpace(entity.Reason))
            {
                throw new ValidateException("Thiếu lý do", "Lý do xin nghỉ không được để trống");
            }

            if (!entity.StartDate.HasValue || !entity.ReturnDate.HasValue)
            {
                throw new ValidateException("Thiếu ngày nghỉ", "Ngày bắt đầu và ngày đi làm lại không được để trống");
            }

            if (entity.ReturnDate.Value.Date < entity.StartDate.Value.Date)
            {
                throw new ValidateException(
                    "Khoảng ngày không hợp lệ",
                    "Ngày đi làm lại phải lớn hơn hoặc bằng ngày bắt đầu nghỉ"
                );
            }

            var hasOverlap = await _leaveRequestRepository.HasOverlappingRequestAsync(
                entity.EmployeeId.Value,
                entity.StartDate.Value,
                entity.ReturnDate.Value,
                entity.LeaveRequestId
            );

            if (hasOverlap)
            {
                throw new ValidateException(
                    "Đơn nghỉ phép bị trùng khoảng ngày",
                    "Nhân viên đã có đơn nghỉ phép đang chờ duyệt hoặc đã được duyệt trong khoảng ngày này"
                );
            }
        }
    }
}