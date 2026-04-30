using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MISA.QLSX.Core.Entities
{
    /// <summary>
    /// Đối tượng danh mục bằng cấp.
    /// </summary>
    [Table("degree")]
    public class Degree
    {
        /// <summary>
        /// ID bằng cấp (UUID).
        /// </summary>
        [Key]
        [Column("degree_id")]
        public Guid? DegreeId { get; set; }

        /// <summary>
        /// Mã bằng cấp.
        /// </summary>
        [Column("degree_code")]
        public string? DegreeCode { get; set; }

        /// <summary>
        /// Tên bằng cấp.
        /// </summary>
        [Column("degree_name")]
        public string? DegreeName { get; set; }

        /// <summary>
        /// Mô tả bằng cấp.
        /// </summary>
        [Column("description")]
        public string? Description { get; set; }

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
