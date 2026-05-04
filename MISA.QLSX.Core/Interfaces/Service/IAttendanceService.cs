using MISA.QLSX.Core.DTOs.Responses;
using MISA.QLSX.Core.Entities;
using System;
using System.Threading.Tasks;

namespace MISA.QLSX.Core.Interfaces.Service
{
    public interface IAttendanceService : IBaseService<Attendance>
    {
        Task<AttendanceDashboardDto> GetAttendanceDashboard(DateTime date);
        Task<AttendanceCalendarDto> GetEmployeeCalendar(Guid employeeId, int month, int year);
    }
}