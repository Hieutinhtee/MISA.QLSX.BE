using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MISA.QLSX.Core.Entities
{
    [Table("employee")]
    public class Employee
    {
        [Key]
        [Column("employee_id")]
        public Guid? EmployeeId { get; set; }

        [Column("employee_code")]
        public string? EmployeeCode { get; set; }

        [Column("full_name")]
        public string? FullName { get; set; }

        [Column("gender")]
        public string? Gender { get; set; }

        [Column("date_of_birth")]
        public DateTime? DateOfBirth { get; set; }

        [Column("address")]
        public string? Address { get; set; }

        [Column("phone_number")]
        public string? PhoneNumber { get; set; }

        [Column("email")]
        public string? Email { get; set; }

        [Column("join_date")]
        public DateTime? JoinDate { get; set; }

        [Column("department_id")]
        public Guid? DepartmentId { get; set; }

        [NotMapped]
        public string? DepartmentName { get; set; }

        [Column("shift_id")]
        public Guid? ShiftId { get; set; }

        [NotMapped]
        public string? ShiftName { get; set; }

        [Column("national_id")]
        public string? NationalId { get; set; }

        [Column("degree_id")]
        public Guid? DegreeId { get; set; }

        [NotMapped]
        public string? DegreeName { get; set; }

        [Column("contract_id")]
        public Guid? ContractId { get; set; }

        [NotMapped]
        public string? ContractCode { get; set; }

        [Column("position_id")]
        public Guid? PositionId { get; set; }

        [NotMapped]
        public string? PositionName { get; set; }

        [Column("account_id")]
        public Guid? AccountId { get; set; }

        [NotMapped]
        public string? AccountName { get; set; }

        [Column("avatar_url")]
        public string? AvatarUrl { get; set; }

        [Column("cv_url")]
        public string? CvUrl { get; set; }

        [Column("place_of_birth")]
        public string? PlaceOfBirth { get; set; }

        [Column("hometown")]
        public string? Hometown { get; set; }

        [Column("ethnic")]
        public string? Ethnic { get; set; }

        [Column("religion")]
        public string? Religion { get; set; }

        [Column("nationality")]
        public string? Nationality { get; set; }

        [Column("marital_status")]
        public string? MaritalStatus { get; set; }

        [Column("personal_email")]
        public string? PersonalEmail { get; set; }

        [Column("facebook_url")]
        public string? FacebookUrl { get; set; }

        [Column("zalo_number")]
        public string? ZaloNumber { get; set; }

        [Column("temporary_address")]
        public string? TemporaryAddress { get; set; }

        [Column("bank_account_number")]
        public string? BankAccountNumber { get; set; }

        [Column("bank_name")]
        public string? BankName { get; set; }

        [Column("bank_branch")]
        public string? BankBranch { get; set; }

        [Column("emergency_contact_name")]
        public string? EmergencyContactName { get; set; }

        [Column("emergency_contact_phone")]
        public string? EmergencyContactPhone { get; set; }

        [Column("emergency_contact_relationship")]
        public string? EmergencyContactRelationship { get; set; }

        [Column("height")]
        public decimal? Height { get; set; }

        [Column("weight")]
        public decimal? Weight { get; set; }

        [Column("blood_group")]
        public string? BloodGroup { get; set; }

        [Column("health_status")]
        public string? HealthStatus { get; set; }

        [Column("social_insurance_number")]
        public string? SocialInsuranceNumber { get; set; }

        [Column("is_deleted")]
        public Guid? IsDeleted { get; set; } = Guid.Empty;

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
