using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MISA.QLSX.Core.Entities
{
    [Table("contract")]
    public class Contract
    {
        [Key]
        [Column("contract_id")]
        public Guid? ContractId { get; set; }

        [Column("contract_code")]
        public string? ContractCode { get; set; }

        [Column("template_id")]
        public Guid? TemplateId { get; set; }

        [NotMapped]
        public string? TemplateCode { get; set; }

        [NotMapped]
        public string? TemplateName { get; set; }

        [NotMapped]
        public string? ContractType { get; set; }

        [Column("company_representative_id")]
        public Guid? CompanyRepresentativeId { get; set; }

        [NotMapped]
        public string? CompanyRepresentativeCode { get; set; }

        [NotMapped]
        public string? CompanyRepresentativeName { get; set; }

        [Column("company_signer_title")]
        public string? CompanySignerTitle { get; set; }

        [Column("employee_id")]
        public Guid? EmployeeId { get; set; }

        [NotMapped]
        public string? EmployeeCode { get; set; }

        [NotMapped]
        public string? EmployeeName { get; set; }

        [Column("effective_date")]
        public DateTime? EffectiveDate { get; set; }

        [Column("term_months")]
        public int? TermMonths { get; set; }

        [Column("base_salary")]
        public decimal? BaseSalary { get; set; }

        [Column("insurance_salary")]
        public decimal? InsuranceSalary { get; set; }

        [Column("salary_ratio")]
        public decimal? SalaryRatio { get; set; }

        [Column("summary")]
        public string? Summary { get; set; }

        [Column("attachment_link")]
        public string? AttachmentLink { get; set; }

        [Column("is_signed")]
        public bool? IsSigned { get; set; }

        [Column("signed_at")]
        public DateTime? SignedAt { get; set; }

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
