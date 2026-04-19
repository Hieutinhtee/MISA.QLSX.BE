using System;
using MISA.QLSX.Core.Entities;
using MISA.QLSX.Core.Exceptions;
using MISA.QLSX.Core.Interfaces.Repository;
using MISA.QLSX.Core.Interfaces.Service;

namespace MISA.QLSX.Core.Services
{
    public class EmployeeService : BaseServices<Employee>, IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepository;

        public EmployeeService(IEmployeeRepository employeeRepository)
            : base(employeeRepository)
        {
            _employeeRepository = employeeRepository;
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
                throw new ValidateException("Employee object is null", "Dữ liệu nhân viên không được để trống");

            if (string.IsNullOrWhiteSpace(employee.EmployeeCode))
                throw new ValidateException("EmployeeCode required", "Mã nhân viên không được để trống");

            if (employee.EmployeeCode.Length > 20)
                throw new ValidateException("EmployeeCode max length", "Mã nhân viên tối đa 20 ký tự");

            if (string.IsNullOrWhiteSpace(employee.FullName))
                throw new ValidateException("FullName required", "Họ và tên không được để trống");

            if (employee.FullName.Length > 120)
                throw new ValidateException("FullName max length", "Họ và tên tối đa 120 ký tự");

            if (string.IsNullOrWhiteSpace(employee.Gender))
                throw new ValidateException("Gender required", "Giới tính không được để trống");

            if (employee.DateOfBirth == null)
                throw new ValidateException("DateOfBirth required", "Ngày sinh không được để trống");

            if (string.IsNullOrWhiteSpace(employee.Address))
                throw new ValidateException("Address required", "Địa chỉ không được để trống");

            if (string.IsNullOrWhiteSpace(employee.PhoneNumber))
                throw new ValidateException("PhoneNumber required", "Số điện thoại không được để trống");

            if (string.IsNullOrWhiteSpace(employee.Email))
                throw new ValidateException("Email required", "Email không được để trống");

            if (employee.JoinDate == null)
                throw new ValidateException("JoinDate required", "Ngày vào làm không được để trống");

            if (string.IsNullOrWhiteSpace(employee.NationalId))
                throw new ValidateException("NationalId required", "CCCD/CMND không được để trống");

            if (employee.DegreeId == null)
                throw new ValidateException("DegreeId required", "Bằng cấp không được để trống");

            if (await _employeeRepository.IsValueExistAsync(nameof(Employee.EmployeeCode), employee.EmployeeCode, ignoreId))
                throw new ValidateException("EmployeeCode duplicate", "Mã nhân viên đã tồn tại");
        }
    }
}
