using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Dapper;
using MISA.QLSX.Core.DTOs.Requests;
using MISA.QLSX.Core.DTOs.Responses;
using MISA.QLSX.Core.Interfaces.Repository;
using MISA.QLSX.Infrastructure.Connection;

namespace MISA.QLSX.Infrastructure.Repositories
{
    /// <summary>
    /// Repository base cho tất cả các entity.
    /// dùng Dapper làm micro-ORM.
    /// Tự động lấy tên bảng từ [TableName] hoặc fallback sang snake_case.
    /// Created by TMHieu - 27/2/2026
    /// </summary>
    /// <typeparam name="T">Entity kế thừa từ class, phải có ít nhất 1 property đánh dấu [Key] nếu muốn Insert trả về Id</typeparam>
    public abstract class BaseRepository<T> : IBaseRepository<T>
        where T : class
    {
        // Connection factory (được inject qua DI container)
        protected readonly MySqlConnectionFactory _factory;

        // Tên bảng trong database
        protected readonly string _tableName;

        // Tên cột khóa chính, mặc định là Id
        protected readonly string _idColumn;

        //Tên cột dùng để sắp xếp mặc định
        protected readonly string _defaultSortFiled;

        // ===== Normalize paging =====
        const int DEFAULT_PAGE = 1;
        const int DEFAULT_PAGE_SIZE = 20;
        const int MAX_PAGE_SIZE = 500;

        /// <summary>
        /// Constructor cho BaseRepository.
        /// Khởi tạo factory kết nối và xác định tên bảng/entity.
        /// Created by TMHieu - 27/2/2026
        /// </summary>
        /// <param name="factory">Factory tạo MySqlConnection (bắt buộc, không được null)</param>
        protected BaseRepository(MySqlConnectionFactory factory)
        {
            //  Check factory trước (ưu tiên tránh lỗi runtime)
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));

            var type = typeof(T);

            //  Lấy tên bảng qua attribute TableName
            var tableAttr = type.GetCustomAttribute<TableAttribute>();
            _tableName = tableAttr?.Name ?? ToSnakeCase(type.Name);

            _defaultSortFiled = ResolveDefaultSortField(type, _tableName);

            //  Lấy property Key
            var keyProp = type.GetProperties()
                .FirstOrDefault(p => p.GetCustomAttribute<KeyAttribute>() != null);

            if (keyProp == null)
            {
                throw new Exception($"Entity {type.Name} phải có [Key] attribute trên 1 property.");
            }

            //  Lấy tên cột ID (cột DB)
            var colAttr = keyProp.GetCustomAttribute<ColumnAttribute>();

