using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MISA.QLSX.Core.Entities
{
    /// <summary>
    /// Đối tượng danh mục chức vụ.
    /// </summary>
    [Table("position")]
    public class Position
    {
        /// <summary>
        /// ID chức vụ (UUID).
        /// </summary>
        [Key]
        [Column("position_id")]
        public Guid? PositionId { get; set; }

        /// <summary>
        /// Mã chức vụ.
        /// </summary>
        [Column("position_code")]
        public string? PositionCode { get; set; }

        /// <summary>
        /// Tên chức vụ.
        /// </summary>
        [Column("position_name")]
        public string? PositionName { get; set; }

        /// <summary>
        /// Mô tả chức vụ.
        /// </summary>
        [Column("description")]
        public string? Description { get; set; }

        /// <summary>
        /// Mức phụ cấp chức vụ.
        /// </summary>
        [Column("allowance")]
        public decimal Allowance { get; set; }

        [Column("is_deleted")]
        public Guid? IsDeleted { get; set; } = Guid.Empty;

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
