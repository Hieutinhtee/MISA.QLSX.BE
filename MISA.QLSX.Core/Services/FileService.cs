using MISA.QLSX.Core.DTOs.Requests;
using MISA.QLSX.Core.DTOs.Responses;
using MISA.QLSX.Core.Entities;
using MISA.QLSX.Core.Exceptions;
using MISA.QLSX.Core.Interfaces.Repository;
using MISA.QLSX.Core.Interfaces.Service;

namespace MISA.QLSX.Core.Services
{
    /// <summary>
    /// Service xu ly upload, doc va xoa tep local dung chung cho nhieu luong.
    /// </summary>
    public class FileService : IFileService
    {
        private readonly IFileRepository _fileRepository;
        private readonly string _rootPath;
        private readonly long _maxFileSizeBytes;
        private readonly HashSet<string> _allowedExtensions;

        /// <summary>
        /// Khoi tao service tep local voi cac cau hinh mac dinh.
        /// </summary>
        /// <param name="fileRepository">Repository metadata va lien ket tep.</param>
        public FileService(IFileRepository fileRepository)
        {
            _fileRepository = fileRepository;

            var customRoot = Environment.GetEnvironmentVariable("MISA_FILE_UPLOAD_ROOT");
            _rootPath = string.IsNullOrWhiteSpace(customRoot)
                ? Path.Combine(AppContext.BaseDirectory, "uploads")
                : Path.GetFullPath(customRoot);

            var maxMb = 10L;
            var maxMbEnv = Environment.GetEnvironmentVariable("MISA_FILE_MAX_MB");
            if (!string.IsNullOrWhiteSpace(maxMbEnv) && long.TryParse(maxMbEnv, out var parsedMb) && parsedMb > 0)
            {
                maxMb = parsedMb;
            }

            _maxFileSizeBytes = maxMb * 1024 * 1024;

            var extEnv = Environment.GetEnvironmentVariable("MISA_FILE_ALLOWED_EXT");
            var extDefaults = new[] { ".jpg", ".jpeg", ".png", ".pdf", ".doc", ".docx", ".xlsx" };
            _allowedExtensions = (string.IsNullOrWhiteSpace(extEnv) ? extDefaults : extEnv.Split(','))
                .Select(x => x.Trim().ToLowerInvariant())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToHashSet();
        }

