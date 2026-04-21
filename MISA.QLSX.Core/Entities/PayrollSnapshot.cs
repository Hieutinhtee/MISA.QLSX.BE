using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MISA.QLSX.Core.Entities
{
    [Table("payroll_snapshot")]
    public class PayrollSnapshot
    {
        [Key]
        [Column("payroll_snapshot_id")]
        public Guid? PayrollSnapshotId { get; set; }

        [Column("payroll_id")]
        public Guid? PayrollId { get; set; }

        [Column("snapshot_at")]
        public DateTime? SnapshotAt { get; set; }

        [Column("contract_payload")]
        public string? ContractPayload { get; set; }

        [Column("policy_payload")]
        public string? PolicyPayload { get; set; }

        [Column("tax_profile_payload")]
        public string? TaxProfilePayload { get; set; }

        [Column("payroll_payload")]
        public string? PayrollPayload { get; set; }

        [Column("items_payload")]
        public string? ItemsPayload { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        [Column("created_by")]
        public Guid? CreatedBy { get; set; }
    }
}