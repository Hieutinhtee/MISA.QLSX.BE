using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MISA.QLSX.Core.DTOs.Requests;
using MISA.QLSX.Core.DTOs.Responses;
using MISA.QLSX.Core.Exceptions;
using MISA.QLSX.Core.Interfaces.Repository;
using MISA.QLSX.Core.Interfaces.Service;

namespace MISA.QLSX.Core.Services
{
    /// <summary>
    /// Lớp cơ sở cung cấp các dịch vụ chung cho các thực thể T.
    /// Created by: TMHieu (07/12/2025)
    /// </summary>
    /// <typeparam name="T">Loại thực thể mà dịch vụ xử lý.</typeparam>
    public class BaseServices<T> : IBaseService<T>
        where T : class
    {
        protected readonly IBaseRepository<T> _repo;

        /// <summary>
        /// Hàm khởi tạo cho lớp BaseServices.
        /// Created by: TMHieu (07/12/2025)
        /// </summary>
        /// <param name="repo">Kho lưu trữ cơ bản cho thực thể T.</param>
        protected BaseServices(IBaseRepository<T> repo)
        {
            _repo = repo;
        }

        /// <summary>
        /// Kiểm tra giá trị có trùng lặp theo cột chỉ định.
        /// Created by: TMHieu (07/12/2025)
        /// </summary>
        /// <param name="propertyName">Truyền vào kiểu giống nameof(Customer.Phone) hàm sẽ tự map với tên cột</param>
        /// <param name="value">Giá trị cần kiểm tra.</param>
        /// <param name="ignoreId">ID cần bỏ qua (khi update để tránh trùng chính nó).</param>
        /// <returns>Task hoàn thành sau khi kiểm tra.</returns>
        public async Task<bool> IsValueExistAsync(
            string propertyName,
            object value,
            Guid? ignoreId = null
        )
        {
            var exists = await _repo.IsValueExistAsync(propertyName, value, ignoreId);
            if (exists)
                throw new ValidateException(
                    $"Giá trị '{value}' ở cột {propertyName} đã tồn tại",
                    $"Dữ liệu nhập đã tồn tại"
                );
            return exists;
        }

        /// <summary>
        /// Đảm bảo thực thể tồn tại theo ID.
        /// Created by: TMHieu (07/12/2025)
        /// </summary>
        /// <param name="id">ID của thực thể cần kiểm tra.</param>
        /// <returns>Thực thể nếu tồn tại, nếu không ném ngoại lệ.</returns>
        protected async Task<T?> EnsureExistsAsync(Guid id)
        {
            var data = await _repo.GetById(id);
            if (data == null)
                throw new NotFoundException(
                    "Không tìm thấy dữ liệu",
                    $"{typeof(T).Name} không tồn tại"
                );
            return data;
        }

        /// <summary>
        /// Validate tùy chỉnh cho thực thể (có thể override ở lớp con).
        /// Created by: TMHieu (07/12/2025)
        /// </summary>
        /// <param name="entity">Thực thể cần validate.</param>
        /// <param name="id">ID tùy chọn để validate (ví dụ khi update).</param>
        /// <returns>Task hoàn thành sau validate.</returns>
        protected virtual Task ValidateAsync(T entity, Guid? id = null)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Tùy chỉnh cho thực thể (có thể override ở lớp con) trước khi save
        /// tính toán hoặc xử lý thêm gì đó
        /// Created by: TMHieu (07/12/2025)
        /// </summary>
        /// <param name="entity">Thực thể cần xử lí.</param>
        /// <param name="isUpdate">Xử lí cho update hay không, false là insert</param>
        /// <returns>Task hoàn thành sau xử lí.</returns>
        protected virtual Task BeforeSaveAsync(T entity, bool isUpdate = false)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Lấy tất cả thực thể.
        /// Created by: TMHieu (07/12/2025)
        /// </summary>
        /// <returns>Danh sách tất cả thực thể T.</returns>
        public virtual Task<List<T>> GetAllAsync() => _repo.GetAllAsync();

        /// <summary>
        /// Lấy thực thể theo ID.
        /// Created by: TMHieu (07/12/2025)
        /// </summary>
        /// <param name="id">ID của thực thể cần lấy.</param>
        /// <returns>Thực thể T theo ID.</returns>
        public virtual Task<T?> GetByIdAsync(Guid id) => EnsureExistsAsync(id);

        /// <summary>
        /// Tạo mới thực thể.
        /// Created by: TMHieu (07/12/2025)
        /// </summary>
        /// <param name="entity">Thực thể cần tạo.</param>
        /// <returns>ID của thực thể mới tạo.</returns>
        public virtual async Task<Guid> CreateAsync(T entity)
        {
            await ValidateAsync(entity, null);
            await BeforeSaveAsync(entity, false);

            // Lấy tất cả property của entity
            var properties = typeof(T).GetProperties();
            foreach (var prop in properties)
            {
                var propType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                if (propType == typeof(Guid))
                {
                    prop.SetValue(entity, Guid.NewGuid());
                }
            }
            return await _repo.InsertAsync(entity);
        }

        /// <summary>
        /// Cập nhật thực thể theo ID.
        /// Created by: TMHieu (07/12/2025)
        /// </summary>
        /// <param name="id">ID của thực thể cần cập nhật.</param>
        /// <param name="entity">Thực thể với dữ liệu cập nhật.</param>
        /// <returns>ID của thực thể đã cập nhật.</returns>
        public virtual async Task<Guid> UpdateAsync(Guid id, T entity)
        {
            await EnsureExistsAsync(id);
            await ValidateAsync(entity, id);
            await BeforeSaveAsync(entity, true);
            return await _repo.UpdateAsync(id, entity);
        }

        /// <summary>
        /// Xóa thực thể theo ID.
        /// Created by: TMHieu (07/12/2025)
        /// </summary>
        /// <param name="id">ID của thực thể cần xóa.</param>
        /// <returns>Số lượng bản ghi bị ảnh hưởng (thường là 1).</returns>
        public virtual async Task<int> DeleteAsync(Guid id)
        {
            await EnsureExistsAsync(id);
            return await _repo.DeleteAsync(id);
        }

        /// <summary>
        /// Cập nhật 1 cột thành các giá trị giống nhau của nhiều bản ghi
        /// Created By: TMHieu (07/12/2025)
        /// </summary>
        public virtual Task<int> BulkUpdateSameValueAsync(
            List<Guid> ids,
            string columnName,
            object value
        )
        {
            return _repo.BulkUpdateSameValueAsync(ids, columnName, value);
        }

        /// <summary>
        /// Lấy danh sách entity có phân trang, tìm kiếm và sắp xếp
        /// Created By: TMHieu (07/12/2025)
        /// </summary>
        public virtual Task<PagingResponse<T>> QueryPagingAsync(QueryRequest request)
        {
            return _repo.QueryPagingAsync(request);
        }

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
        public virtual Task<PagingResponse<T>> QueryAllExcludeAsync(
            string? search,
            string? type,
            List<Guid>? excludeIds
        )
        {
            return _repo.QueryAllExcludeAsync(search, type, excludeIds);
        }

        /// <summary>
        /// Thực hiện xóa  hàng loạt bản ghi
        /// </summary>
        /// <param name="ids">Danh sách ID (Guid) bản ghi cần xóa mềm</param>
        /// <returns>Tổng số bản ghi đã bị ảnh hưởng</returns>
        /// Created by TMHieu - 8/12/2025
        public async Task<int> DeleteManyAsync(List<Guid> ids)
        {
            // Kiểm tra input rỗng
            if (ids == null || ids.Count == 0)
                return 0;

            // Có thể check id trùng
            ids = ids.Distinct().ToList();

            // Gọi repository xử lý soft delete
            int affected = await _repo.DeleteManyAsync(ids);

            return affected;
        }
    }
}
