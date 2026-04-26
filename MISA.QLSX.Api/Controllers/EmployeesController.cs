using Microsoft.AspNetCore.Mvc;
using MISA.QLSX.Api.Authorization;
using MISA.QLSX.Core.DTOs.Requests;
using MISA.QLSX.Core.DTOs.Responses;
using MISA.QLSX.Core.Entities;
using MISA.QLSX.Core.Exceptions;
using MISA.QLSX.Core.Interfaces.Service;

namespace MISA.QLSX.Api.Controllers
{
    /// <summary>
    /// Controller quản lý API nhân viên.
    /// </summary>
    [RequireRole("ADMIN", "HR", "MANAGER", "EMPLOYEE")]
    public class EmployeesController : BaseController<Employee>
    {
        private readonly IEmployeeService _employeeService;

        public EmployeesController(IEmployeeService employeeService)
            : base(employeeService)
        {
            _employeeService = employeeService;
        }

        /// <summary>
        /// Lấy danh sách nhân viên, chỉ ADMIN và HR được lấy toàn bộ.
        /// </summary>
        /// <returns>Danh sách nhân viên.</returns>
        [RequireRole("ADMIN", "HR")]
        public override async Task<IActionResult> GetAll()
        {
            return await base.GetAll();
        }

        /// <summary>
        /// Lấy chi tiết nhân viên theo ID với ràng buộc EMPLOYEE chỉ được xem bản thân.
        /// </summary>
        /// <param name="id">ID nhân viên cần xem.</param>
        /// <returns>Chi tiết nhân viên theo quyền truy cập.</returns>
        public override async Task<IActionResult> GetById(Guid id)
        {
            var role = HttpContext.Session.GetString("role_code")?.ToUpperInvariant();
            var sessionEmployeeId = HttpContext.Session.GetString("employee_id");

            if (role == "EMPLOYEE" && sessionEmployeeId != id.ToString())
            {
                throw new ForbiddenException(
                    "EMPLOYEE chỉ được xem dữ liệu của bản thân",
                    "Bạn không có quyền xem dữ liệu nhân viên khác"
                );
            }

            return await base.GetById(id);
        }

        /// <summary>
        /// Lấy danh sách phân trang với phạm vi theo role.
        /// </summary>
        /// <param name="request">Yêu cầu phân trang/tìm kiếm/lọc.</param>
        /// <returns>Dữ liệu phân trang theo quyền truy cập.</returns>
        [HttpPost("paging")]
        public override async Task<PagingResponse<Employee>> GetPaging([FromBody] QueryRequest request)
        {
            request ??= new QueryRequest();
            request.Filters ??= new List<FilterCondition>();

            var role = HttpContext.Session.GetString("role_code")?.ToUpperInvariant();
            var employeeId = HttpContext.Session.GetString("employee_id");
            var departmentId = HttpContext.Session.GetString("department_id");

            if (role == "EMPLOYEE" && Guid.TryParse(employeeId, out var parsedEmployeeId))
            {
                request.Filters.RemoveAll(f => f.Field == "employeeId");
                request.Filters.Add(
                    new FilterCondition { Field = "employeeId", Operator = "eq", Value = parsedEmployeeId }
                );
            }

            if (role == "MANAGER" && Guid.TryParse(departmentId, out var parsedDepartmentId))
            {
                request.Filters.RemoveAll(f => f.Field == "departmentId");
                request.Filters.Add(
                    new FilterCondition
                    {
                        Field = "departmentId",
                        Operator = "eq",
                        Value = parsedDepartmentId,
                    }
                );
            }

            return await _employeeService.QueryPagingAsync(request);
        }

        /// <summary>
        /// Lấy danh sách nhân viên chưa có hợp đồng.
        /// </summary>
        /// <returns>Danh sách nhân viên có ContractId = null.</returns>
        [RequireRole("ADMIN", "HR")]
        [HttpGet("without-contract")]
        public async Task<IActionResult> GetEmployeesWithoutContract()
        {
            var data = await _employeeService.GetEmployeesWithoutContractAsync();
            return Ok(new { data });
        }
    }
}
