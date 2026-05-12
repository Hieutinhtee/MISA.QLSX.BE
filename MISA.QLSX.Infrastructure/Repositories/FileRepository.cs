using Dapper;
using MISA.QLSX.Core.DTOs.Requests;
using MISA.QLSX.Core.DTOs.Responses;
using MISA.QLSX.Core.Entities;
using MISA.QLSX.Core.Interfaces.Repository;
using MISA.QLSX.Infrastructure.Connection;

namespace MISA.QLSX.Infrastructure.Repositories
{
    /// <summary>
    /// Repository xu ly metadata va lien ket tep.
    /// </summary>
    public class FileRepository : BaseRepository<FileResource>, IFileRepository
    {
        /// <summary>
        /// Khoi tao repository tep voi factory ket noi MySQL.
        /// </summary>
        /// <param name="factory">Factory tao ket noi database.</param>
        public FileRepository(MySqlConnectionFactory factory)
            : base(factory) { }

        /// <summary>
        /// Lay danh sach cot ho tro tim kiem metadata tep.
        /// </summary>
        /// <returns>Danh sach cot tim kiem hop le.</returns>
        protected override HashSet<string> GetSearchFields()
        {
            return new HashSet<string>
            {
                "original_name",
                "stored_name",
                "mime_type",
            };
        }

        /// <summary>
        /// Dinh nghia map truong cho filter/paging metadata tep.
        /// </summary>
        protected override Dictionary<string, FieldMapItem> FieldMap =>
            new()
            {
                ["originalName"] = new()
                {
                    Column = "original_name",
                    DataType = typeof(string),
                    Operators = new() { "eq", "contains", "starts", "ends", "neq", "notcontains" },
                },
                ["mimeType"] = new()
                {
                    Column = "mime_type",
                    DataType = typeof(string),
                    Operators = new() { "eq", "contains", "starts", "ends", "neq", "notcontains" },
                },
                ["createdAt"] = new()
                {
                    Column = "created_at",
                    DataType = typeof(DateTime),
                    Operators = new() { "eq", "lt", "lte", "gt", "gte" },
                },
            };

        /// <summary>
        /// Them lien ket tep vao ban ghi nghiep vu.
        /// </summary>
        /// <param name="reference">Thong tin lien ket tep.</param>
        /// <returns>ID lien ket vua tao.</returns>
        public async Task<Guid> InsertReferenceAsync(FileReference reference)
        {
            using var conn = Connection;
            var sql =
                @"INSERT INTO file_reference
                  (
                    file_reference_id,
                    file_id,
                    module_name,
                    entity_name,
                    entity_id,
                    purpose,
                    created_by,
                    created_at
                  )
                  VALUES
                  (
                    @FileReferenceId,
                    @FileId,
                    @ModuleName,
                    @EntityName,
                    @EntityId,
                    @Purpose,
                    @CreatedBy,
                    @CreatedAt
                  );";

            await conn.ExecuteAsync(sql, reference);
            return reference.FileReferenceId ?? Guid.Empty;
        }

        /// <summary>
        /// Lay danh sach tep theo entity nghiep vu.
        /// </summary>
        /// <param name="moduleName">Ma module nghiep vu.</param>
        /// <param name="entityName">Ten thuc the nghiep vu.</param>
        /// <param name="entityId">ID ban ghi nghiep vu.</param>
        /// <returns>Danh sach tep da gan.</returns>
        public async Task<List<FileItemResponse>> GetByEntityAsync(string moduleName, string entityName, Guid entityId)
        {
            using var conn = Connection;
            var sql =
                @"SELECT
                    f.file_id AS FileId,
                    f.original_name AS OriginalName,
                    f.mime_type AS MimeType,
                    f.size_bytes AS SizeBytes,
                    r.module_name AS ModuleName,
                    r.entity_name AS EntityName,
                    r.entity_id AS EntityId,
                    r.purpose AS Purpose,
                    r.created_at AS CreatedAt
                  FROM file_reference r
                  INNER JOIN file_resource f ON r.file_id = f.file_id
                  WHERE r.module_name = @ModuleName
                    AND r.entity_name = @EntityName
                    AND r.entity_id = @EntityId
                    AND f.is_deleted = '00000000-0000-0000-0000-000000000000'
                  ORDER BY r.created_at DESC";

            var data = await conn.QueryAsync<FileItemResponse>(
                sql,
                new
                {
                    ModuleName = moduleName,
                    EntityName = entityName,
                    EntityId = entityId,
                }
            );

            return data.ToList();
        }

        /// <summary>
        /// Xoa toan bo lien ket theo file ID.
        /// </summary>
        /// <param name="fileId">ID tep can xoa lien ket.</param>
        /// <returns>So lien ket bi xoa.</returns>
        public async Task<int> DeleteReferencesByFileIdAsync(Guid fileId)
        {
            using var conn = Connection;
            var sql = "DELETE FROM file_reference WHERE file_id = @FileId";
            return await conn.ExecuteAsync(sql, new { FileId = fileId });
        }
    }
}
