using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MISA.QLSX.Core.Entities
{
    [Table("business_trip")]
    public class BusinessTrip
    {
        [Key]
        [Column("business_trip_id")]
        public Guid? BusinessTripId { get; set; }

        [Column("business_trip_code")]
        public string? BusinessTripCode { get; set; }

        [Column("employee_id")]
        public Guid? EmployeeId { get; set; }

        [NotMapped]
        public string? EmployeeCode { get; set; }

        [NotMapped]
        public string? EmployeeName { get; set; }

        [Column("start_date")]
        public DateTime? StartDate { get; set; }

        [Column("end_date")]
        public DateTime? EndDate { get; set; }

        [Column("location")]
        public string? Location { get; set; }

        [Column("purpose")]
        public string? Purpose { get; set; }

        [Column("support_amount")]
        public decimal? SupportAmount { get; set; }

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
