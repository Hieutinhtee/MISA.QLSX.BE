using Microsoft.AspNetCore.Mvc;
using MISA.QLSX.Core.Entities;
using MISA.QLSX.Core.Interfaces.Service;

namespace MISA.QLSX.Api.Controllers
{
    /// <summary>
    /// Controller quản lý API danh mục chức vụ.
    /// </summary>
    public class PositionsController : BaseController<Position>
    {
        public PositionsController(IPositionService positionService)
            : base(positionService) { }
    }
}
