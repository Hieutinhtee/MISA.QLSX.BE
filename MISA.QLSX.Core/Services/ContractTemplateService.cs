using System;
using MISA.QLSX.Core.Entities;
using MISA.QLSX.Core.Exceptions;
using MISA.QLSX.Core.Interfaces.Repository;
using MISA.QLSX.Core.Interfaces.Service;

namespace MISA.QLSX.Core.Services
{
    public class ContractTemplateService : BaseServices<ContractTemplate>, IContractTemplateService
    {
        private readonly IContractTemplateRepository _contractTemplateRepository;

        public ContractTemplateService(IContractTemplateRepository contractTemplateRepository)
            : base(contractTemplateRepository)
        {
            _contractTemplateRepository = contractTemplateRepository;
        }

        protected override Task BeforeSaveAsync(ContractTemplate template, bool isUpdate = false)
        {
            if (!isUpdate)
            {
                template.CreatedAt = DateTime.Now;
                template.IsActive ??= true;
                template.Version ??= 1;
            }

            template.UpdatedAt = DateTime.Now;
            return Task.CompletedTask;
        }

        protected override async Task ValidateAsync(ContractTemplate template, Guid? ignoreId = null)
        {
            if (template == null)
                throw new ValidateException("ContractTemplate object is null", "Dữ liệu mẫu hợp đồng không được để trống");

            if (string.IsNullOrWhiteSpace(template.TemplateCode))
                throw new ValidateException("TemplateCode required", "Mã mẫu hợp đồng không được để trống");

            if (template.TemplateCode.Length > 50)
                throw new ValidateException("TemplateCode max length", "Mã mẫu hợp đồng tối đa 50 ký tự");

            if (string.IsNullOrWhiteSpace(template.TemplateName))
                throw new ValidateException("TemplateName required", "Tên mẫu hợp đồng không được để trống");

            if (template.TemplateName.Length > 255)
                throw new ValidateException("TemplateName max length", "Tên mẫu hợp đồng tối đa 255 ký tự");

            if (string.IsNullOrWhiteSpace(template.ContractType))
                throw new ValidateException("ContractType required", "Loại hợp đồng không được để trống");

            if (template.Version.HasValue && template.Version.Value < 1)
                throw new ValidateException("Version invalid", "Phiên bản phải lớn hơn hoặc bằng 1");

            if (await _contractTemplateRepository.IsValueExistAsync(nameof(ContractTemplate.TemplateCode), template.TemplateCode, ignoreId))
                throw new ValidateException("TemplateCode duplicate", "Mã mẫu hợp đồng đã tồn tại");
        }
    }
}
