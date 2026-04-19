using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MISA.QLSX.Core.Entities
{
    [Table("contract_template")]
    public class ContractTemplate
    {
        [Key]
        [Column("template_id")]
        public Guid? TemplateId { get; set; }

        [Column("template_code")]
        public string? TemplateCode { get; set; }

        [Column("template_name")]
        public string? TemplateName { get; set; }

        [Column("contract_type")]
        public string? ContractType { get; set; }

        [Column("content")]
        public string? Content { get; set; }

        [Column("version")]
        public int? Version { get; set; }

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
