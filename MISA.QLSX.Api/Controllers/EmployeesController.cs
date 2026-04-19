using MISA.QLSX.Core.Entities;
using MISA.QLSX.Core.Interfaces.Service;

namespace MISA.QLSX.Api.Controllers
{
    public class EmployeesController : BaseController<Employee>
    {
        public EmployeesController(IEmployeeService employeeService)
            : base(employeeService) { }
    }
}
