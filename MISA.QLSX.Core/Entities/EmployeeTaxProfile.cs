using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MISA.QLSX.Core.Entities
{
    [Table("employee_tax_profile")]
    public class EmployeeTaxProfile
    {
        [Key]
        [Column("employee_tax_profile_id")]
        public Guid? EmployeeTaxProfileId { get; set; }

        [Column("employee_id")]
        public Guid? EmployeeId { get; set; }

        [Column("tax_code")]
        public string? TaxCode { get; set; }

        [Column("dependent_count")]
        public int? DependentCount { get; set; }

        [Column("is_resident")]
        public bool? IsResident { get; set; }

        [Column("effective_from")]
        public DateTime? EffectiveFrom { get; set; }

        [Column("effective_to")]
        public DateTime? EffectiveTo { get; set; }

        [Column("is_active")]
        public bool? IsActive { get; set; }

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
