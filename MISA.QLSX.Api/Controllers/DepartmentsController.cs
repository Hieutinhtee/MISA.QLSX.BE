using Microsoft.AspNetCore.Mvc;
using MISA.QLSX.Core.Entities;
using MISA.QLSX.Core.Interfaces.Service;

namespace MISA.QLSX.Api.Controllers
{
    /// <summary>
    /// Controller quản lý API danh mục phòng ban.
    /// </summary>
    public class DepartmentsController : BaseController<Department>
    {
        public DepartmentsController(IDepartmentService departmentService)
            : base(departmentService) { }
    }
}
