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
    [Table("shift")]
    public class Shift
    {
        /// <summary>
        /// ID ca làm việc (UUID)
        /// </summary>
        [Key]
        [Column("shift_id")]
        public Guid? ShiftId { get; set; }

        /// <summary>
        /// Mã ca làm việc
        /// </summary>
        [Column("shift_code")]
        public string? ShiftCode { get; set; }

        /// <summary>
        /// Tên ca làm việc
        /// </summary>
        [Column("shift_name")]
        public string? ShiftName { get; set; }

        /// <summary>
        /// Tên ca làm việc
        /// </summary>
        [Column("description")]
        public string? Description { get; set; }

        /// <summary>
        /// Thời gian bắt đầu ca
        /// </summary>
        [Column("start_time")]
        public TimeSpan? StartTime { get; set; }

        /// <summary>
        /// Thời gian bắt đầu nghỉ giữa ca
        /// </summary>
        [Column("break_start_time")]
        public TimeSpan? BreakStartTime { get; set; }

        /// <summary>
        /// Thời gian kết thúc ca
        /// </summary>
        [Column("end_time")]
        public TimeSpan? EndTime { get; set; }

        /// <summary>
        /// Thời gian kết thúc nghỉ giữa ca
        /// </summary>
        [Column("break_end_time")]
        public TimeSpan? BreakEndTime { get; set; }

        /// <summary>
        /// Tổng thời gian làm việc (giờ)
        /// </summary>
        [Column("working_hours")]
        public decimal? WorkingHours { get; set; }

        /// <summary>
        /// Tổng thời gian nghỉ (giờ)
        /// </summary>
        [Column("break_hours")]
        public decimal? BreakHours { get; set; }

        /// <summary>
        /// Trạng thái hoạt động (1 - Đang hoạt động, 0 - Ngừng hoạt động)
        /// </summary>
        [Column("is_active")]
        public bool? IsActive { get; set; }

        /// <summary>
        /// Người tạo
        /// </summary>
        [Column("created_by")]
        public Guid? CreatedBy { get; set; }

        /// <summary>
        /// Ngày tạo
        /// </summary>
        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        /// <summary>
        /// Người sửa
        /// </summary>
        [Column("updated_by")]
        public Guid? UpdatedBy { get; set; }

        /// <summary>
        /// Ngày sửa
        /// </summary>
        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }
    }
}
