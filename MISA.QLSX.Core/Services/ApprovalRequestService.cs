using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using MISA.QLSX.Core.Entities;
using MISA.QLSX.Core.Exceptions;
using MISA.QLSX.Core.Interfaces.Repository;
using MISA.QLSX.Core.Interfaces.Service;

namespace MISA.QLSX.Core.Services
{
    /// <summary>
    /// Dịch vụ nghiệp vụ phê duyệt — chứa core logic tạo request, approve/reject, và thực thi thay đổi.
    /// </summary>
    public class ApprovalRequestService : BaseServices<ApprovalRequest>, IApprovalRequestService
    {
        private readonly IApprovalRequestRepository _approvalRepo;
        private readonly IEmployeeRepository _employeeRepo;

        public ApprovalRequestService(
            IApprovalRequestRepository repo,
            IEmployeeRepository employeeRepo) : base(repo)
        {
            _approvalRepo = repo;
            _employeeRepo = employeeRepo;
        }

        /// <inheritdoc />
        public async Task<Guid> CreateRequestAsync(ApprovalRequest request)
        {
            // Validate bắt buộc
            if (string.IsNullOrWhiteSpace(request.RequestType))
                throw new ValidateException("Loại yêu cầu không được để trống", "Thiếu loại yêu cầu");

            if (string.IsNullOrWhiteSpace(request.Title))
                throw new ValidateException("Tiêu đề không được để trống", "Thiếu tiêu đề");

            if (string.IsNullOrWhiteSpace(request.Payload))
                throw new ValidateException("Dữ liệu thay đổi không được để trống", "Thiếu payload");

            // Tự sinh mã
            request.RequestCode = await _approvalRepo.GenerateRequestCodeAsync();
            request.Status = "pending";
            request.CurrentStep = 1;

            // Xác định số bước duyệt theo loại yêu cầu
            var steps = BuildSteps(request.RequestType);
            request.TotalSteps = steps.Count;

            if (request.RequestType == "leave_request" && request.CreatedBy.HasValue)
            {
                var managerId = await _employeeRepo.GetDepartmentManagerIdAsync(request.CreatedBy.Value);
                var managerStep = steps.FirstOrDefault(s => s.ApproverRole == "MANAGER");

                if (managerStep != null)
                {
                    if (!managerId.HasValue || managerId.Value == request.CreatedBy.Value)
                    {
                        // Người tạo là trưởng phòng, hoặc phòng chưa có trưởng phòng -> Chuyển cho HR duyệt
                        managerStep.ApproverRole = "HR";
                    }
                    else
                    {
                        // Gán đích danh ID trưởng phòng
                        managerStep.ApproverId = managerId.Value;
                    }
                }
            }

            // Tạo request
            var requestId = await base.CreateAsync(request);

            // Tạo các bước phê duyệt
            foreach (var step in steps)
            {
                step.ApprovalRequestId = requestId;
                await _approvalRepo.InsertStepAsync(step);
            }

            return requestId;
        }

