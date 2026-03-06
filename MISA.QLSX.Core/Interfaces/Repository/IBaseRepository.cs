using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MISA.QLSX.Core.DTOs.Requests;
using MISA.QLSX.Core.DTOs.Responses;

namespace MISA.QLSX.Core.Interfaces.Repository
{
    /// <summary>
    /// Base repo interface cho CRUD cơ bản
    /// </summary>
    /// Created By: TMHieu (03/12/2025)
    /// <typeparam name="T">Thực thể cần truyền vào VD: Customer</typeparam>
    public interface IBaseRepository<T>
        where T : class
    {
        /// <summary>
        /// Hàm lấy danh sách tất cả dữ liệu
        /// </summary>
        /// <returns>trả về danh sách tất cả dữ liệu</returns>
        /// Created By: TMHieu (03/12/2025)
        Task<List<T>> GetAllAsync();

        /// <summary>
        /// Hàm lấy thông tin chi tiết theo id
        /// </summary>
        /// <param name="id"> id của đối tượng muốn tìm </param>
        /// <returns>đối tượng có id phù hợp</returns>
        /// Created By: TMHieu (03/12/2025)
        Task<T?> GetById(Guid id);

        /// <summary>
        /// Hàm thêm mới bản ghi trong database
        /// </summary>
        /// <param name="entity">thuộc tính của thực thể muốn thêm</param>
        /// <returns>trả về Id của bản ghi mới</returns>
        /// Created By: TMHieu (03/12/2025)
        Task<Guid> InsertAsync(T entity);

        /// <summary>
        /// Hàm cập nhật 1 bản ghi theo id
        /// </summary>
        /// <param name="id"> id của bản ghi mình muốn cập nhật</param>
        /// <param name="entity">thuộc tính của thực thể</param>
        /// <returns>Guid của bản ghi vừa cập nhật</returns>
        /// Created By: TMHieu (03/12/2025)
        Task<Guid> UpdateAsync(Guid id, T entity);

        /// <summary>
        /// Hàm cập nhật 1 cột thành các giá trị giống nhau của nhiều bản ghi theo id
        /// </summary>
        /// <param name="ids"> danh sách id của bản ghi muốn cập nhật</param>
        /// <param name="columnName">Cột cần cập nhật hàng loạt</param>
        /// <param name="value">Giá trị mới cần cập nhật</param>
        /// <returns>Số bản ghi bị ảnh hưởng</returns>
        /// Created By: TMHieu (05/12/2025)
        Task<int> BulkUpdateSameValueAsync(List<Guid> ids, string columnName, object value);

        /// <summary>
        /// Xóa mềm 1 bản ghi trong database
        /// </summary>
        /// <param name="id">id mà mình muốn xóa </param>
        /// <returns>số dòng bị ảnh hưởng </returns>
        /// Created By: TMHieu (03/12/2025)
        Task<int> DeleteAsync(Guid id);

        /// <summary>
        /// Kiểm tra giá trị có tồn tại trong cột (bỏ qua soft delete và ignoreId nếu có).
        /// </summary>
        /// <param name="columnName">Tên cột cần kiểm tra.</param>
        /// <param name="value">Giá trị cần kiểm tra.</param>
        /// <param name="ignoreId">ID cần bỏ qua (tùy chọn).</param>
        /// <returns>True nếu tồn tại, False nếu không.</returns>
        /// Created By: TMHieu (05/12/2025)
        Task<bool> IsValueExistAsync(string columnName, object value, Guid? ignoreId = null);

        /// <summary>
        /// Lấy danh sách entity có phân trang, tìm kiếm và sắp xếp
        /// </summary>
        /// <param name="page">Trang thứ mấy</param>
        /// <param name="pageSize">Số bản ghi một trang.</param>
        /// <param name="search">Từ khóa tìm kiếm</param>
        /// <returns>Đối tượng PagingResponse chứa dữ liệu và metadata</returns>
        /// Created By: TMHieu (07/12/2025)
        Task<PagingResponse<T>> QueryPagingAsync(QueryRequest request);

        /// <summary>
        /// Hàm truy vấn toàn bộ dữ liệu theo điều kiện tìm kiếm và loại bản ghi,
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
        Task<PagingResponse<T>> QueryAllExcludeAsync(
            string? search,
            string? type,
            List<Guid>? excludeIds
        );

        /// <summary>
        /// Thực hiện xóa  hàng loạt bản ghi
        /// </summary>
        /// <param name="ids">Danh sách ID (Guid) bản ghi cần xóa mềm</param>
        /// <returns>Tổng số bản ghi đã bị ảnh hưởng</returns>
        /// Created by TMHieu - 8/12/2025
        Task<int> DeleteManyAsync(List<Guid> ids);
    }
}
