using System;
using System.Collections.Generic;

namespace MISA.QLSX.Core.DTOs.Responses
{
    public class AttendanceDashboardDto
    {
        public int TotalAbsentToday { get; set; }
        public int TotalLateThisMonth { get; set; }
        public List<LateRankingItem> LateRankings { get; set; } = new();
        public List<AbsentTodayItem> AbsentsToday { get; set; } = new();
    }

    public class LateRankingItem
    {
        public Guid EmployeeId { get; set; }
        public string EmployeeCode { get; set; }
        public string FullName { get; set; }
        public int LateCount { get; set; }
        public string AvatarUrl { get; set; }
    }

    public class AbsentTodayItem
    {
        public Guid EmployeeId { get; set; }
        public string EmployeeCode { get; set; }
        public string FullName { get; set; }
        public string DepartmentName { get; set; }
        public string AvatarUrl { get; set; }
    }
}