            // Nếu có attribute [Column("crm_customer_id")] → dùng tên cột
            // Nếu không có → dùng tên property
            _idColumn = colAttr?.Name ?? ToSnakeCase(keyProp.Name);
        }

        private static string ResolveDefaultSortField(Type type, string tableName)
        {
            var props = type.GetProperties();

            bool hasUpdatedAt = props.Any(p =>
                string.Equals(
                    p.GetCustomAttribute<ColumnAttribute>()?.Name ?? ToSnakeCase(p.Name),
                    "updated_at",
                    StringComparison.OrdinalIgnoreCase
                )
            );

            if (hasUpdatedAt)
                return "updated_at";

            return "modified_date";
        }

        /// <summary>
        /// Nguồn dữ liệu dùng cho các truy vấn đọc.
        /// Mặc định là bảng chính, repository con có thể override để đọc từ VIEW.
        /// </summary>
        protected virtual string GetReadTableName() => _tableName;

        /// <summary>
        /// Phương thức helper để chuyển tên class sang snake_case (ví dụ: Customer → customer).
        /// Sử dụng để fallback khi không có [TableName].
        /// Created by TMHieu - 27/2/2026
        /// </summary>
        /// <param name="s">Chuỗi tên class cần chuyển đổi.</param>
        /// <returns>Tên snake_case tương ứng.</returns>
        protected static string ToSnakeCase(string s)
        {
            // Khởi tạo kết quả rỗng
            var result = "";
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                // Thêm _ trước chữ cái hoa (trừ chữ đầu)
                if (i > 0 && char.IsUpper(c))
                {
                    result += "_" + char.ToLower(c);
                }
                else
                {
                    result += char.ToLower(c);
                }
            }
            return result;
        }

        /// <summary>
        /// Property để lấy connection từ factory.
        /// Mỗi lần gọi sẽ tạo mới connection để tránh reuse sai.
        /// Created by TMHieu - 27/2/2026
        /// </summary>
        protected IDbConnection Connection => _factory.CreateConnection();

        /// <summary>
        /// Phương thức abstract để lấy danh sách các trường hợp lệ cho tìm kiếm (whitelist để tránh SQL injection).
        /// Phải override ở repository con.
        /// Created by TMHieu - 27/2/2026
        /// </summary>
        /// <returns>HashSet các tên cột hợp lệ.</returns>
        protected abstract HashSet<string> GetSearchFields();

        /// <summary>
        /// Phương thức abstract để lấy danh sách các trường hợp lệ cho filter (whitelist để tránh SQL injection).
        /// Phải override ở repository con.
        /// Created by TMHieu - 27/2/2026
        /// </summary>
        /// <returns>HashSet các tên cột hợp lệ.</returns>
        /// </summary>
        protected abstract Dictionary<string, FieldMapItem> FieldMap { get; }

        /// <summary>
        /// Lấy tất cả entity.
        /// Created by TMHieu - 27/2/2026
        /// </summary>
        /// <returns>Danh sách entity T.</returns>
        public virtual async Task<List<T>> GetAllAsync()
        {
            // Sử dụng using để tự động đóng connection sau khi dùng xong
            using var conn = Connection;
            // SQL chỉ lấy các record chưa xóa (is_deleted = 0)
            var sql = $"SELECT * FROM {GetReadTableName()} ORDER BY {_defaultSortFiled} DESC";
            var res = await conn.QueryAsync<T>(sql);
            return res.ToList();
        }

        /// <summary>
        /// Insert entity mới .
        /// Sử dụng attribute [ColumnName] để mapping cột.
        /// Created by TMHieu - 27/2/2026
        /// </summary>
        /// <param name="entity">Entity cần insert.</param>
        /// <returns>Id của entity mới (Guid, từ [Key] property).</returns>
        public virtual async Task<Guid> InsertAsync(T entity)
        {
            using var conn = Connection;

            // Lấy danh sách property để mapping
            var properties = typeof(T)
                .GetProperties()
                .Where(p =>
                    p.CanRead && p.CanWrite && p.GetCustomAttribute<NotMappedAttribute>() == null
                )
                .ToList();

            // Tạo chuỗi columns từ attribute Name hoặc tên property (snake_case)
            var columns = string.Join(
                ", ",
                properties.Select(p =>
                    p.GetCustomAttribute<ColumnAttribute>()?.Name ?? ToSnakeCase(p.Name)
                )
            );

            // Tạo chuỗi param names (@PropertyName)
            var paramNames = string.Join(", ", properties.Select(p => "@" + p.Name));

            // Xây dựng SQL INSERT
            var sql = $"INSERT INTO {_tableName} ({columns}) VALUES ({paramNames});";

            // Tạo DynamicParameters từ entity properties
            var parameters = new DynamicParameters();
            foreach (var prop in properties)
            {
                parameters.Add("@" + prop.Name, prop.GetValue(entity));
            }
            // Thực thi INSERT
            await conn.ExecuteAsync(sql, parameters);
            // Lấy Id từ property có [Key]
            var keyProp = typeof(T)
                .GetProperties()
                .FirstOrDefault(p => p.GetCustomAttribute<KeyAttribute>() != null);
            // Trả về Id nếu có Guid.Empty
            return keyProp != null ? (Guid)keyProp.GetValue(entity)! : Guid.Empty;
        }

        /// <summary>
        /// Update entity theo Id .
        /// nếu bảng nào không cho Update thì override lại method này.
        /// Created by TMHieu - 27/2/2026
        /// </summary>
        /// <param name="id">Id của entity cần update.</param>
        /// <param name="entity">Entity với dữ liệu mới.</param>
        /// <returns>Số record bị ảnh hưởng.</returns>
        public virtual async Task<Guid> UpdateAsync(Guid id, T entity)
        {
            using var conn = Connection;

            // Lấy property key
            var keyProp = typeof(T)
                .GetProperties()
                .FirstOrDefault(p => p.GetCustomAttribute<KeyAttribute>() != null);

            if (keyProp == null)
                return Guid.Empty;

            // Lấy danh sách property có [ColumnName] nhưng bỏ cột key, cột ngày tạo, tạo bởi ai
            var properties = typeof(T)
                .GetProperties()
                .Where(p =>
                {
                    if (p.GetCustomAttribute<NotMappedAttribute>() != null)
                        return false;

                    if (p == keyProp)
                        return false;

                    var columnName =
                        p.GetCustomAttribute<ColumnAttribute>()?.Name ?? ToSnakeCase(p.Name);

                    return columnName != "created_by" && columnName != "created_at";
                })
                .ToList();

            // Tạo chuỗi "Col1 = @Prop1, Col2 = @Prop2"
            var setClause = string.Join(
                ", ",
                properties.Select(p =>
                    $"{p.GetCustomAttribute<ColumnAttribute>()?.Name ?? ToSnakeCase(p.Name)} = @{p.Name}"
                )
            );

            // Lấy tên cột khóa chính trong DB
            var keyColumnName = keyProp.GetCustomAttribute<ColumnAttribute>()?.Name ?? keyProp.Name;

            var sql = $"UPDATE {_tableName} SET {setClause} WHERE {keyColumnName} = @Id;";

            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);

            foreach (var prop in properties)
            {
                parameters.Add("@" + prop.Name, prop.GetValue(entity));
            }

            await conn.ExecuteAsync(sql, parameters);

            return id;
        }

        /// <summary>
        /// Hàm cập nhật 1 cột thành các giá trị giống nhau của nhiều bản ghi theo id
        /// </summary>
        /// <param name="ids"> danh sách id của bản ghi mình muốn cập nhật</param>
        /// <param name="columnName">Cột cần cập nhật hàng loạt</param>
        /// param name="value">Giá trị mới cần cập nhật</param>
        /// <returns>Guid của dòng vừa thêm</returns>
        /// Created by TMHieu - 27/2/2026
        public async Task<int> BulkUpdateSameValueAsync(
            List<Guid> ids,
            string columnName,
            object value
        )
        {
            using var conn = Connection;

            var prop = typeof(T)
                .GetProperties()
                .FirstOrDefault(p =>
                    string.Equals(
                        p.GetCustomAttribute<ColumnAttribute>()?.Name ?? ToSnakeCase(p.Name),
                        columnName,
                        StringComparison.OrdinalIgnoreCase
                    ) || string.Equals(p.Name, columnName, StringComparison.OrdinalIgnoreCase)
                );

            if (prop == null)
                throw new Exception(
                    $"Column '{columnName}' không tồn tại trong entity {typeof(T).Name}."
                );

            var dbColumnName =
                prop.GetCustomAttribute<ColumnAttribute>()?.Name ?? ToSnakeCase(prop.Name);

            var idColumn = _idColumn;

            var modifiedByProp = typeof(T)
                .GetProperties()
                .FirstOrDefault(p =>
                    string.Equals(
                        p.GetCustomAttribute<ColumnAttribute>()?.Name ?? ToSnakeCase(p.Name),
                        "updated_by",
                        StringComparison.OrdinalIgnoreCase
                    ) || p.Name.EndsWith("ModifiedBy")
                );

            var modifiedDateProp = typeof(T)
                .GetProperties()
                .FirstOrDefault(p =>
                    string.Equals(
                        p.GetCustomAttribute<ColumnAttribute>()?.Name ?? ToSnakeCase(p.Name),
                        "updated_at",
                        StringComparison.OrdinalIgnoreCase
                    ) || p.Name.EndsWith("ModifiedDate")
                );

            var modifiedByColumn =
                modifiedByProp == null
                    ? null
                    : modifiedByProp.GetCustomAttribute<ColumnAttribute>()?.Name
                        ?? ToSnakeCase(modifiedByProp.Name);

            var modifiedDateColumn =
                modifiedDateProp == null
                    ? null
                    : modifiedDateProp.GetCustomAttribute<ColumnAttribute>()?.Name
                        ?? ToSnakeCase(modifiedDateProp.Name);

            var setParts = new List<string> { $"{dbColumnName} = @Value" };

            if (modifiedByColumn != null)
                setParts.Add($"{modifiedByColumn} = @ModifiedBy");

            if (modifiedDateColumn != null)
                setParts.Add($"{modifiedDateColumn} = @ModifiedDate");

            var setClause = string.Join(", ", setParts);

            var sql =
                $@"
                    UPDATE {_tableName}
                    SET {setClause}
                    WHERE {idColumn} IN @Ids";

            var parameters = new DynamicParameters();
            parameters.Add("@Value", value);
            parameters.Add("@Ids", ids);
            parameters.Add("@ModifiedBy", "b8373a59-3be2-11f1-97ac-d0c5d346d1a4");
            parameters.Add("@ModifiedDate", DateTime.UtcNow);

            return await conn.ExecuteAsync(sql, parameters);
        }

        /// <summary>
        /// Xóa mềm entity theo Id (async, set is_deleted = 1).
        /// Created by TMHieu - 27/2/2026
        /// </summary>
        /// <param name="id">Id của entity cần xóa mềm.</param>
        /// <returns>Số record bị ảnh hưởng (thường là 1 hoặc 0).</returns>
        public virtual async Task<int> DeleteAsync(Guid id)
        {
            using var conn = Connection;
            var sql = $"DELETE FROM {_tableName} WHERE {_idColumn} = @Id";
            return await conn.ExecuteAsync(sql, new { Id = id });
        }

        /// <summary>
        /// Thực hiện xóa  hàng loạt bản ghi
        /// </summary>
        /// <param name="ids">Danh sách ID (Guid) bản ghi cần xóa mềm</param>
        /// <returns>Tổng số bản ghi đã bị ảnh hưởng</returns>
        /// Created by TMHieu - 8/12/2025
        public async Task<int> DeleteManyAsync(List<Guid> ids)
        {
            if (ids == null || ids.Count == 0)
                return 0;

            using var conn = Connection;

            var sql =
                $@"DELETE FROM `{_tableName}`
                         WHERE `{_idColumn}` IN @Ids;
            ";

            return await conn.ExecuteAsync(sql, new { Ids = ids });
        }

        /// <summary>
        /// Lấy entity theo Id (sync, chưa implement).
        /// Nên dùng GetByIdAsync thay thế.
        /// Created by TMHieu - 27/2/2026
        /// </summary>
        /// <param name="id">Id của entity.</param>
        /// <returns>Entity T nếu tồn tại.</returns>
        public async Task<T?> GetById(Guid id)
        {
            using var conn = Connection;

            var sql = $@" SELECT * FROM {GetReadTableName()} WHERE {_idColumn} = @Id;";

            var entity = await conn.QueryFirstOrDefaultAsync<T>(sql, new { Id = id });
            return entity;
        }

        /// <summary>
        /// Kiểm tra giá trị có tồn tại trong cột (bỏ qua soft delete và ignoreId nếu có).
        /// Created by TMHieu - 27/2/2026
        /// </summary>
        /// <param name="propertyOrColumnName">Truyền vào tên cột hàm sẽ tự map với tên cột</param>
        /// <param name="value">Giá trị cần kiểm tra.</param>
        /// <param name="ignoreId">ID cần bỏ qua (khi update để tránh trùng chính nó).</param>
        /// <returns>True nếu tồn tại, False nếu không.</returns>
        public async Task<bool> IsValueExistAsync(
            string propertyOrColumnName,
            object value,
            Guid? ignoreId = null
        )
        {
            using var conn = Connection;

            // 1) Tìm property theo ColumnAttribute.Name
            var prop = typeof(T)
                .GetProperties()
                .FirstOrDefault(p =>
                    string.Equals(
                        p.GetCustomAttribute<ColumnAttribute>()?.Name,
                        propertyOrColumnName,
                        StringComparison.OrdinalIgnoreCase
                    )
                );

            // 2) Nếu không tìm thấy, thử tìm theo tên property C#
            if (prop == null)
            {
                prop = typeof(T).GetProperty(
                    propertyOrColumnName,
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase
                );
            }

            // 3) Nếu vẫn null => báo lỗi rõ ràng
            if (prop == null)
                throw new Exception(
                    $"Không tìm thấy property hoặc column '{propertyOrColumnName}' trên entity {typeof(T).Name}."
                );

            // 4) Lấy tên cột thực trong DB:
            //    - nếu có [Column("...")] thì dùng attribute
            //    - nếu không có thì fallback: ToSnakeCase(property.Name)
            var colAttr = prop.GetCustomAttribute<ColumnAttribute>();
            var columnName = colAttr?.Name ?? ToSnakeCase(prop.Name);

            // 5) Xây SQL, dùng param để tránh injection
            var sql =
                $@" SELECT COUNT(1)
                    FROM {_tableName}
                    WHERE {columnName} = @Value
                    AND (@IgnoreId IS NULL OR {_idColumn} <> @IgnoreId);";

            // 6) Thực thi và trả về kết quả
            var count = await conn.ExecuteScalarAsync<int>(
                sql,
                new { Value = value, IgnoreId = ignoreId }
            );

            return count > 0;
        }

        #region Paging Helper

        /// <summary>
        /// Hàm helper xử lý nối chuỗi phân trang
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="param"></param>
        /// <param name="currentPage"></param>
        /// <param name="currentPageSize"></param>
        /// <returns></returns>
        protected string BuildPagingClause(
            int? page,
            int? pageSize,
            DynamicParameters param,
            out int currentPage,
            out int currentPageSize
        )
        {
            currentPage = page.HasValue && page.Value > 0 ? page.Value : DEFAULT_PAGE;

            currentPageSize =
                pageSize.HasValue && pageSize.Value > 0
                    ? Math.Min(pageSize.Value, MAX_PAGE_SIZE)
                    : DEFAULT_PAGE_SIZE;

            int offset = (currentPage - 1) * currentPageSize;

            param.Add("@Offset", offset);
            param.Add("@PageSize", currentPageSize);

            return " LIMIT @Offset, @PageSize ";
        }

        #endregion

        /// <summary>
        /// Hàm trả về danh sách có phân trang, tìm kiếm và sắp xếp
        /// Created by TMHieu - 27/2/2026
        /// </summary>
        /// <param name="page">Trang thứ mấy</param>
        /// <param name="pageSize">Số bản ghi một trang.</param>
        /// <param name="search">Từ khóa tìm kiếm</param>
        /// <returns>Đối tượng PagingResponse chứa dữ liệu và metadata</returns>
        //public virtual async Task<PagingResponse<T>> QueryPagingAsync(
        //    int? page,
        //    int? pageSize,
        //    string? search
        //)
        //{
        //    using var conn = Connection;

        //    var param = new DynamicParameters();

        //    var searchFields = GetSearchFields();

        //    // ===== WHERE =====
        //    var where = " WHERE 1=1 ";

        //    if (!string.IsNullOrWhiteSpace(search) && searchFields.Any())
        //    {
        //        var likeParts = searchFields.Select(f => $"{f} LIKE @SearchStr");
        //        where += " AND (" + string.Join(" OR ", likeParts) + ") ";
        //        param.Add("@SearchStr", $"%{search}%");
        //    }

        //    // ===== DEFAULT ORDER =====
        //    var orderClause = $" ORDER BY {_defaultSortFiled} DESC ";

        //    // ===== COUNT =====
        //    string sqlCount = $"SELECT COUNT(*) FROM {_tableName} {where};";

        //    // ===== BASE SQL =====
        //    string baseSql = $"SELECT * FROM {_tableName} {where} {orderClause}";

        //    // ===== PAGING =====
        //    string pagingClause = BuildPagingClause(
        //        page,
        //        pageSize,
        //        param,
        //        out int currentPage,
        //        out int currentPageSize
        //    );

        //    string finalSql = baseSql + pagingClause;

        //    // ===== EXECUTE =====
        //    var total = await conn.ExecuteScalarAsync<int>(sqlCount, param);
        //    var data = (await conn.QueryAsync<T>(finalSql, param)).ToList();

        //    return new PagingResponse<T>
        //    {
        //        Data = data,
        //        Meta = new Meta
        //        {
        //            Page = currentPage,
        //            PageSize = currentPageSize,
        //            Total = total,
        //        },
        //    };
        //}

        public virtual async Task<PagingResponse<T>> QueryPagingAsync(QueryRequest request)
        {
            using var conn = Connection;
            var searchFields = GetSearchFields();

            var param = new DynamicParameters();
            var where = " WHERE 1=1 ";

            int index = 0;

            //Search
            if (!string.IsNullOrWhiteSpace(request.Search) && searchFields.Any())
            {
                var likeParts = searchFields.Select(f => $"{f} LIKE @SearchStr");
                where += " AND (" + string.Join(" OR ", likeParts) + ") ";
                param.Add("@SearchStr", $"%{request.Search}%");
            }

            // Filter
            if (request.Filters != null && request.Filters.Any())
            {
                foreach (var filter in request.Filters)
                {
                    if (!FieldMap.TryGetValue(filter.Field, out var map))
                        continue;

                    if (!map.Operators.Contains(filter.Operator))
                        continue;

                    object? value = filter.Value;

                    if (
                        value != null
                        && filter.Operator != "active"
                        && filter.Operator != "inactive"
                        && filter.Operator != "isnull"
                        && filter.Operator != "notnull"
                    )
                    {
                        try
                        {
                            if (value is JsonElement json)
                            {
                                if (json.ValueKind == JsonValueKind.String)
                                    value = json.GetString();
                                else if (json.ValueKind == JsonValueKind.Number)
                                    value = json.GetDecimal();
                                else if (json.ValueKind == JsonValueKind.True)
                                    value = true;
                                else if (json.ValueKind == JsonValueKind.False)
                                    value = false;
                                else
                                    value = null;
                            }

                            if (value != null)
                                value = Convert.ChangeType(value, map.DataType);
                        }
                        catch
                        {
                            continue;
                        }
                    }

                    string paramName = $"@p{index}";
                    index++;

                    switch (filter.Operator)
                    {
                        case "eq":
                            where += $" AND {map.Column} = {paramName}";
                            param.Add(paramName, value);
                            break;

                        case "lt":
                            where += $" AND {map.Column} < {paramName}";
                            param.Add(paramName, value);
                            break;

                        case "lte":
                            where += $" AND {map.Column} <= {paramName}";
                            param.Add(paramName, value);
                            break;

                        case "gt":
                            where += $" AND {map.Column} > {paramName}";
                            param.Add(paramName, value);
                            break;

                        case "gte":
                            where += $" AND {map.Column} >= {paramName}";
                            param.Add(paramName, value);
                            break;

                        case "contains":
                            where += $" AND {map.Column} LIKE {paramName}";
                            param.Add(paramName, $"%{value}%");
                            break;

                        case "notcontains":
                            where +=
                                $" AND ({map.Column} NOT LIKE {paramName} OR {map.Column} IS NULL)";
                            param.Add(paramName, $"%{value}%");
                            break;

                        case "starts":
                            where += $" AND {map.Column} LIKE {paramName}";
                            param.Add(paramName, $"{value}%");
                            break;

                        case "ends":
                            where += $" AND {map.Column} LIKE {paramName}";
                            param.Add(paramName, $"%{value}");
                            break;

                        case "neq":
                            where += $" AND {map.Column} <> {paramName}";
                            param.Add(paramName, value);
                            break;

                        case "active":
                            where += $" AND {map.Column} = TRUE";
                            break;

                        case "inactive":
                            where += $" AND {map.Column} = FALSE";
                            break;

                        case "isnull":
                            where += $" AND {map.Column} IS NULL";
                            break;

                        case "notnull":
                            where += $" AND {map.Column} IS NOT NULL";
                            break;
                    }
                }
            }

            // ===== SORT =====

            string orderClause = "";

            if (request.Sorts != null && request.Sorts.Any())
            {
                var orderParts = request.Sorts.Select(s =>
                {
                    var prop = typeof(T)
                        .GetProperties()
                        .FirstOrDefault(p =>
                            p.Name.EndsWith(s.Field, StringComparison.OrdinalIgnoreCase)
                        );

                    if (prop == null)
                        throw new Exception(
                            $"Field '{s.Field}' không tồn tại trong entity {typeof(T).Name}"
                        );

                    var columnName =
                        prop.GetCustomAttribute<ColumnAttribute>()?.Name ?? ToSnakeCase(prop.Name);

                    var direction = s.Direction?.ToUpper() == "DESC" ? "DESC" : "ASC";

                    return $"{columnName} {direction}";
                });

                orderClause = " ORDER BY " + string.Join(", ", orderParts);
            }
            else
            {
                orderClause = $" ORDER BY {_defaultSortFiled} DESC ";
            }

            // ===== COUNT =====

            string sqlCount = $"SELECT COUNT(*) FROM {GetReadTableName()} {where}";

            // ===== BASE SQL =====

            string baseSql = $"SELECT * FROM {GetReadTableName()} {where} {orderClause}";

            // ===== PAGING =====

            string pagingClause = BuildPagingClause(
                request.Page,
                request.PageSize,
                param,
                out int currentPage,
                out int currentPageSize
            );

            string finalSql = baseSql + pagingClause;

            var total = await conn.ExecuteScalarAsync<int>(sqlCount, param);
            var data = (await conn.QueryAsync<T>(finalSql, param)).ToList();

            return new PagingResponse<T>
            {
                Data = data,
                Meta = new Meta
                {
                    Page = currentPage,
                    PageSize = currentPageSize,
                    Total = total,
                },
            };
        }

        /// <summary>
        /// Hàm truy vấn toàn bộ dữ liệu theo điều kiện tìm kiếm và loại bản ghi,
        /// không áp dụng phân trang và không áp dụng sắp xếp.
        /// Created by TMHieu - 27/2/2026
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
        public virtual async Task<PagingResponse<T>> QueryAllExcludeAsync(
            string? search,
            string? type,
            List<Guid>? excludeIds
        )
        {
            using var conn = Connection;

            // 1. WHERE cơ bản (chưa xóa mềm)
            var where = $"";

            // 2. Lọc theo type
            if (!string.IsNullOrWhiteSpace(type))
            {
                where += $" AND {_tableName}_type = @Type ";
            }

            // 3. Search
            var searchFields = GetSearchFields();
            if (!string.IsNullOrWhiteSpace(search) && searchFields.Any())
            {
                var likeParts = searchFields.Select(f => $"{f} LIKE @SearchStr");
                where += " AND (" + string.Join(" OR ", likeParts) + ") ";
            }

            // 4. Exclude theo danh sách Guid
            if (excludeIds != null && excludeIds.Any())
            {
                where += $" AND {_idColumn} NOT IN @ExcludeIds ";
            }

            // 5. SQL
            var sql =
                $@" SELECT *
                    FROM {GetReadTableName()}
                    {where};";

            // 6. Params
            var param = new DynamicParameters();
            param.Add("@SearchStr", $"%{search}%");

            if (!string.IsNullOrWhiteSpace(type))
                param.Add("@Type", type);

            if (excludeIds != null && excludeIds.Any())
                param.Add("@ExcludeIds", excludeIds);

            // 7. Query
            var data = await conn.QueryAsync<T>(sql, param);
            var total = data.ToList().Count;

            return new PagingResponse<T>
            {
                Data = data.ToList(),
                Meta = new Meta
                {
                    Page = 1,
                    PageSize = total,
                    Total = total,
                },
            };
        }
    }
}
