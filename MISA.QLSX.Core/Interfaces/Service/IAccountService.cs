using MISA.QLSX.Core.DTOs.Requests;
using MISA.QLSX.Core.DTOs.Responses;
using MISA.QLSX.Core.Entities;

namespace MISA.QLSX.Core.Interfaces.Service
{
    /// <summary>
    /// Service interface cho Account
    /// </summary>
    public interface IAccountService : IBaseService<Account>
    {
        /// <summary>
        /// Xử lý logic đăng nhập
        /// </summary>
        /// <param name="request">Yêu cầu đăng nhập chứa username và password</param>
        /// <returns>Phản hồi đăng nhập chứa role và employee info</returns>
        Task<LoginResponse?> LoginAsync(LoginRequest request);
    }
}
