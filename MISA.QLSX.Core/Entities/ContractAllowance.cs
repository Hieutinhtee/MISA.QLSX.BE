using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MISA.QLSX.Core.Entities
{
    /// <summary>
    /// Liên kết giữa hợp đồng và phụ cấp áp dụng cho hợp đồng đó.
    /// </summary>
    [Table("contract_allowance")]
    public class ContractAllowance
    {
        /// <summary>
        /// Khóa chính UUID của liên kết hợp đồng phụ cấp.
        /// </summary>
        [Key]
        [Column("contract_allowance_id")]
        public Guid? ContractAllowanceId { get; set; }

        /// <summary>
        /// Khóa ngoại tham chiếu hợp đồng.
        /// </summary>
        [Column("contract_id")]
        public Guid? ContractId { get; set; }

        /// <summary>
        /// Khóa ngoại tham chiếu phụ cấp.
        /// </summary>
        [Column("allowance_id")]
        public Guid? AllowanceId { get; set; }

        /// <summary>
        /// Thời điểm tạo liên kết hợp đồng và phụ cấp.
        /// </summary>
        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        /// <summary>
        /// UUID người tạo dữ liệu liên kết phụ cấp.
        /// </summary>
        [Column("created_by")]
        public Guid? CreatedBy { get; set; }

        /// <summary>
        /// UUID người cập nhật gần nhất của liên kết phụ cấp.
        /// </summary>
        [Column("updated_by")]
        public Guid? UpdatedBy { get; set; }

        /// <summary>
        /// Thời điểm cập nhật gần nhất của liên kết phụ cấp.
        /// </summary>
        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Thông tin phụ cấp (tạm không mapped từ DB, dùng cho hiển thị).
        /// </summary>
        [NotMapped]
        public Allowance? AllowanceDetail { get; set; }
    }
}
