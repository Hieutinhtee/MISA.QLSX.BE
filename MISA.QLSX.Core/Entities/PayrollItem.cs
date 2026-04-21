using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MISA.QLSX.Core.Entities
{
    [Table("payroll_item")]
    public class PayrollItem
    {
        [Key]
        [Column("payroll_item_id")]
        public Guid? PayrollItemId { get; set; }

        [Column("payroll_item_code")]
        public string? PayrollItemCode { get; set; }

        [Column("payroll_id")]
        public Guid? PayrollId { get; set; }

        [Column("item_type")]
        public string? ItemType { get; set; }

        [Column("item_name")]
        public string? ItemName { get; set; }

        [Column("formula_component")]
        public string? FormulaComponent { get; set; }

        [Column("amount")]
        public decimal? Amount { get; set; }

        [Column("source_table")]
        public string? SourceTable { get; set; }

        [Column("source_id")]
        public Guid? SourceId { get; set; }

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