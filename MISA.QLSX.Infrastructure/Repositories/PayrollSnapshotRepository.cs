using MISA.QLSX.Core.DTOs.Requests;
using MISA.QLSX.Core.Entities;
using MISA.QLSX.Core.Interfaces.Repository;
using MISA.QLSX.Infrastructure.Connection;
using Dapper;

namespace MISA.QLSX.Infrastructure.Repositories
{
    /// <summary>
    /// Cung cấp truy cập dữ liệu snapshot bảng lương và truy vấn snapshot theo danh sách bảng lương.
    /// </summary>
    public class PayrollSnapshotRepository : BaseRepository<PayrollSnapshot>, IPayrollSnapshotRepository
    {
        /// <summary>
        /// Khởi tạo repository snapshot bảng lương với factory tạo kết nối MySQL.
        /// </summary>
        /// <param name="factory">Factory tạo kết nối cơ sở dữ liệu.</param>
        public PayrollSnapshotRepository(MySqlConnectionFactory factory)
            : base(factory) { }

        /// <summary>
        /// Trả về tập cột được dùng cho tìm kiếm nhanh snapshot bảng lương.
        /// </summary>
        /// <returns>Tập tên cột hỗ trợ tìm kiếm.</returns>
        protected override HashSet<string> GetSearchFields()
        {
            return new HashSet<string> { "payroll_id" };
        }

        protected override Dictionary<string, FieldMapItem> FieldMap =>
            new()
            {
                ["payrollId"] = new() { Column = "payroll_id", DataType = typeof(Guid), Operators = new() { "eq", "neq" } },
                ["snapshotAt"] = new() { Column = "snapshot_at", DataType = typeof(DateTime), Operators = new() { "eq", "lt", "lte", "gt", "gte" } },
            };

        /// <summary>
        /// Lấy snapshot theo danh sách định danh bảng lương.
        /// </summary>
        /// <param name="payrollIds">Danh sách định danh bảng lương.</param>
        /// <returns>Danh sách snapshot thuộc các bảng lương được chỉ định.</returns>
        public async Task<List<PayrollSnapshot>> GetByPayrollIdsAsync(List<Guid> payrollIds)
        {
            if (payrollIds == null || payrollIds.Count == 0)
                return new List<PayrollSnapshot>();

            using var conn = Connection;
            var sql =
                @"SELECT *
                  FROM payroll_snapshot
                  WHERE payroll_id IN @PayrollIds";

            var data = await conn.QueryAsync<PayrollSnapshot>(sql, new { PayrollIds = payrollIds });
            return data.ToList();
        }
    }
}