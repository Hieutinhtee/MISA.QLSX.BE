using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MISA.QLSX.Core.Entities
{
    [Table("tax_bracket")]
    public class TaxBracket
    {
        [Key]
        [Column("tax_bracket_id")]
        public Guid? TaxBracketId { get; set; }

        [Column("bracket_code")]
        public string? BracketCode { get; set; }

        [Column("bracket_name")]
        public string? BracketName { get; set; }

        [Column("lower_bound")]
        public decimal? LowerBound { get; set; }

        [Column("upper_bound")]
        public decimal? UpperBound { get; set; }

        [Column("tax_rate")]
        public decimal? TaxRate { get; set; }

        [Column("quick_deduction")]
        public decimal? QuickDeduction { get; set; }

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
