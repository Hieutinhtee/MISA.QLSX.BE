using MISA.QLSX.Core.DTOs.Responses;
using MISA.QLSX.Core.Entities;
using MISA.QLSX.Core.Exceptions;
using MISA.QLSX.Core.Interfaces.Repository;
using MISA.QLSX.Core.Interfaces.Service;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace MISA.QLSX.Core.Services
{
    public class AttendanceService : BaseServices<Attendance>, IAttendanceService
    {
        private readonly IAttendanceRepository _attendanceRepository;

        public AttendanceService(IAttendanceRepository repo)
            : base(repo)
        {
            _attendanceRepository = repo;
        }

        public async Task<AttendanceDashboardDto> GetAttendanceDashboard(DateTime date)
        {
            var res = new AttendanceDashboardDto();

            // 1. Lấy danh sách vắng mặt
            var absents = await _attendanceRepository.GetAbsentEmployeesTodayAsync(date);
            foreach (var item in absents)
            {
                res.AbsentsToday.Add(new AbsentTodayItem
                {
                    EmployeeId = item.EmployeeId,
                    EmployeeCode = item.EmployeeCode,
                    FullName = item.FullName,
                    DepartmentName = item.DepartmentName,
                    AvatarUrl = item.AvatarUrl
                });
            }
            res.TotalAbsentToday = res.AbsentsToday.Count;

            // 2. Lấy bảng xếp hạng đi muộn (trong tháng của date)
            var lateRankings = await _attendanceRepository.GetLateRankingsAsync(date.Month, date.Year);
            foreach (var item in lateRankings)
            {
                res.LateRankings.Add(new LateRankingItem
                {
                    EmployeeId = item.EmployeeId,
                    EmployeeCode = item.EmployeeCode,
                    FullName = item.FullName,
                    AvatarUrl = item.AvatarUrl,
                    LateCount = (int)item.LateCount
                });
            }

            // 3. Tổng số lần đi muộn trong tháng
            var monthlyData = await _attendanceRepository.GetAttendancesByDateAsync(date); // This is just for today, need a month one or aggregate
            // For simplicity, we'll just sum the late rankings
            res.TotalLateThisMonth = res.LateRankings.Sum(x => x.LateCount);

            return res;
        }

        public async Task<AttendanceCalendarDto> GetEmployeeCalendar(Guid employeeId, int month, int year)
        {
            var records = await _attendanceRepository.GetAttendancesByEmployeeInMonthAsync(employeeId, month, year);
            var res = new AttendanceCalendarDto();

            foreach (var r in records)
            {
                res.Records.Add(new AttendanceRecordDto
                {
                    AttendanceId = r.AttendanceId ?? Guid.Empty,
                    Date = r.AttendanceDate ?? DateTime.MinValue,
                    CheckIn = r.CheckIn?.ToString(@"hh\:mm") ?? "",
                    CheckOut = r.CheckOut?.ToString(@"hh\:mm") ?? "",
                    Status = r.Status ?? "absent",
                    WorkingHours = r.WorkingHours ?? 0,
                    OvertimeHours = r.OvertimeHours ?? 0,
                    LateMinutes = r.LateMinutes ?? 0
                });
            }

            // Summary calculation
            res.Summary.TotalWorkingDays = res.Records.Where(r => r.Status != "absent").Sum(r => r.WorkingHours) / 8.0m;
            res.Summary.TotalOvertimeHours = res.Records.Sum(r => r.OvertimeHours);
            res.Summary.TotalLateTimes = res.Records.Count(r => r.Status == "late");
            res.Summary.TotalDaysOff = res.Records.Count(r => r.Status == "absent" || r.Status == "on_leave");

            return res;
        }

        protected override Task BeforeSaveAsync(Attendance entity, bool isUpdate = false)
        {
            if (!isUpdate)
                entity.CreatedAt = DateTime.Now;

            entity.UpdatedAt = DateTime.Now;
            return Task.CompletedTask;
        }

        protected override async Task ValidateAsync(Attendance entity, Guid? ignoreId = null)
        {
            if (entity == null)
                throw new ValidateException("Attendance object is null", "Dữ liệu chấm công không được để trống");

            if (string.IsNullOrWhiteSpace(entity.AttendanceCode))
                throw new ValidateException("AttendanceCode required", "Mã chấm công không được để trống");

            if (entity.EmployeeId == null)
                throw new ValidateException("EmployeeId required", "Nhân viên không được để trống");

            if (entity.AttendanceDate == null)
                throw new ValidateException("AttendanceDate required", "Ngày chấm công không được để trống");

            if ((entity.WorkingHours ?? 0) < 0 || (entity.OvertimeHours ?? 0) < 0)
                throw new ValidateException("Hours invalid", "Số giờ công hoặc tăng ca không được âm");

            if ((entity.PenaltyAmount ?? 0) < 0 || (entity.BonusAmount ?? 0) < 0)
                throw new ValidateException("Amount invalid", "Tiền thưởng/phạt không được âm");

            if (await _attendanceRepository.IsValueExistAsync(nameof(Attendance.AttendanceCode), entity.AttendanceCode, ignoreId))
                throw new ValidateException("AttendanceCode duplicate", "Mã chấm công đã tồn tại");
        }
    }
}