        /// <inheritdoc />
        public async Task<bool> ApproveStepAsync(Guid requestId, Guid stepId, string? comment, Guid actedBy)
        {
            var request = await EnsureExistsAsync(requestId);
            if (request == null)
                throw new NotFoundException("Không tìm thấy yêu cầu", "Yêu cầu không tồn tại");

            if (request.Status != "pending")
                throw new ValidateException("Yêu cầu đã được xử lý", "Không thể phê duyệt yêu cầu đã hoàn tất");

            var step = await _approvalRepo.GetStepByIdAsync(stepId);
            if (step == null)
                throw new NotFoundException("Không tìm thấy bước phê duyệt", "Bước duyệt không tồn tại");

            if (step.ApprovalRequestId != requestId)
                throw new ValidateException("Bước không thuộc yêu cầu này", "Bước duyệt không hợp lệ");

            if (step.Status != "pending")
                throw new ValidateException("Bước đã được xử lý", "Không thể duyệt bước đã hoàn tất");

            if (step.StepOrder != request.CurrentStep)
                throw new ValidateException("Chưa đến lượt duyệt bước này", "Phải duyệt theo thứ tự");

            // Kiểm tra quyền đích danh nếu có
            if (step.ApproverId.HasValue && step.ApproverId.Value != actedBy)
                throw new ValidateException("Bạn không có quyền duyệt bước này", "Chỉ người được chỉ định mới có thể duyệt");

            // Cập nhật step
            step.Status = "approved";
            step.Comment = comment;
            step.ActedAt = DateTime.Now;
            step.ActedBy = actedBy;
            await _approvalRepo.UpdateStepAsync(step);

            // Kiểm tra nếu là step cuối
            if (request.CurrentStep >= request.TotalSteps)
            {
                // Duyệt hoàn tất → thực thi thay đổi
                request.Status = "approved";
                request.UpdatedBy = actedBy;
                await _approvalRepo.UpdateAsync(requestId, request);

                await ExecuteApprovedChangeAsync(request);
            }
            else
            {
                // Chuyển sang step tiếp theo
                request.CurrentStep += 1;
                request.UpdatedBy = actedBy;
                await _approvalRepo.UpdateAsync(requestId, request);
            }

            return true;
        }

        /// <inheritdoc />
        public async Task<bool> RejectStepAsync(Guid requestId, Guid stepId, string? comment, Guid actedBy)
        {
            var request = await EnsureExistsAsync(requestId);
            if (request == null)
                throw new NotFoundException("Không tìm thấy yêu cầu", "Yêu cầu không tồn tại");

            if (request.Status != "pending")
                throw new ValidateException("Yêu cầu đã được xử lý", "Không thể từ chối yêu cầu đã hoàn tất");

            var step = await _approvalRepo.GetStepByIdAsync(stepId);
            if (step == null)
                throw new NotFoundException("Không tìm thấy bước phê duyệt", "Bước duyệt không tồn tại");

            if (step.Status != "pending")
                throw new ValidateException("Bước đã được xử lý", "Không thể từ chối bước đã hoàn tất");

            // Kiểm tra quyền đích danh nếu có
            if (step.ApproverId.HasValue && step.ApproverId.Value != actedBy)
                throw new ValidateException("Bạn không có quyền từ chối bước này", "Chỉ người được chỉ định mới có thể từ chối");

            // Cập nhật step
            step.Status = "rejected";
            step.Comment = comment;
            step.ActedAt = DateTime.Now;
            step.ActedBy = actedBy;
            await _approvalRepo.UpdateStepAsync(step);

            // Reject toàn bộ request
            request.Status = "rejected";
            request.UpdatedBy = actedBy;
            await _approvalRepo.UpdateAsync(requestId, request);

            return true;
        }

        /// <inheritdoc />
        public async Task<List<ApprovalStep>> GetStepsAsync(Guid requestId)
        {
            return await _approvalRepo.GetStepsByRequestIdAsync(requestId);
        }

        /// <inheritdoc />
        public async Task<bool> CancelRequestAsync(Guid requestId, Guid actedBy)
        {
            var request = await EnsureExistsAsync(requestId);
            if (request == null)
                throw new NotFoundException("Không tìm thấy yêu cầu", "Yêu cầu không tồn tại");

            if (!string.Equals(request.Status, "pending", StringComparison.OrdinalIgnoreCase))
            {
                throw new ValidateException(
                    "Yêu cầu đã được xử lý",
                    "Chỉ có thể hủy yêu cầu đang chờ duyệt"
                );
            }

            request.Status = "cancelled";
            request.UpdatedBy = actedBy;
            await _approvalRepo.UpdateAsync(requestId, request);
            return true;
        }

