using MISA.QLSX.Core.Entities;

namespace MISA.QLSX.Core.Interfaces.Repository
{
    /// <summary>
    /// Repository interface cho yêu cầu phê duyệt.
    /// </summary>
    public interface IApprovalRequestRepository : IBaseRepository<ApprovalRequest>
    {
        /// <summary>
        /// Lấy danh sách bước phê duyệt theo request ID.
        /// </summary>
        Task<List<ApprovalStep>> GetStepsByRequestIdAsync(Guid requestId);

        /// <summary>
        /// Thêm bước phê duyệt.
        /// </summary>
        Task<Guid> InsertStepAsync(ApprovalStep step);

        /// <summary>
        /// Cập nhật bước phê duyệt.
        /// </summary>
        Task<int> UpdateStepAsync(ApprovalStep step);

        /// <summary>
        /// Lấy bước phê duyệt theo ID.
        /// </summary>
        Task<ApprovalStep?> GetStepByIdAsync(Guid stepId);

        /// <summary>
        /// Cập nhật department_id cho employee (thuyên chuyển).
        /// </summary>
        Task<int> UpdateEmployeeDepartmentAsync(Guid employeeId, Guid departmentId);

        /// <summary>
        /// Cập nhật manager_employee_id cho department.
        /// </summary>
        Task<int> UpdateDepartmentManagerAsync(Guid departmentId, Guid managerEmployeeId);

        /// <summary>
        /// Thêm bản ghi lịch sử thành viên phòng ban.
        /// </summary>
        Task<Guid> InsertMemberHistoryAsync(Guid employeeId, Guid departmentId, string action, DateTime effectiveDate, Guid? approvalRequestId, Guid? createdBy);

        /// <summary>
        /// Thêm bản ghi lịch sử trưởng phòng.
        /// </summary>
        Task<Guid> InsertManagerHistoryAsync(Guid departmentId, Guid managerEmployeeId, DateTime effectiveDate, Guid? approvalRequestId, Guid? createdBy);

        /// <summary>
        /// Sinh mã yêu cầu tự động.
        /// </summary>
        Task<string> GenerateRequestCodeAsync();
    }
}
