using Microsoft.AspNetCore.Mvc;
using MISA.QLSX.Core.Entities;
using MISA.QLSX.Core.Interfaces.Service;

namespace MISA.QLSX.Api.Controllers
{
    /// <summary>
    /// Controller quản lý API danh mục bằng cấp.
    /// </summary>
    public class DegreesController : BaseController<Degree>
    {
        /// <summary>
        /// Khởi tạo controller bằng cấp.
        /// </summary>
        /// <param name="degreeService">Service xử lý nghiệp vụ bằng cấp.</param>
        public DegreesController(IDegreeService degreeService)
            : base(degreeService) { }
    }
}
