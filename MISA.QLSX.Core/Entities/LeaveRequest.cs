using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MISA.QLSX.Core.Entities
{
    /// <summary>
    /// Đơn xin nghỉ phép của nhân viên.
    /// </summary>
    [Table("leave_request")]
    public class LeaveRequest
    {
        /// <summary>
        /// Khóa chính của đơn nghỉ phép.
        /// </summary>
        [Key]
        [Column("leave_request_id")]
        public Guid? LeaveRequestId { get; set; }

        /// <summary>
        /// Mã đơn nghỉ phép.
        /// </summary>
        [Column("leave_request_code")]
        public string? LeaveRequestCode { get; set; }

        /// <summary>
        /// ID nhân viên tạo đơn.
        /// </summary>
        [Column("employee_id")]
        public Guid? EmployeeId { get; set; }

        /// <summary>
        /// Ngày bắt đầu nghỉ phép.
        /// </summary>
        [Column("start_date")]
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Ngày dự kiến đi làm lại.
        /// </summary>
        [Column("return_date")]
        public DateTime? ReturnDate { get; set; }

        /// <summary>
        /// Lý do xin nghỉ phép.
        /// </summary>
        [Column("reason")]
        public string? Reason { get; set; }

        /// <summary>
        /// Trạng thái duyệt: 0 chờ duyệt, 1 đã duyệt, 2 từ chối, 3 đã hủy.
        /// </summary>
        [Column("approval_status")]
        public int ApprovalStatus { get; set; }

        /// <summary>
        /// ID yêu cầu phê duyệt liên kết.
        /// </summary>
        [Column("approval_request_id")]
        public Guid? ApprovalRequestId { get; set; }

        /// <summary>
        /// Thời điểm tạo đơn.
        /// </summary>
        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        /// <summary>
        /// Người tạo đơn.
        /// </summary>
        [Column("created_by")]
        public Guid? CreatedBy { get; set; }

        /// <summary>
        /// Người cập nhật gần nhất.
        /// </summary>
        [Column("updated_by")]
        public Guid? UpdatedBy { get; set; }

        /// <summary>
        /// Thời điểm cập nhật gần nhất.
        /// </summary>
        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Mã nhân viên hiển thị trong danh sách.
        /// </summary>
        [NotMapped]
        public string? EmployeeCode { get; set; }

        /// <summary>
        /// Tên nhân viên hiển thị trong danh sách.
        /// </summary>
        [NotMapped]
        public string? EmployeeName { get; set; }
    }
}