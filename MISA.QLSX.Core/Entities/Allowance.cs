using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MISA.QLSX.Core.Entities
{
    [Table("allowance")]
    public class Allowance
    {
        [Key]
        [Column("allowance_id")]
        public Guid? AllowanceId { get; set; }

        [Column("allowance_code")]
        public string? AllowanceCode { get; set; }

        [Column("allowance_name")]
        public string? AllowanceName { get; set; }

        [Column("calculation_type")]
        public string? CalculationType { get; set; }

        [Column("amount")]
        public decimal? Amount { get; set; }

        [Column("percent")]
        public decimal? Percent { get; set; }

        [Column("version")]
        public int? Version { get; set; }

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
