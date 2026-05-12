using MISA.QLSX.Core.DTOs.Requests;
using MISA.QLSX.Core.DTOs.Responses;
using MISA.QLSX.Core.Entities;

namespace MISA.QLSX.Core.Interfaces.Service
{
    /// <summary>
    /// Service xu ly upload va luu tru tep local dung chung.
    /// </summary>
    public interface IFileService
    {
        /// <summary>
        /// Upload tep vao local storage, luu metadata va gan vao entity nghiep vu.
        /// </summary>
        /// <param name="fileStream">Stream du lieu tep.</param>
        /// <param name="originalName">Ten tep ban dau.</param>
        /// <param name="mimeType">Kieu MIME cua tep.</param>
        /// <param name="sizeBytes">Kich thuoc tep theo byte.</param>
        /// <param name="request">Metadata luong nghiep vu can gan tep.</param>
        /// <param name="createdBy">Nguoi tao thao tac upload.</param>
        /// <returns>Thong tin tep sau khi upload thanh cong.</returns>
        Task<FileUploadResponse> UploadAsync(
            Stream fileStream,
            string originalName,
            string? mimeType,
            long sizeBytes,
            FileUploadRequest request,
            Guid? createdBy
        );

        /// <summary>
        /// Lay danh sach tep theo entity nghiep vu.
        /// </summary>
        /// <param name="moduleName">Ma module nghiep vu.</param>
        /// <param name="entityName">Ten thuc the nghiep vu.</param>
        /// <param name="entityId">ID ban ghi nghiep vu.</param>
        /// <returns>Danh sach tep da gan.</returns>
        Task<List<FileItemResponse>> GetByEntityAsync(string moduleName, string entityName, Guid entityId);

        /// <summary>
        /// Lay thong tin tep theo ID de phuc vu tai xuong.
        /// </summary>
        /// <param name="fileId">ID tep can lay.</param>
        /// <returns>Thong tin metadata tep.</returns>
        Task<FileResource> GetFileByIdAsync(Guid fileId);

        /// <summary>
        /// Lay duong dan vat ly cua tep tren local storage.
        /// </summary>
        /// <param name="resource">Metadata tep.</param>
        /// <returns>Duong dan tuyet doi cua tep.</returns>
        string GetPhysicalPath(FileResource resource);

        /// <summary>
        /// Xoa tep khoi he thong gom lien ket DB, metadata va file vat ly.
        /// </summary>
        /// <param name="fileId">ID tep can xoa.</param>
        /// <returns>ID tep da xoa.</returns>
        Task<Guid> DeleteAsync(Guid fileId);
    }
}
