using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MISA.QLSX.Core.Entities
{
    /// <summary>
    /// Đối tượng ca làm việc
    /// Created by: TMHieu 03/12/2025
    /// </summary>
    [Table("production_shift")]
    public class Shift
    {
        /// <summary>
        /// ID ca làm việc (UUID)
        /// </summary>
        [Key]
        [Column("production_shift_id")]
        public Guid? ProductionShiftId { get; set; }

        /// <summary>
        /// Mã ca làm việc
        /// </summary>
        [MaxLength(20)]
        [Column("production_shift_code")]
        public string? ProductionShiftCode { get; set; }

        /// <summary>
        /// Tên ca làm việc
        /// </summary>
        [MaxLength(50)]
        [Column("production_shift_name")]
        public string? ProductionShiftName { get; set; }

        /// <summary>
        /// Tên ca làm việc
        /// </summary>
        [MaxLength(255)]
        [Column("production_shift_description")]
        public string? ProductionShiftDescription { get; set; }

        /// <summary>
        /// Thời gian bắt đầu ca
        /// </summary>
        [Column("production_shift_begin_time")]
        public TimeSpan? ProductionShiftBeginTime { get; set; }

        /// <summary>
        /// Thời gian bắt đầu nghỉ giữa ca
        /// </summary>
        [Column("production_shift_begin_break_time")]
        public TimeSpan? ProductionShiftBeginBreakTime { get; set; }

        /// <summary>
        /// Thời gian kết thúc ca
        /// </summary>
        [Column("production_shift_end_time")]
        public TimeSpan? ProductionShiftEndTime { get; set; }

        /// <summary>
        /// Thời gian kết thúc nghỉ giữa ca
        /// </summary>
        [Column("production_shift_end_break_time")]
        public TimeSpan? ProductionShiftEndBreakTime { get; set; }

        /// <summary>
        /// Tổng thời gian làm việc (giờ)
        /// </summary>
        [Column("production_shift_working_time", TypeName = "decimal(18,2)")]
        public decimal? ProductionShiftWorkingTime { get; set; }

        /// <summary>
        /// Tổng thời gian nghỉ (giờ)
        /// </summary>
        [Column("production_shift_break_time", TypeName = "decimal(18,2)")]
        public decimal? ProductionShiftBreakTime { get; set; }

        /// <summary>
        /// Trạng thái hoạt động (1 - Đang hoạt động, 0 - Ngừng hoạt động)
        /// </summary>
        [Column("production_shift_is_active")]
        public bool? ProductionShiftIsActive { get; set; }

        /// <summary>
        /// Người tạo
        /// </summary>
        [MaxLength(255)]
        [Column("production_shift_created_by")]
        public string? ProductionShiftCreatedBy { get; set; }

        /// <summary>
        /// Ngày tạo
        /// </summary>
        [Column("production_shift_created_date")]
        public DateTime? ProductionShiftCreatedDate { get; set; }

        /// <summary>
        /// Người sửa
        /// </summary>
        [MaxLength(255)]
        [Column("production_shift_modified_by")]
        public string? ProductionShiftModifiedBy { get; set; }

        /// <summary>
        /// Ngày sửa
        /// </summary>
        [Column("production_shift_modified_date")]
        public DateTime? ProductionShiftModifiedDate { get; set; }
    }
}
