using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MISA.QLSX.Core.Entities
{
    [Table("dependent")]
    public class Dependent
    {
        [Key]
        [Column("dependent_id")]
        public Guid? DependentId { get; set; }

        [Column("employee_id")]
        public Guid? EmployeeId { get; set; }

        [Column("full_name")]
        public string? FullName { get; set; }

        [Column("date_of_birth")]
        public DateTime? DateOfBirth { get; set; }

        [Column("relationship")]
        public string? Relationship { get; set; }

        [Column("tax_code")]
        public string? TaxCode { get; set; }

        [Column("identity_number")]
        public string? IdentityNumber { get; set; }

        [Column("start_date")]
        public DateTime? StartDate { get; set; }

        [Column("end_date")]
        public DateTime? EndDate { get; set; }

        [Column("is_active")]
        public bool? IsActive { get; set; }

        [Column("note")]
        public string? Note { get; set; }

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
