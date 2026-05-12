using MISA.QLSX.Core.DTOs.Responses;
using MISA.QLSX.Core.Entities;

namespace MISA.QLSX.Core.Interfaces.Repository
{
    /// <summary>
    /// Repository xu ly metadata va lien ket tep.
    /// </summary>
    public interface IFileRepository : IBaseRepository<FileResource>
    {
        /// <summary>
        /// Them lien ket tep vao ban ghi nghiep vu.
        /// </summary>
        /// <param name="reference">Thong tin lien ket tep.</param>
        /// <returns>ID lien ket vua tao.</returns>
        Task<Guid> InsertReferenceAsync(FileReference reference);

        /// <summary>
        /// Lay danh sach tep theo ban ghi nghiep vu.
        /// </summary>
        /// <param name="moduleName">Ma module nghiep vu.</param>
        /// <param name="entityName">Ten thuc the nghiep vu.</param>
        /// <param name="entityId">ID ban ghi nghiep vu.</param>
        /// <returns>Danh sach tep da gan vao ban ghi.</returns>
        Task<List<FileItemResponse>> GetByEntityAsync(string moduleName, string entityName, Guid entityId);

        /// <summary>
        /// Xoa tat ca lien ket theo file ID.
        /// </summary>
        /// <param name="fileId">ID tep can xoa lien ket.</param>
        /// <returns>So lien ket bi xoa.</returns>
        Task<int> DeleteReferencesByFileIdAsync(Guid fileId);
    }
}
