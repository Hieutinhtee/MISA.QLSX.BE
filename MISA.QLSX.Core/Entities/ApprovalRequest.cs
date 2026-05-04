using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MISA.QLSX.Core.Entities
{
    /// <summary>
    /// Đối tượng yêu cầu phê duyệt.
    /// </summary>
    [Table("approval_request")]
    public class ApprovalRequest
    {
        [Key]
        [Column("approval_request_id")]
        public Guid? ApprovalRequestId { get; set; }

        [Column("request_code")]
        public string? RequestCode { get; set; }

        /// <summary>
        /// Loại yêu cầu: department_member_transfer, department_manager_change, contract_change, leave_request
        /// </summary>
        [Column("request_type")]
        public string? RequestType { get; set; }

        [Column("title")]
        public string? Title { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        /// <summary>
        /// JSON chứa dữ liệu thay đổi cần áp dụng khi được duyệt.
        /// </summary>
        [Column("payload")]
        public string? Payload { get; set; }

        /// <summary>
        /// Ngày có hiệu lực của thay đổi.
        /// </summary>
        [Column("effective_date")]
        public DateTime? EffectiveDate { get; set; }

        /// <summary>
        /// Trạng thái: pending, approved, rejected, cancelled
        /// </summary>
        [Column("status")]
        public string? Status { get; set; }

        [Column("current_step")]
        public int CurrentStep { get; set; } = 1;

        [Column("total_steps")]
        public int TotalSteps { get; set; } = 1;

        [Column("is_deleted")]
        public Guid? IsDeleted { get; set; } = Guid.Empty;

        [Column("created_by")]
        public Guid? CreatedBy { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        [Column("updated_by")]
        public Guid? UpdatedBy { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        // === NotMapped: dùng cho hiển thị FE ===

        /// <summary>
        /// Tên người tạo yêu cầu (JOIN từ employee).
        /// </summary>
        [NotMapped]
        public string? CreatedByName { get; set; }

        /// <summary>
        /// Danh sách các bước phê duyệt.
        /// </summary>
        [NotMapped]
        public List<ApprovalStep>? Steps { get; set; }
    }
}
