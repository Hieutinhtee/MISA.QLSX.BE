using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MISA.QLSX.Core.DTOs.Responses
{
    public class ShiftDtoRes
    {
        public Guid? ShiftId { get; set; }

        public string? ShiftCode { get; set; }

        public string? ShiftName { get; set; }

        public string? ShiftDescription { get; set; }

        public TimeSpan? ShiftBeginTime { get; set; }

        public TimeSpan? ShiftEndTime { get; set; }

        public TimeSpan? BeginBreakTime { get; set; }

        public TimeSpan? EndBreakTime { get; set; }

        public decimal? WorkingTime { get; set; }

        public decimal? BreakTime { get; set; }

        public bool? IsActive { get; set; }

        public string? CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; }

        public string? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }
    }
}
