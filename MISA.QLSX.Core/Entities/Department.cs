using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MISA.QLSX.Core.Entities
{
    /// <summary>
    /// Đối tượng danh mục phòng ban.
    /// </summary>
    [Table("department")]
    public class Department
    {
        /// <summary>
        /// ID phòng ban (UUID).
        /// </summary>
        [Key]
        [Column("department_id")]
        public Guid? DepartmentId { get; set; }

        /// <summary>
        /// Mã phòng ban.
        /// </summary>
        [Column("department_code")]
        public string? DepartmentCode { get; set; }

        /// <summary>
        /// Tên phòng ban.
        /// </summary>
        [Column("department_name")]
        public string? DepartmentName { get; set; }

        /// <summary>
        /// Mô tả phòng ban.
        /// </summary>
        [Column("description")]
        public string? Description { get; set; }

        /// <summary>
        /// UUID nhân viên trưởng phòng.
        /// </summary>
        [Column("manager_employee_id")]
        public Guid? ManagerEmployeeId { get; set; }

        /// <summary>
        /// Trạng thái sử dụng (1 - Đang sử dụng, 0 - Ngừng sử dụng).
        /// </summary>
        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Ngày áp dụng ngừng sử dụng.
        /// </summary>
        [Column("inactive_effective_date")]
        public DateTime? InactiveEffectiveDate { get; set; }

        [Column("is_deleted")]
        public Guid? IsDeleted { get; set; } = Guid.Empty;

        // === NotMapped: dùng cho hiển thị FE ===

        /// <summary>
        /// Tên trưởng phòng (JOIN từ employee).
        /// </summary>
        [NotMapped]
        public string? ManagerEmployeeName { get; set; }
        /// <summary>
        /// Người tạo.
        /// </summary>
        [Column("created_by")]
        public Guid? CreatedBy { get; set; }

        /// <summary>
        /// Ngày tạo.
        /// </summary>
        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        /// <summary>
        /// Người cập nhật.
        /// </summary>
        [Column("updated_by")]
        public Guid? UpdatedBy { get; set; }

        /// <summary>
        /// Ngày cập nhật.
        /// </summary>
        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }
    }
}
