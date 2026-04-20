using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MISA.QLSX.Core.Entities
{
    [Table("salary_policy")]
    public class SalaryPolicy
    {
        [Key]
        [Column("policy_id")]
        public Guid? PolicyId { get; set; }

        [Column("policy_code")]
        public string? PolicyCode { get; set; }

        [Column("policy_name")]
        public string? PolicyName { get; set; }

        [Column("standard_workdays")]
        public decimal? StandardWorkdays { get; set; }

        [Column("overtime_multiplier_weekday")]
        public decimal? OvertimeMultiplierWeekday { get; set; }

        [Column("overtime_multiplier_weekend")]
        public decimal? OvertimeMultiplierWeekend { get; set; }

        [Column("overtime_multiplier_holiday")]
        public decimal? OvertimeMultiplierHoliday { get; set; }

        [Column("effective_from")]
        public DateTime? EffectiveFrom { get; set; }

        [Column("effective_to")]
        public DateTime? EffectiveTo { get; set; }

        [Column("is_active")]
        public bool? IsActive { get; set; }

        [Column("description")]
        public string? Description { get; set; }

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