using MISA.QLSX.Core.DTOs.Requests;
using MISA.QLSX.Core.DTOs.Responses;
using MISA.QLSX.Core.Entities;
using MISA.QLSX.Core.Exceptions;
using MISA.QLSX.Core.Interfaces.Repository;
using MISA.QLSX.Core.Interfaces.Service;

namespace MISA.QLSX.Core.Services
{
    /// <summary>
    /// Service cho Account
    /// </summary>
    public class AccountService : BaseServices<Account>, IAccountService
    {
        private readonly IAccountRepository _accountRepository;

        /// <summary>
        /// Constructor cho AccountService
        /// </summary>
        /// <param name="accountRepository">Repository chuyên biệt cho Account</param>
        public AccountService(IAccountRepository accountRepository)
            : base(accountRepository)
        {
            _accountRepository = accountRepository;
        }

        /// <summary>
        /// Xử lý logic đăng nhập
        /// </summary>
        /// <param name="request">Yêu cầu đăng nhập chứa username và password</param>
        /// <returns>Phản hồi đăng nhập chứa role và employee info</returns>
        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Username) || string.IsNullOrWhiteSpace(request?.Password))
            {
                throw new ValidateException("Username và password không được để trống");
            }

            // Lấy account với role và employee
            var accountData = await _accountRepository.GetAccountWithRoleAndEmployeeByUsernameAsync(
                request.Username
            );

            if (accountData == null)
            {
                throw new UnauthorizedException(
                    "Tên đăng nhập hoặc mật khẩu không chính xác",
                    "Tên đăng nhập hoặc mật khẩu không chính xác"
                );
            }

            // Kiểm tra trạng thái tài khoản
            if (!accountData.IsActive)
            {
                throw new ForbiddenException("Tài khoản đã bị khóa", "Tài khoản đã bị khóa");
            }

            // Kiểm tra mật khẩu
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(
                request.Password,
                accountData.PasswordHash
            );
            if (!isPasswordValid)
            {
                throw new UnauthorizedException(
                    "Tên đăng nhập hoặc mật khẩu không chính xác",
                    "Tên đăng nhập hoặc mật khẩu không chính xác"
                );
            }

            // Trả về response
            return new LoginResponse
            {
                AccountId = accountData.AccountId,
                Role = accountData.RoleCode,
                Name = accountData.FullName,
                EmployeeId = accountData.EmployeeId,
                DepartmentId = accountData.DepartmentId
            };
        }
    }
}
