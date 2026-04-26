using System;
using MISA.QLSX.Core.DTOs.Responses;
using MISA.QLSX.Core.Entities;

namespace MISA.QLSX.Core.Interfaces.Repository
{
    /// <summary>
    /// Repository interface cho Account
    /// </summary>
    public interface IAccountRepository : IBaseRepository<Account>
    {
        /// <summary>
        /// Lấy thông tin account cùng role và employee theo username
        /// </summary>
        /// <param name="username">Tên đăng nhập</param>
        /// <returns>DTO chứa thông tin account, role, employee</returns>
        Task<AuthAccountInfo?> GetAccountWithRoleAndEmployeeByUsernameAsync(string username);
    }
}
