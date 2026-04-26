using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MISA.QLSX.Core.Entities
{
    /// <summary>
    /// Đối tượng vai trò hệ thống
    /// </summary>
    [Table("role")]
    public class Role
    {
        /// <summary>
        /// ID vai trò (UUID)
        /// </summary>
        [Key]
        [Column("role_id")]
        public Guid? RoleId { get; set; }

        /// <summary>
        /// Mã vai trò
        /// </summary>
        [Column("role_code")]
        public string? RoleCode { get; set; }

        /// <summary>
        /// Tên vai trò
        /// </summary>
        [Column("role_name")]
        public string? RoleName { get; set; }

        /// <summary>
        /// Mô tả vai trò
        /// </summary>
        [Column("description")]
        public string? Description { get; set; }

        /// <summary>
        /// Thời điểm tạo vai trò
        /// </summary>
        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        /// <summary>
        /// ID người tạo vai trò
        /// </summary>
        [Column("created_by")]
        public Guid? CreatedBy { get; set; }

        /// <summary>
        /// ID người cập nhật vai trò lần cuối
        /// </summary>
        [Column("updated_by")]
        public Guid? UpdatedBy { get; set; }

        /// <summary>
        /// Thời điểm cập nhật vai trò lần cuối
        /// </summary>
        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }
    }
}
