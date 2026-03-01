using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MISA.QLSX.Core.DTOs.Responses;

namespace MISA.QLSX.Core.Interfaces.Service
{
    /// <summary>
    /// Base service interface cho CRUD cơ bản
    /// </summary>
    /// Created by TMHieu - 28/2/2026
    /// <typeparam name="T">Thực thể cần truyền vào VD: Customer</typeparam>
    public interface IBaseService<T>
        where T : class
    {
        /// <summary>
        /// Hàm lấy danh sách tất cả dữ liệu
        /// </summary>
        /// <returns>trả về danh sách tất cả dữ liệu</returns>
        /// Created by TMHieu - 28/2/2026
        Task<List<T>> GetAllAsync();

        /// <summary>
        /// Hàm lấy thông tin chi tiết theo id
        /// </summary>
        /// <param name="id"> id của đối tượng muốn tìm </param>
        /// <returns>đối tượng có id phù hợp</returns>
        /// Created by TMHieu - 28/2/2026
        Task<T?> GetByIdAsync(Guid id);

        /// <summary>
        /// Hàm thêm mới bản ghi trong database
        /// </summary>
        /// <param name="entity">thuộc tính của thực thể muốn thêm</param>
        /// <returns>Id của bản ghi mới</returns>
        /// Created by TMHieu - 28/2/2026
        Task<Guid> CreateAsync(T entity);

        /// <summary>
        /// Hàm cập nhật 1 bản ghi theo id
        /// </summary>
        /// <param name="id"> id của bản ghi mình muốn cập nhật</param>
        /// <param name="entity">thuộc tính của thực thể</param>
        /// <returns>Id của bản ghi vừa cập nhật</returns>
        /// Created by TMHieu - 28/2/2026
        Task<Guid> UpdateAsync(Guid id, T entity);

        /// <summary>
        /// Hàm xóa mềm 1 bản ghi
        /// </summary>
        /// <param name="id">Id bản ghi muốn xóa</param>
        /// <returns>Số dòng bị ảnh hưởng</returns>
        /// Created by TMHieu - 28/2/2026
        Task<int> DeleteAsync(Guid id);

        /// <summary>
        /// Hàm cập nhật 1 cột thành các giá trị giống nhau của nhiều bản ghi
        /// </summary>
        /// <param name="ids">Danh sách Id cần cập nhật</param>
        /// <param name="columnName">Tên cột cần cập nhật</param>
        /// <param name="value">Giá trị mới</param>
        /// <returns>Số bản ghi bị ảnh hưởng</returns>
        /// Created By: TMHieu 28/2/2026
        Task<int> BulkUpdateSameValueAsync(List<Guid> ids, string columnName, object value);

        /// <summary>
        /// Kiểm tra giá trị có tồn tại trong cột (bỏ qua soft delete và ignoreId nếu có)
        /// </summary>
        /// <param name="columnName">Tên cột cần kiểm tra</param>
        /// <param name="value">Giá trị cần kiểm tra</param>
        /// <param name="ignoreId">ID cần bỏ qua (tùy chọn)</param>
        /// <returns>True nếu tồn tại, False nếu không</returns>
        /// Created By: TMHieu 28/2/2026
        Task<bool> IsValueExistAsync(string columnName, object value, Guid? ignoreId = null);

        /// <summary>
        /// Lấy danh sách entity có phân trang, tìm kiếm và sắp xếp
        /// </summary>
        /// <param name="page">Trang thứ mấy</param>
        /// <param name="pageSize">Số bản ghi một trang</param>
        /// <param name="search">Từ khóa tìm kiếm</param>
        /// <param name="sortBy">Cột cần sắp xếp</param>
        /// <param name="sortOrder">Hướng sắp xếp (ASC/DESC)</param>
        /// <returns>Đối tượng PagingResponse chứa dữ liệu và metadata</returns>
        /// Created By: TMHieu 28/2/2026
        Task<PagingResponse<T>> QueryPagingAsync(
            int? page,
            int? pageSize,
            string? search,
            string? sortBy,
            string? sortOrder,
            string? type = null
        );

        /// <summary>
        /// Hàm truy vấn toàn bộ dữ liệu theo điều kiện tìm kiếm và loại bản ghi,
        /// không áp dụng phân trang và không áp dụng sắp xếp.
        /// Created By: TMHieu 28/2/2026
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
        Task<PagingResponse<T>> QueryAllExcludeAsync(
            string? search,
            string? type,
            List<Guid>? excludeIds
        );
    }
}
