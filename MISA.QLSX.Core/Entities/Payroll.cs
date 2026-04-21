using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MISA.QLSX.Core.Entities
{
    [Table("payroll")]
    public class Payroll
    {
        [Key]
        [Column("payroll_id")]
        public Guid? PayrollId { get; set; }

        [Column("payroll_code")]
        public string? PayrollCode { get; set; }

        [Column("salary_period_id")]
        public Guid? SalaryPeriodId { get; set; }

        [Column("employee_id")]
        public Guid? EmployeeId { get; set; }

        [Column("status")]
        public string? Status { get; set; }

        [Column("gross_salary")]
        public decimal? GrossSalary { get; set; }

        [Column("net_salary")]
        public decimal? NetSalary { get; set; }

        [Column("taxable_salary")]
        public decimal? TaxableSalary { get; set; }

        [Column("pit_tax_amount")]
        public decimal? PitTaxAmount { get; set; }

        [Column("insurance_deduction")]
        public decimal? InsuranceDeduction { get; set; }

        [Column("working_days_actual")]
        public decimal? WorkingDaysActual { get; set; }

        [Column("working_days_standard")]
        public decimal? WorkingDaysStandard { get; set; }

        [Column("total_allowance")]
        public decimal? TotalAllowance { get; set; }

        [Column("total_addition")]
        public decimal? TotalAddition { get; set; }

        [Column("total_deduction")]
        public decimal? TotalDeduction { get; set; }

        [Column("locked_at")]
        public DateTime? LockedAt { get; set; }

        [Column("paid_at")]
        public DateTime? PaidAt { get; set; }

        [Column("salary_policy_id")]
        public Guid? SalaryPolicyId { get; set; }

        [Column("deduction_policy_id")]
        public Guid? DeductionPolicyId { get; set; }

        [Column("employee_tax_profile_id")]
        public Guid? EmployeeTaxProfileId { get; set; }

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