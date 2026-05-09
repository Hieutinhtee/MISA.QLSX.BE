using Microsoft.AspNetCore.Mvc;
using MISA.QLSX.Core.Entities;
using MISA.QLSX.Api.Authorization;
using MISA.QLSX.Core.DTOs.Requests;
using MISA.QLSX.Core.DTOs.Responses;
using MISA.QLSX.Core.Exceptions;
using MISA.QLSX.Core.Interfaces.Service;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace MISA.QLSX.Api.Controllers
{
    [RequireRole("ADMIN", "HR", "MANAGER", "EMPLOYEE")]
    public class AttendancesController : BaseController<Attendance>
    {
        private readonly IAttendanceService _attendanceService;

        public AttendancesController(IAttendanceService service)
            : base(service)
        {
            _attendanceService = service;
        }

        [RequireRole("ADMIN", "HR", "MANAGER")]
        public override async Task<IActionResult> GetAll()
        {
            return await base.GetAll();
        }

        [HttpPost("paging")]
        public override async Task<PagingResponse<Attendance>> GetPaging(
            [FromBody] QueryRequest request
        )
        {
            request ??= new QueryRequest();
            request.Filters ??= new List<FilterCondition>();

            var role = HttpContext.Session.GetString("role_code")?.ToUpperInvariant();
            var employeeId = HttpContext.Session.GetString("employee_id");

            if (role == "EMPLOYEE" && Guid.TryParse(employeeId, out var parsedEmployeeId))
            {
                request.Filters.RemoveAll(f => f.Field == "employeeId");
                request.Filters.Add(
                    new FilterCondition { Field = "employeeId", Operator = "eq", Value = parsedEmployeeId }
                );
            }

            return await _attendanceService.QueryPagingAsync(request);
        }

        /// <summary>
        /// Lấy chi tiết chấm công theo ID với ràng buộc EMPLOYEE chỉ được xem bản thân.
        /// </summary>
        /// <param name="id">ID bản ghi chấm công cần xem.</param>
        /// <returns>Chi tiết chấm công theo quyền truy cập.</returns>
        public override async Task<IActionResult> GetById(Guid id)
        {
            var role = HttpContext.Session.GetString("role_code")?.ToUpperInvariant();

            if (role == "EMPLOYEE")
            {
                var attendance = await _attendanceService.GetByIdAsync(id);
                var sessionEmployeeId = HttpContext.Session.GetString("employee_id");

                if (!attendance.EmployeeId.HasValue || !Guid.TryParse(sessionEmployeeId, out var parsedId) || attendance.EmployeeId.Value != parsedId)
                {
                    throw new ForbiddenException(
                        "EMPLOYEE chỉ được xem dữ liệu chấm công của bản thân",
                        "Bạn không có quyền xem chấm công của nhân viên khác"
                    );
                }

                return Ok(new { data = attendance });
            }

            return await base.GetById(id);
        }

        [HttpGet("dashboard")]
        [RequireRole("ADMIN", "HR", "MANAGER")]
        public async Task<IActionResult> GetDashboard([FromQuery] DateTime? date)
        {
            var res = await _attendanceService.GetAttendanceDashboard(date ?? DateTime.Now);
            return Ok(res);
        }

        [HttpGet("employee/{employeeId}/calendar")]
        public async Task<IActionResult> GetEmployeeCalendar(Guid employeeId, [FromQuery] int month, [FromQuery] int year)
        {
            var role = HttpContext.Session.GetString("role_code")?.ToUpperInvariant();
            var sessionEmployeeId = HttpContext.Session.GetString("employee_id");

            if (role == "EMPLOYEE" && (!Guid.TryParse(sessionEmployeeId, out var parsedId) || parsedId != employeeId))
            {
                throw new ForbiddenException(
                    "EMPLOYEE chỉ được xem lịch chấm công của bản thân",
                    "Bạn không có quyền xem lịch chấm công của nhân viên khác"
                );
            }

            var res = await _attendanceService.GetEmployeeCalendar(employeeId, month, year);
            return Ok(res);
        }
    }
}