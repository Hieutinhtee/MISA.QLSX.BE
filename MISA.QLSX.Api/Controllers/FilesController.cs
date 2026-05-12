using Microsoft.AspNetCore.Mvc;
using MISA.QLSX.Api.Authorization;
using MISA.QLSX.Core.DTOs.Requests;
using MISA.QLSX.Core.Interfaces.Service;

namespace MISA.QLSX.Api.Controllers
{
    /// <summary>
    /// API upload va luu tru tep local dung chung cho nhieu luong nghiep vu.
    /// </summary>
    [Route("api/v1/[controller]")]
    [ApiController]
    [RequireRole("ADMIN", "HR", "MANAGER", "EMPLOYEE")]
    public class FilesController : ControllerBase
    {
        private readonly IFileService _fileService;

        /// <summary>
        /// Khoi tao controller tep.
        /// </summary>
        /// <param name="fileService">Service xu ly upload, download va xoa tep.</param>
        public FilesController(IFileService fileService)
        {
            _fileService = fileService;
        }

        /// <summary>
        /// Upload tep va gan vao ban ghi nghiep vu.
        /// </summary>
        /// <param name="request">Thong tin form upload gom tep va metadata nghiep vu.</param>
        /// <returns>Thong tin tep sau khi upload thanh cong.</returns>
        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromForm] FileUploadFormRequest request)
        {
            if (request?.File == null)
            {
                return BadRequest(new { message = "Khong tim thay tep trong request" });
            }

            var uploadRequest = new FileUploadRequest
            {
                ModuleName = request.ModuleName,
                EntityName = request.EntityName,
                EntityId = request.EntityId,
                Purpose = request.Purpose,
            };

            var createdBy = GetCurrentEmployeeId();
            await using var stream = request.File.OpenReadStream();
            var result = await _fileService.UploadAsync(
                stream,
                request.File.FileName,
                request.File.ContentType,
                request.File.Length,
                uploadRequest,
                createdBy
            );

            return StatusCode(201, new { data = result });
        }

        /// <summary>
        /// Lay danh sach tep da gan theo entity nghiep vu.
        /// </summary>
        /// <param name="moduleName">Ma module nghiep vu.</param>
        /// <param name="entityName">Ten thuc the nghiep vu.</param>
        /// <param name="entityId">ID ban ghi nghiep vu can lay tep.</param>
        /// <returns>Danh sach tep da gan vao entity.</returns>
        [HttpGet("by-entity")]
        public async Task<IActionResult> GetByEntity(
            [FromQuery] string moduleName,
            [FromQuery] string entityName,
            [FromQuery] Guid entityId
        )
        {
            var data = await _fileService.GetByEntityAsync(moduleName, entityName, entityId);
            return Ok(new { data });
        }

        /// <summary>
        /// Tai tep theo file ID.
        /// </summary>
        /// <param name="fileId">ID tep can tai.</param>
        /// <returns>File stream tai xuong.</returns>
        [HttpGet("{fileId}/download")]
        public async Task<IActionResult> Download(Guid fileId)
        {
            var resource = await _fileService.GetFileByIdAsync(fileId);
            var physicalPath = _fileService.GetPhysicalPath(resource);

            if (!System.IO.File.Exists(physicalPath))
            {
                return NotFound(new { message = "Khong tim thay tep vat ly tren local storage" });
            }

            var mimeType = string.IsNullOrWhiteSpace(resource.MimeType)
                ? "application/octet-stream"
                : resource.MimeType;

            var downloadName = string.IsNullOrWhiteSpace(resource.OriginalName)
                ? resource.StoredName
                : resource.OriginalName;

            return PhysicalFile(physicalPath, mimeType, downloadName);
        }

        /// <summary>
        /// Xoa tep khoi he thong.
        /// </summary>
        /// <param name="fileId">ID tep can xoa.</param>
        /// <returns>ID tep da xoa.</returns>
        [HttpDelete("{fileId}")]
        public async Task<IActionResult> Delete(Guid fileId)
        {
            var deletedId = await _fileService.DeleteAsync(fileId);
            return Ok(new { data = deletedId, message = "Deleted successfully" });
        }

        /// <summary>
        /// Lay ID nhan vien hien tai tu session.
        /// </summary>
        /// <returns>Employee ID neu co; nguoc lai tra ve null.</returns>
        private Guid? GetCurrentEmployeeId()
        {
            var rawValue = HttpContext.Session.GetString("employee_id");
            return Guid.TryParse(rawValue, out var employeeId) ? employeeId : null;
        }

        /// <summary>
        /// Request form cho endpoint upload tep.
        /// </summary>
        public class FileUploadFormRequest
        {
            /// <summary>
            /// Tep can upload.
            /// </summary>
            public IFormFile? File { get; set; }

            /// <summary>
            /// Ma module nghiep vu su dung tep.
            /// </summary>
            public string? ModuleName { get; set; }

            /// <summary>
            /// Ten thuc the nghiep vu.
            /// </summary>
            public string? EntityName { get; set; }

            /// <summary>
            /// ID ban ghi nghiep vu can gan tep.
            /// </summary>
            public Guid? EntityId { get; set; }

            /// <summary>
            /// Muc dich tep.
            /// </summary>
            public string? Purpose { get; set; }
        }
    }
}
