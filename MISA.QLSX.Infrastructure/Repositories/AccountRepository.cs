using Dapper;
using MISA.QLSX.Core.DTOs.Requests;
using MISA.QLSX.Core.DTOs.Responses;
using MISA.QLSX.Core.Entities;
using MISA.QLSX.Core.Interfaces.Repository;
using MISA.QLSX.Infrastructure.Connection;

namespace MISA.QLSX.Infrastructure.Repositories
{
    /// <summary>
    /// Repository cho Account
    /// </summary>
    public class AccountRepository : BaseRepository<Account>, IAccountRepository
    {
        public AccountRepository(MySqlConnectionFactory factory)
            : base(factory) { }

        /// <summary>
        /// Trả về danh sách cột hỗ trợ tìm kiếm nhanh cho account.
        /// </summary>
        /// <returns>Tập tên cột hỗ trợ tìm kiếm.</returns>
        protected override HashSet<string> GetSearchFields()
        {
            return new HashSet<string> { "account_code", "username" };
        }

        /// <summary>
        /// Mapping field filter từ FE sang cột DB cho account.
        /// </summary>
        protected override Dictionary<string, FieldMapItem> FieldMap =>
            new()
            {
                ["accountCode"] = new()
                {
                    Column = "account_code",
                    DataType = typeof(string),
                    Operators = new() { "eq", "neq", "contains", "notcontains", "starts", "ends" },
                },
                ["username"] = new()
                {
                    Column = "username",
                    DataType = typeof(string),
                    Operators = new() { "eq", "neq", "contains", "notcontains", "starts", "ends" },
                },
                ["roleId"] = new()
                {
                    Column = "role_id",
                    DataType = typeof(Guid),
                    Operators = new() { "eq", "neq", "isnull", "notnull" },
                },
                ["isActive"] = new()
                {
                    Column = "is_active",
                    DataType = typeof(bool),
                    Operators = new() { "eq", "neq" },
                },
            };

        /// <summary>
        /// Lấy thông tin account cùng role và employee theo username
        /// </summary>
        /// <param name="username">Tên đăng nhập</param>
        /// <returns>DTO chứa thông tin account, role, employee</returns>
        public async Task<AuthAccountInfo?> GetAccountWithRoleAndEmployeeByUsernameAsync(
            string username
        )
        {
            const string sql = @"
                SELECT 
                    a.account_id,
                    a.username,
                    a.password_hash,
                    a.is_active,
                    r.role_code,
                    r.role_name,
                    e.employee_id,
                    e.full_name,
                    e.department_id
                FROM account a
                LEFT JOIN role r ON a.role_id = r.role_id
                LEFT JOIN employee e ON a.account_id = e.account_id
                WHERE a.username = @Username";

            using (var connection = _factory.CreateConnection())
            {
                var result = await connection.QueryFirstOrDefaultAsync<AuthAccountInfo>(
                    sql,
                    new { Username = username }
                );
                return result;
            }
        }
    }
}
