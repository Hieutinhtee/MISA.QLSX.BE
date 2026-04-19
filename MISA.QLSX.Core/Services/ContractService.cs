using System;
using MISA.QLSX.Core.Entities;
using MISA.QLSX.Core.Exceptions;
using MISA.QLSX.Core.Interfaces.Repository;
using MISA.QLSX.Core.Interfaces.Service;

namespace MISA.QLSX.Core.Services
{
    public class ContractService : BaseServices<Contract>, IContractService
    {
        private readonly IContractRepository _contractRepository;

        public ContractService(IContractRepository contractRepository)
            : base(contractRepository)
        {
            _contractRepository = contractRepository;
        }

        protected override Task BeforeSaveAsync(Contract contract, bool isUpdate = false)
        {
            if (!isUpdate)
                contract.CreatedAt = DateTime.Now;

            contract.UpdatedAt = DateTime.Now;

            if (contract.IsSigned == true && contract.SignedAt == null)
                contract.SignedAt = DateTime.Now;

            return Task.CompletedTask;
        }

        protected override async Task ValidateAsync(Contract contract, Guid? ignoreId = null)
        {
            if (contract == null)
                throw new ValidateException("Contract object is null", "Dữ liệu hợp đồng không được để trống");

            if (string.IsNullOrWhiteSpace(contract.ContractCode))
                throw new ValidateException("ContractCode required", "Mã hợp đồng không được để trống");

            if (contract.ContractCode.Length > 50)
                throw new ValidateException("ContractCode max length", "Mã hợp đồng tối đa 50 ký tự");

            if (contract.TemplateId == null)
                throw new ValidateException("TemplateId required", "Mẫu hợp đồng không được để trống");

            if (contract.EmployeeId == null)
                throw new ValidateException("EmployeeId required", "Nhân viên không được để trống");

            if (contract.CompanyRepresentativeId == null)
                throw new ValidateException("CompanyRepresentativeId required", "Đại diện công ty không được để trống");

            if (string.IsNullOrWhiteSpace(contract.CompanySignerTitle))
                throw new ValidateException("CompanySignerTitle required", "Chức danh người ký không được để trống");

            if (contract.EffectiveDate == null)
                throw new ValidateException("EffectiveDate required", "Ngày hiệu lực không được để trống");

            if (contract.BaseSalary == null || contract.BaseSalary <= 0)
                throw new ValidateException("BaseSalary invalid", "Lương cơ bản phải lớn hơn 0");

            if (contract.InsuranceSalary == null || contract.InsuranceSalary <= 0)
                throw new ValidateException("InsuranceSalary invalid", "Lương đóng bảo hiểm phải lớn hơn 0");

            if (contract.SalaryRatio == null || contract.SalaryRatio <= 0)
                throw new ValidateException("SalaryRatio invalid", "Tỷ lệ lương phải lớn hơn 0");

            if (contract.EmployeeId == contract.CompanyRepresentativeId)
                throw new ValidateException("Representative invalid", "Đại diện công ty không được trùng nhân viên ký hợp đồng");

            if (await _contractRepository.IsValueExistAsync(nameof(Contract.ContractCode), contract.ContractCode, ignoreId))
                throw new ValidateException("ContractCode duplicate", "Mã hợp đồng đã tồn tại");
        }
    }
}
