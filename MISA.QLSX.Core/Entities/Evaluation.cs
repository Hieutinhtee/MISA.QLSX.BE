using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MISA.QLSX.Core.Entities
{
    [Table("evaluation")]
    public class Evaluation
    {
        [Key]
        [Column("evaluation_id")]
        public Guid? EvaluationId { get; set; }

        [Column("evaluation_code")]
        public string? EvaluationCode { get; set; }

        [Column("employee_id")]
        public Guid? EmployeeId { get; set; }

        [NotMapped]
        public string? EmployeeCode { get; set; }

        [NotMapped]
        public string? EmployeeName { get; set; }

        [Column("evaluation_type")]
        public string? EvaluationType { get; set; }

        [Column("reason")]
        public string? Reason { get; set; }

        [Column("amount")]
        public decimal? Amount { get; set; }

        [Column("evaluation_date")]
        public DateTime? EvaluationDate { get; set; }

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
