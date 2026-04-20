using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MISA.QLSX.Core.Entities
{
    [Table("deduction_policy")]
    public class DeductionPolicy
    {
        [Key]
        [Column("deduction_policy_id")]
        public Guid? DeductionPolicyId { get; set; }

        [Column("policy_code")]
        public string? PolicyCode { get; set; }

        [Column("policy_name")]
        public string? PolicyName { get; set; }

        [Column("social_insurance_rate")]
        public decimal? SocialInsuranceRate { get; set; }

        [Column("health_insurance_rate")]
        public decimal? HealthInsuranceRate { get; set; }

        [Column("unemployment_insurance_rate")]
        public decimal? UnemploymentInsuranceRate { get; set; }

        [Column("personal_deduction_amount")]
        public decimal? PersonalDeductionAmount { get; set; }

        [Column("dependent_deduction_amount")]
        public decimal? DependentDeductionAmount { get; set; }

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
