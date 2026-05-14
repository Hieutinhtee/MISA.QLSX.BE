using Microsoft.AspNetCore.Mvc;
using MISA.QLSX.Core.Entities;
using MISA.QLSX.Core.Interfaces.Service;

namespace MISA.QLSX.Api.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class DependentsController : BaseController<Dependent>
    {
        private readonly IDependentService _dependentService;

        public DependentsController(IDependentService service) : base(service)
        {
            _dependentService = service;
        }

        /// <summary>
        /// Lấy danh sách người phụ thuộc theo ID nhân viên
        /// </summary>
        /// <param name="employeeId">ID nhân viên</param>
        /// <returns>Danh sách người phụ thuộc</returns>
        [HttpGet("employee/{employeeId}")]
        public async Task<IActionResult> GetByEmployeeId(Guid employeeId)
        {
            var result = await _dependentService.GetByEmployeeIdAsync(employeeId);
            return Ok(result);
        }
    }
}
