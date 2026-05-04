using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MISA.QLSX.Core.Entities
{
    /// <summary>
    /// Đối tượng bước phê duyệt trong quy trình.
    /// </summary>
    [Table("approval_step")]
    public class ApprovalStep
    {
        [Key]
        [Column("approval_step_id")]
        public Guid? ApprovalStepId { get; set; }

        [Column("approval_request_id")]
        public Guid? ApprovalRequestId { get; set; }

        /// <summary>
        /// Thứ tự bước phê duyệt (1, 2, ...).
        /// </summary>
        [Column("step_order")]
        public int StepOrder { get; set; }

        /// <summary>
        /// Role cần phê duyệt bước này: ADMIN, HR, MANAGER.
        /// </summary>
        [Column("approver_role")]
        public string? ApproverRole { get; set; }

        /// <summary>
        /// Người duyệt cụ thể (null = bất kỳ ai có role phù hợp).
        /// </summary>
        [Column("approver_id")]
        public Guid? ApproverId { get; set; }

        /// <summary>
        /// Trạng thái bước: pending, approved, rejected.
        /// </summary>
        [Column("status")]
        public string? Status { get; set; }

        [Column("comment")]
        public string? Comment { get; set; }

        [Column("acted_at")]
        public DateTime? ActedAt { get; set; }

        [Column("acted_by")]
        public Guid? ActedBy { get; set; }

        // === NotMapped ===

        [NotMapped]
        public string? ActedByName { get; set; }
    }
}
