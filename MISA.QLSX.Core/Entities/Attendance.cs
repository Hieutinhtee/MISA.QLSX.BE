using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MISA.QLSX.Core.Entities
{
    [Table("attendance")]
    public class Attendance
    {
        [Key]
        [Column("attendance_id")]
        public Guid? AttendanceId { get; set; }

        [Column("attendance_code")]
        public string? AttendanceCode { get; set; }

        [Column("employee_id")]
        public Guid? EmployeeId { get; set; }

        [Column("shift_id")]
        public Guid? ShiftId { get; set; }

        [Column("attendance_date")]
        public DateTime? AttendanceDate { get; set; }

        [Column("working_hours")]
        public decimal? WorkingHours { get; set; }

        [Column("overtime_hours")]
        public decimal? OvertimeHours { get; set; }

        [Column("penalty_amount")]
        public decimal? PenaltyAmount { get; set; }

        [Column("bonus_amount")]
        public decimal? BonusAmount { get; set; }

        [Column("net_income")]
        public decimal? NetIncome { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        [Column("created_by")]
        public Guid? CreatedBy { get; set; }

        [Column("updated_by")]
        public Guid? UpdatedBy { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }
    }
}