using System;
using System.Collections.Generic;

namespace MISA.QLSX.Core.DTOs.Responses
{
    public class AttendanceCalendarDto
    {
        public List<AttendanceRecordDto> Records { get; set; } = new();
        public AttendanceSummaryDto Summary { get; set; } = new();
    }

    public class AttendanceRecordDto
    {
        public Guid AttendanceId { get; set; }
        public DateTime Date { get; set; }
        public string ShiftCode { get; set; }
        public string CheckIn { get; set; }
        public string CheckOut { get; set; }
        public string Status { get; set; }
        public decimal WorkingHours { get; set; }
        public decimal OvertimeHours { get; set; }
        public int LateMinutes { get; set; }
    }

    public class AttendanceSummaryDto
    {
        public decimal TotalWorkingDays { get; set; }
        public decimal TotalOvertimeHours { get; set; }
        public int TotalLateTimes { get; set; }
        public decimal TotalDaysOff { get; set; }
    }
}