        /// <summary>
        /// Upload tep vao local storage, luu metadata va gan vao entity nghiep vu.
        /// </summary>
        /// <param name="fileStream">Stream du lieu tep.</param>
        /// <param name="originalName">Ten tep goc tu nguoi dung.</param>
        /// <param name="mimeType">Kieu MIME cua tep.</param>
        /// <param name="sizeBytes">Kich thuoc tep theo byte.</param>
        /// <param name="request">Metadata luong nghiep vu can gan tep.</param>
        /// <param name="createdBy">Nguoi thuc hien upload.</param>
        /// <returns>Ket qua upload thanh cong.</returns>
        public async Task<FileUploadResponse> UploadAsync(
            Stream fileStream,
            string originalName,
            string? mimeType,
            long sizeBytes,
            FileUploadRequest request,
            Guid? createdBy
        )
        {
            ValidateUploadInput(fileStream, originalName, sizeBytes, request);

            var fileId = Guid.NewGuid();
            var extension = Path.GetExtension(originalName)?.ToLowerInvariant() ?? string.Empty;
            var safeName = SanitizeFileName(Path.GetFileNameWithoutExtension(originalName));
            var storedName = $"{fileId}_{safeName}{extension}";
            var relativePath = BuildRelativePath(request.ModuleName!, storedName);
            var absolutePath = Path.Combine(_rootPath, relativePath.Replace('/', Path.DirectorySeparatorChar));
            var directoryPath = Path.GetDirectoryName(absolutePath);

            if (!string.IsNullOrWhiteSpace(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            await using var output = new FileStream(absolutePath, FileMode.Create, FileAccess.Write, FileShare.None);
            await fileStream.CopyToAsync(output);

            var now = DateTime.Now;
            var resource = new FileResource
            {
                FileId = fileId,
                OriginalName = originalName,
                StoredName = storedName,
                RelativePath = relativePath,
                MimeType = string.IsNullOrWhiteSpace(mimeType) ? "application/octet-stream" : mimeType,
                SizeBytes = sizeBytes,
                IsDeleted = Guid.Empty,
                CreatedBy = createdBy,
                CreatedAt = now,
                UpdatedBy = createdBy,
                UpdatedAt = now,
            };

            try
            {
                await _fileRepository.InsertAsync(resource);

                var reference = new FileReference
                {
                    FileReferenceId = Guid.NewGuid(),
                    FileId = fileId,
                    ModuleName = request.ModuleName,
                    EntityName = request.EntityName,
                    EntityId = request.EntityId,
                    Purpose = request.Purpose,
                    CreatedBy = createdBy,
                    CreatedAt = now,
                };

                await _fileRepository.InsertReferenceAsync(reference);
            }
            catch
            {
                if (System.IO.File.Exists(absolutePath))
                {
                    System.IO.File.Delete(absolutePath);
                }

                throw;
            }

            return new FileUploadResponse
            {
                FileId = fileId,
                OriginalName = originalName,
                MimeType = resource.MimeType,
                SizeBytes = sizeBytes,
                ModuleName = request.ModuleName,
                EntityName = request.EntityName,
                EntityId = request.EntityId,
                Purpose = request.Purpose,
            };
        }

        /// <summary>
        /// Lay danh sach tep theo entity nghiep vu.
        /// </summary>
        /// <param name="moduleName">Ma module nghiep vu.</param>
        /// <param name="entityName">Ten thuc the nghiep vu.</param>
        /// <param name="entityId">ID ban ghi nghiep vu.</param>
        /// <returns>Danh sach tep da gan vao entity.</returns>
        public Task<List<FileItemResponse>> GetByEntityAsync(string moduleName, string entityName, Guid entityId)
        {
            if (string.IsNullOrWhiteSpace(moduleName))
            {
                throw new ValidateException("ModuleName is required", "ModuleName khong duoc de trong");
            }

            if (string.IsNullOrWhiteSpace(entityName))
            {
                throw new ValidateException("EntityName is required", "EntityName khong duoc de trong");
            }

            return _fileRepository.GetByEntityAsync(moduleName.Trim().ToLowerInvariant(), entityName.Trim().ToLowerInvariant(), entityId);
        }

        /// <summary>
        /// Lay metadata tep theo ID de phuc vu tai xuong.
        /// </summary>
        /// <param name="fileId">ID tep can lay.</param>
        /// <returns>Metadata tep ton tai.</returns>
        public async Task<FileResource> GetFileByIdAsync(Guid fileId)
        {
            var resource = await _fileRepository.GetById(fileId);
            if (resource == null)
            {
                throw new NotFoundException("Khong tim thay tep", "Tep khong ton tai hoac da bi xoa");
            }

            return resource;
        }

        /// <summary>
        /// Tra ve duong dan vat ly cua tep tren local storage.
        /// </summary>
        /// <param name="resource">Metadata tep can tinh duong dan.</param>
        /// <returns>Duong dan tuyet doi cua tep local.</returns>
        public string GetPhysicalPath(FileResource resource)
        {
            if (string.IsNullOrWhiteSpace(resource.RelativePath))
            {
                throw new ValidateException("RelativePath is invalid", "Thong tin duong dan tep khong hop le");
            }

            return Path.Combine(_rootPath, resource.RelativePath.Replace('/', Path.DirectorySeparatorChar));
        }

        /// <summary>
        /// Xoa tep khoi he thong gom lien ket DB, metadata va file vat ly.
        /// </summary>
        /// <param name="fileId">ID tep can xoa.</param>
        /// <returns>ID tep da xoa.</returns>
        public async Task<Guid> DeleteAsync(Guid fileId)
        {
            var resource = await GetFileByIdAsync(fileId);
            var physicalPath = GetPhysicalPath(resource);

            await _fileRepository.DeleteReferencesByFileIdAsync(fileId);
            await _fileRepository.DeleteAsync(fileId);

            if (System.IO.File.Exists(physicalPath))
            {
                System.IO.File.Delete(physicalPath);
            }

            return fileId;
        }

        /// <summary>
        /// Kiem tra hop le du lieu upload truoc khi luu file.
        /// </summary>
        /// <param name="fileStream">Stream du lieu tep.</param>
        /// <param name="originalName">Ten tep goc.</param>
        /// <param name="sizeBytes">Kich thuoc tep theo byte.</param>
        /// <param name="request">Metadata nghiep vu de gan tep.</param>
        /// <returns>Khong tra ve gia tri.</returns>
        private void ValidateUploadInput(
            Stream fileStream,
            string originalName,
            long sizeBytes,
            FileUploadRequest request
        )
        {
            if (fileStream == null)
            {
                throw new ValidateException("File stream is null", "Noi dung tep khong hop le");
            }

            if (sizeBytes <= 0)
            {
                throw new ValidateException("File is empty", "Tep tai len khong duoc rong");
            }

            if (sizeBytes > _maxFileSizeBytes)
            {
                var maxMb = _maxFileSizeBytes / (1024 * 1024);
                throw new ValidateException("File size exceeds limit", $"Kich thuoc tep vuot qua gioi han {maxMb}MB");
            }

            if (string.IsNullOrWhiteSpace(originalName))
            {
                throw new ValidateException("File name is required", "Ten tep khong duoc de trong");
            }

            var extension = Path.GetExtension(originalName)?.ToLowerInvariant() ?? string.Empty;
            if (!_allowedExtensions.Contains(extension))
            {
                throw new ValidateException("File extension is not allowed", "Dinh dang tep khong duoc ho tro");
            }

            if (request == null)
            {
                throw new ValidateException("Request is null", "Thong tin metadata tep khong hop le");
            }

            if (string.IsNullOrWhiteSpace(request.ModuleName))
            {
                throw new ValidateException("ModuleName is required", "ModuleName khong duoc de trong");
            }

            if (string.IsNullOrWhiteSpace(request.EntityName))
            {
                throw new ValidateException("EntityName is required", "EntityName khong duoc de trong");
            }

            if (!request.EntityId.HasValue || request.EntityId.Value == Guid.Empty)
            {
                throw new ValidateException("EntityId is required", "EntityId khong duoc de trong");
            }

            request.ModuleName = request.ModuleName.Trim().ToLowerInvariant();
            request.EntityName = request.EntityName.Trim().ToLowerInvariant();
            request.Purpose = request.Purpose?.Trim().ToLowerInvariant();
        }

        /// <summary>
        /// Tao duong dan tuong doi luu tep theo module va thoi gian.
        /// </summary>
        /// <param name="moduleName">Module nghiep vu su dung tep.</param>
        /// <param name="storedName">Ten tep vat ly da chuan hoa.</param>
        /// <returns>Duong dan tuong doi theo convention local storage.</returns>
        private static string BuildRelativePath(string moduleName, string storedName)
        {
            var now = DateTime.Now;
            return $"{moduleName}/{now:yyyy}/{now:MM}/{storedName}";
        }

        /// <summary>
        /// Chuan hoa ten tep de tranh ky tu nguy hiem tren he thong file.
        /// </summary>
        /// <param name="name">Ten tep can chuan hoa.</param>
        /// <returns>Ten tep an toan cho local storage.</returns>
        private static string SanitizeFileName(string name)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitizedChars = name
                .Select(ch => invalidChars.Contains(ch) ? '_' : ch)
                .ToArray();

            var sanitized = new string(sanitizedChars).Trim();
            return string.IsNullOrWhiteSpace(sanitized) ? "file" : sanitized;
        }
    }
}
