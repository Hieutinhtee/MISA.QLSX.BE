using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MISA.QLSX.Core.DTOs.Requests;
using MISA.QLSX.Core.DTOs.Responses;
using MISA.QLSX.Core.Exceptions;
using MISA.QLSX.Core.Interfaces.Service;

namespace MISA.QLSX.Api.Controllers
{
    /// <summary>
    /// Base Controller (Generic) cung cấp các API cơ bản cho một thực thể (Entity)
    /// <para/>Các Controller cụ thể (như CustomersController) sẽ kế thừa BaseController để sử dụng lại các API CRUD, Paging...
    /// </summary>
    /// <typeparam name="T">Loại Entity (Class) mà Controller này quản lý</typeparam>
    /// Created by TMHieu - 7/12/2025
    [Route("api/v1/[controller]")]
    [ApiController]
    public abstract class BaseController<T> : ControllerBase
        where T : class
    {
        #region Declaration

        /// <summary>
        /// Service xử lý nghiệp vụ cơ sở cho Entity T
        /// </summary>
        protected readonly IBaseService<T> _service;

        #endregion Declaration

        #region Constructor

        /// <summary>
        /// Hàm khởi tạo Base Controller
        /// </summary>
        /// <param name="service">Service xử lý nghiệp vụ được tiêm vào (Dependency Injection)</param>
        /// Created by TMHieu - 7/12/2025
        public BaseController(IBaseService<T> service)
        {
            _service = service;
        }

        #endregion Constructor

        #region Method

        /// <summary>
        /// Lấy tất cả bản ghi của Entity T
        /// </summary>
        /// <returns>Danh sách tất cả các Entity T</returns>
        /// Created by TMHieu - 7/12/2025
        [HttpGet]
        public virtual async Task<IActionResult> GetAll()
        {
            var data = await _service.GetAllAsync();
            return Ok(new { data });
        }

        /// <summary>
        /// Lấy một bản ghi theo ID
        /// </summary>
        /// <param name="id">ID (Guid) của bản ghi cần lấy</param>
        /// <returns>Bản ghi Entity T tương ứng</returns>
        /// Created by TMHieu - 7/12/2025
        [HttpGet("{id}")]
        public virtual async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var data = await _service.GetByIdAsync(id);
                return Ok(new { data });
            }
            catch (NotFoundException ex)
            {
                // Xử lý lỗi không tìm thấy (404)
                return NotFound(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Thêm mới một bản ghi Entity T
        /// </summary>
        /// <param name="entity">Đối tượng Entity T cần thêm mới</param>
        /// <returns>ID (Guid) của bản ghi vừa được tạo</returns>
        /// Created by TMHieu - 7/12/2025
        [HttpPost]
        public virtual async Task<IActionResult> Create([FromBody] T entity)
        {
            var id = await _service.CreateAsync(entity);
            return StatusCode(201, new { id, message = "Created successfully" }); // 201 Created
        }

        /// <summary>
        /// Cập nhật một bản ghi Entity T theo ID
        /// </summary>
        /// <param name="id">ID (Guid) của bản ghi cần cập nhật</param>
        /// <param name="entity">Đối tượng Entity T chứa dữ liệu mới</param>
        /// <returns>Số bản ghi đã được cập nhật</returns>
        /// Created by TMHieu - 7/12/2025
        [HttpPut("{id}")]
        public virtual async Task<IActionResult> Update(Guid id, [FromBody] T entity)
        {
            try
            {
                var rows = await _service.UpdateAsync(id, entity);
                return Ok(new { updated = rows, message = "Updated successfully" });
            }
            catch (NotFoundException ex)
            {
                // Xử lý lỗi không tìm thấy (404)
                return NotFound(new { error = ex.Message });
            }
            catch (ValidateException ex)
            {
                // Xử lý lỗi nghiệp vụ/validate (400)
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Xóa một bản ghi theo ID
        /// </summary>
        /// <param name="id">ID (Guid) của bản ghi cần xóa</param>
        /// <returns>Số bản ghi đã được xóa</returns>
        /// Created by TMHieu - 7/12/2025
        [HttpDelete("{id}")]
        public virtual async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var rows = await _service.DeleteAsync(id);
                return Ok(new { deleted = rows, message = "Deleted successfully" });
            }
            catch (NotFoundException ex)
            {
                // Xử lý lỗi không tìm thấy (404)
                return NotFound(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Lấy dữ liệu phân trang, hỗ trợ tìm kiếm và sắp xếp
        /// </summary>
        /// <param name="page">Số trang hiện tại (mặc định là 1)</param>
        /// <param name="pageSize">Kích thước trang (mặc định là 100)</param>
        /// <param name="search">Chuỗi tìm kiếm</param>
        /// <returns>Đối tượng PagingResponse chứa danh sách dữ liệu và thông tin meta</returns>
        /// Created by TMHieu - 7/12/2025
        [HttpPost("paging")]
        public virtual async Task<PagingResponse<T>> GetPaging([FromBody] QueryRequest request)
        {
            return await _service.QueryPagingAsync(request);
        }

        /// <summary>
        /// Truy vấn toàn bộ dữ liệu theo điều kiện tìm kiếm và loại bản ghi,
        /// không áp dụng phân trang và không áp dụng sắp xếp.
        /// </summary>
        /// <typeparam name="T">
        /// Kiểu entity trả về.
        /// </typeparam>
        /// <param name="search">
        /// Từ khóa tìm kiếm.
        /// Áp dụng tìm kiếm trên các cột được khai báo trong GetSearchFields().
        /// Nếu null hoặc rỗng thì không áp dụng tìm kiếm.
        /// </param>
        /// <param name="type">
        /// Loại bản ghi dùng để lọc dữ liệu.
        /// Nếu null hoặc rỗng thì không áp dụng điều kiện lọc theo type.
        /// </param>
        /// <param name="excludeIds">
        /// Danh sách khóa chính cần loại bỏ khỏi kết quả truy vấn.
        /// Nếu null hoặc rỗng thì không áp dụng điều kiện loại bỏ.
        /// </param>
        /// <returns>
        /// Danh sách dữ liệu thỏa mãn điều kiện tìm kiếm,
        /// đã loại bỏ các bản ghi nằm trong excludeIds.
        /// </returns>
        [HttpPost("query-all")]
        public virtual async Task<PagingResponse<T>> QueryAll(
            [FromQuery] string? search = null,
            [FromQuery] string? type = null,
            [FromBody] List<Guid>? excludeIds = null
        )
        {
            var data = await _service.QueryAllExcludeAsync(search, type, excludeIds);

            return data;
        }

        /// <summary>
        /// Thực hiện xóa 1 hoặc hàng loạt các bản ghi
        /// </summary>
        /// <param name="ids">Danh sách ID (Guid) của các bản ghi Khách hàng cần xóa mềm</param>
        /// <returns>Kết quả HTTP 200 OK kèm theo số lượng bản ghi đã bị ảnh hưởng</returns>
        /// Created by TMHieu - 7/12/2025
        [HttpPost("batch-delete")]
        public async Task<IActionResult> DeleteMany([FromBody] List<Guid> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                // Ném lỗi ValidateException để Middleware bắt, thay vì trả về BadRequest trực tiếp
                throw new ValidateException(
                    "Danh sách Id trống.",
                    "Danh sách bản ghi cẫn xóa không được để trống."
                );
            }

            int affected = await _service.DeleteManyAsync(ids);

            return Ok(new { TotalAffected = affected });
        }

        #endregion Method
    }
}
