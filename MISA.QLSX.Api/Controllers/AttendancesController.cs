using Microsoft.AspNetCore.Mvc;
using MISA.QLSX.Core.Entities;
using MISA.QLSX.Api.Authorization;
using MISA.QLSX.Core.DTOs.Requests;
using MISA.QLSX.Core.DTOs.Responses;
using MISA.QLSX.Core.Interfaces.Service;

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

        /// <summary>
        /// Lấy toàn bộ dữ liệu chấm công, EMPLOYEE không được gọi endpoint này.
        /// </summary>
        /// <returns>Danh sách chấm công.</returns>
        [RequireRole("ADMIN", "HR", "MANAGER")]
        public override async Task<IActionResult> GetAll()
        {
            return await base.GetAll();
        }

        /// <summary>
        /// Lấy dữ liệu chấm công phân trang với ràng buộc EMPLOYEE chỉ xem bản thân.
        /// </summary>
        /// <param name="request">Yêu cầu phân trang/tìm kiếm/lọc.</param>
        /// <returns>Dữ liệu chấm công theo quyền truy cập.</returns>
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
    }
}