        /// <summary>
        /// Tạo danh sách bước phê duyệt theo loại yêu cầu.
        /// </summary>
        private List<ApprovalStep> BuildSteps(string requestType)
        {
            return requestType switch
            {
                // HR tạo → ADMIN duyệt
                "department_member_transfer" or "department_manager_change" or "contract_change" =>
                    new List<ApprovalStep>
                    {
                        new() { StepOrder = 1, ApproverRole = "ADMIN", Status = "pending" }
                    },

                // Employee tạo → đích danh MANAGER duyệt (đã được gán ApproverId ở trên)
                "leave_request" =>
                    new List<ApprovalStep>
                    {
                        new() { StepOrder = 1, ApproverRole = "MANAGER", Status = "pending" }
                    },

                _ => throw new ValidateException($"Loại yêu cầu '{requestType}' không hợp lệ", "Loại yêu cầu không được hỗ trợ")
            };
        }

        /// <summary>
        /// Thực thi thay đổi khi yêu cầu được duyệt hoàn tất.
        /// Parse JSON payload và thực hiện update tương ứng.
        /// </summary>
        private async Task ExecuteApprovedChangeAsync(ApprovalRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Payload)) return;

            var payload = JsonSerializer.Deserialize<JsonElement>(request.Payload);
            var effectiveDate = request.EffectiveDate ?? DateTime.Today;

            switch (request.RequestType)
            {
                case "department_member_transfer":
                    await ExecuteDepartmentMemberTransfer(payload, effectiveDate, request);
                    break;

                case "department_manager_change":
                    await ExecuteDepartmentManagerChange(payload, effectiveDate, request);
                    break;

                case "contract_change":
                    // Placeholder — sẽ implement chi tiết khi cần
                    break;

                case "leave_request":
                    // Đơn nghỉ phép chỉ cần ghi nhận trạng thái approved, không update data
                    break;
            }
        }

        /// <summary>
        /// Thuyên chuyển thành viên phòng ban.
        /// </summary>
        private async Task ExecuteDepartmentMemberTransfer(JsonElement payload, DateTime effectiveDate, ApprovalRequest request)
        {
            var employeeId = Guid.Parse(payload.GetProperty("employeeId").GetString()!);
            var toDepartmentId = Guid.Parse(payload.GetProperty("toDepartmentId").GetString()!);

            // Ghi lịch sử rời phòng cũ (nếu có)
            if (payload.TryGetProperty("fromDepartmentId", out var fromProp) && !string.IsNullOrEmpty(fromProp.GetString()))
            {
                var fromDepartmentId = Guid.Parse(fromProp.GetString()!);
                await _approvalRepo.InsertMemberHistoryAsync(employeeId, fromDepartmentId, "leave", effectiveDate, request.ApprovalRequestId, request.CreatedBy);
            }

            // Ghi lịch sử gia nhập phòng mới
            await _approvalRepo.InsertMemberHistoryAsync(employeeId, toDepartmentId, "join", effectiveDate, request.ApprovalRequestId, request.CreatedBy);

            // Update employee.department_id
            await _approvalRepo.UpdateEmployeeDepartmentAsync(employeeId, toDepartmentId);
        }

        /// <summary>
        /// Đổi trưởng phòng.
        /// </summary>
        private async Task ExecuteDepartmentManagerChange(JsonElement payload, DateTime effectiveDate, ApprovalRequest request)
        {
            var departmentId = Guid.Parse(payload.GetProperty("departmentId").GetString()!);
            var newManagerId = Guid.Parse(payload.GetProperty("newManagerId").GetString()!);

            // Ghi lịch sử bổ nhiệm
            await _approvalRepo.InsertManagerHistoryAsync(departmentId, newManagerId, effectiveDate, request.ApprovalRequestId, request.CreatedBy);

            // Update department.manager_employee_id
            await _approvalRepo.UpdateDepartmentManagerAsync(departmentId, newManagerId);
        }
    }
}
