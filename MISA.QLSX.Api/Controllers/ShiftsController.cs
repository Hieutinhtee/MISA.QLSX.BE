using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MISA.QLSX.Core.Entities;
using MISA.QLSX.Core.Exceptions;
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

        #region Method

        /// <summary>
        /// Thực hiện xóa mềm (soft delete) hàng loạt các bản ghi Khách hàng
        /// <para/>Sử dụng phương thức HTTP PUT để cập nhật trạng thái xóa mềm thay vì DELETE
        /// </summary>
        /// <param name="ids">Danh sách ID (Guid) của các bản ghi Khách hàng cần xóa mềm</param>
        /// <returns>Kết quả HTTP 200 OK kèm theo số lượng bản ghi đã bị ảnh hưởng</returns>
        /// Created by TMHieu - 7/12/2025
        [HttpPut("batch-active")]
        public async Task<IActionResult> UpdateIsActiveMany(
            [FromBody] List<Guid> ids,
            bool isActive
        )
        {
            if (ids == null || ids.Count == 0)
            {
                // Ném lỗi ValidateException để Middleware bắt, thay vì trả về BadRequest trực tiếp
                throw new ValidateException(
                    "Danh sách Id trống.",
                    "Danh sách ID không được để trống."
                );
            }

            int affected = await _shiftService.UpdateIsActiveMany(ids, isActive);

            return Ok(new { TotalAffected = affected });
        }

        [HttpGet("export")]
        public async Task<IActionResult> ExportShift()
        {
            var file = await _shiftService.ExportShiftExcelAsync();

            return File(
                file,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Shift.xlsx"
            );
        }

        #endregion Method
    }
}
