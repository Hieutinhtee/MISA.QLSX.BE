using System;
using System.Collections.Generic;
using System.Linq;
using MISA.QLSX.Core.Entities;
using MISA.QLSX.Core.Exceptions;
using MISA.QLSX.Core.Interfaces.Repository;
using MISA.QLSX.Core.Interfaces.Service;

namespace MISA.QLSX.Core.Services
{
    public class EmployeeService : BaseServices<Employee>, IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IAccountRepository _accountRepository;

        /// <summary>
        /// Role mặc định cho nhân viên mới (Nhân viên)
        /// </summary>
        private static readonly Guid DefaultEmployeeRoleId = Guid.Parse(
            "4abcf2f6-43e7-11f1-8388-d0c5d346d1a4"
        );

        public EmployeeService(
            IEmployeeRepository employeeRepository,
            IAccountRepository accountRepository
        )
            : base(employeeRepository)
        {
            _employeeRepository = employeeRepository;
            _accountRepository = accountRepository;
        }

        /// <summary>
        /// Override CreateAsync để tự động tạo account cho nhân viên mới,
        /// tránh lỗi FK constraint trên account_id.
        /// </summary>
        public override async Task<Guid> CreateAsync(Employee employee)
        {
            await ValidateAsync(employee, null);
            await BeforeSaveAsync(employee, false);

            // Gán GUID mới cho các property Guid (giống logic base)
            var properties = typeof(Employee).GetProperties();

            employee.EmployeeId = employee.EmployeeId ?? Guid.NewGuid();

            // Kiểm tra xem account với code này đã tồn tại chưa
            if (await _accountRepository.IsValueExistAsync("account_code", employee.EmployeeCode))
            {
                throw new ValidateException(
                    "AccountCode duplicate",
                    $"Tài khoản với mã {employee.EmployeeCode} đã tồn tại trong hệ thống. Vui lòng sử dụng mã nhân viên khác."
                );
            }

            // Tạo account mặc định cho nhân viên
            var account = new Account
            {
                AccountId = employee.AccountId ?? Guid.NewGuid(),
                AccountCode = employee.EmployeeCode,
                Username = employee.EmployeeCode,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                RoleId = DefaultEmployeeRoleId,
                IsActive = true,
                CreatedAt = DateTime.Now,
            };
            await _accountRepository.InsertAsync(account);

            // Gán account_id cho nhân viên
            employee.AccountId = account.AccountId;

            return await _employeeRepository.InsertAsync(employee);
        }

        protected override Task BeforeSaveAsync(Employee employee, bool isUpdate = false)
        {
            if (!isUpdate)
            {
                employee.CreatedAt = DateTime.Now;
            }

            employee.UpdatedAt = DateTime.Now;
            employee.AvatarUrl ??= "profile.jpg";

            return Task.CompletedTask;
        }

        protected override async Task ValidateAsync(Employee employee, Guid? ignoreId = null)
        {
            if (employee == null)
                throw new ValidateException(
                    "Employee object is null",
                    "Dữ liệu nhân viên không được để trống"
                );

            if (string.IsNullOrWhiteSpace(employee.EmployeeCode))
                throw new ValidateException(
                    "EmployeeCode required",
                    "Mã nhân viên không được để trống"
                );

            if (employee.EmployeeCode.Length > 20)
                throw new ValidateException(
                    "EmployeeCode max length",
                    "Mã nhân viên tối đa 20 ký tự"
                );

            if (string.IsNullOrWhiteSpace(employee.FullName))
                throw new ValidateException("FullName required", "Họ và tên không được để trống");

            if (employee.FullName.Length > 120)
                throw new ValidateException("FullName max length", "Họ và tên tối đa 120 ký tự");

            if (string.IsNullOrWhiteSpace(employee.Gender))
                throw new ValidateException("Gender required", "Giới tính không được để trống");

            if (employee.DateOfBirth == null)
                throw new ValidateException(
                    "DateOfBirth required",
                    "Ngày sinh không được để trống"
                );

            if (string.IsNullOrWhiteSpace(employee.Address))
                throw new ValidateException("Address required", "Địa chỉ không được để trống");

            if (string.IsNullOrWhiteSpace(employee.PhoneNumber))
                throw new ValidateException(
                    "PhoneNumber required",
                    "Số điện thoại không được để trống"
                );

            if (string.IsNullOrWhiteSpace(employee.Email))
                throw new ValidateException("Email required", "Email không được để trống");

            if (employee.JoinDate == null)
                throw new ValidateException(
                    "JoinDate required",
                    "Ngày vào làm không được để trống"
                );

            if (string.IsNullOrWhiteSpace(employee.NationalId))
                throw new ValidateException("NationalId required", "CCCD/CMND không được để trống");

            if (employee.DegreeId == null)
                throw new ValidateException("DegreeId required", "Bằng cấp không được để trống");

            if (
                await _employeeRepository.IsValueExistAsync(
                    nameof(Employee.EmployeeCode),
                    employee.EmployeeCode,
                    ignoreId
                )
            )
                throw new ValidateException("EmployeeCode duplicate", "Mã nhân viên đã tồn tại");

            // Kiểm tra trùng mã tài khoản (chỉ khi insert mới hoặc đổi mã)
            if (
                await _accountRepository.IsValueExistAsync(
                    "account_code",
                    employee.EmployeeCode,
                    employee.AccountId
                )
            )
                throw new ValidateException(
                    "AccountCode duplicate",
                    "Mã nhân viên đã được sử dụng cho một tài khoản khác"
                );
        }

        /// <summary>
        /// Lấy danh sách nhân viên chưa được gán hợp đồng.
        /// </summary>
        /// <returns>Danh sách nhân viên có ContractId = null.</returns>
        public async Task<List<Employee>> GetEmployeesWithoutContractAsync()
        {
            var employees = await GetAllAsync();
            return employees.Where(employee => employee.ContractId == null).ToList();
        }

        /// <summary>
        /// Lấy danh sách cán bộ đại diện ký hợp đồng (Phòng HR).
        /// </summary>
        /// <returns>Danh sách cán bộ.</returns>
        public async Task<List<Employee>> GetRepresentativesAsync()
        {
            return await _employeeRepository.GetEmployeesByRoleAsync("HR");
        }
    }
}
