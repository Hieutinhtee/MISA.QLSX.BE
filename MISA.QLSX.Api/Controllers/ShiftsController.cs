using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MISA.QLSX.Core.Entities;
using MISA.QLSX.Core.Interfaces.Service;
using MISA.QLSX.Core.Services;

namespace MISA.QLSX.Api.Controllers
{
    public class ShiftsController : BaseController<Shift>
    {
        #region Declaration

        private readonly IShiftService _shiftService;

        #endregion Declaration

        #region Constructor

        /// <summary>
        /// Hàm khởi tạo Controller
        /// </summary>
        /// <param name="shiftService">Service xử lý nghiệp vụ khách hàng được tiêm vào (Dependency Injection)</param>
        /// Created by TMHieu - 7/12/2025
        public ShiftsController(IShiftService shiftService)
            : base(shiftService)
        {
            _shiftService = shiftService;
        }

        #endregion Constructor
    }
}
