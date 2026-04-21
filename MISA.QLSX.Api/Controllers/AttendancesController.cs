using MISA.QLSX.Core.Entities;
using MISA.QLSX.Core.Interfaces.Service;

namespace MISA.QLSX.Api.Controllers
{
    public class AttendancesController : BaseController<Attendance>
    {
        public AttendancesController(IAttendanceService service)
            : base(service) { }
    }
}