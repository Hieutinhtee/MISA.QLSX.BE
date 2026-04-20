using MISA.QLSX.Core.Entities;
using MISA.QLSX.Core.Exceptions;
using MISA.QLSX.Core.Interfaces.Repository;
using MISA.QLSX.Core.Interfaces.Service;

namespace MISA.QLSX.Core.Services
{
    public class SalaryPolicyService : BaseServices<SalaryPolicy>, ISalaryPolicyService
    {
        private readonly ISalaryPolicyRepository _salaryPolicyRepository;

        public SalaryPolicyService(ISalaryPolicyRepository repo)
            : base(repo)
        {
            _salaryPolicyRepository = repo;
        }

        protected override Task BeforeSaveAsync(SalaryPolicy entity, bool isUpdate = false)
        {
            if (!isUpdate)
                entity.CreatedAt = DateTime.Now;

            entity.UpdatedAt = DateTime.Now;
            return Task.CompletedTask;
        }

        protected override async Task ValidateAsync(SalaryPolicy entity, Guid? ignoreId = null)
        {
            if (entity == null)
                throw new ValidateException("SalaryPolicy object is null", "Dữ liệu chính sách lương không được để trống");

            if (string.IsNullOrWhiteSpace(entity.PolicyCode))
                throw new ValidateException("PolicyCode required", "Mã chính sách không được để trống");

            if (string.IsNullOrWhiteSpace(entity.PolicyName))
                throw new ValidateException("PolicyName required", "Tên chính sách không được để trống");

            if (entity.StandardWorkdays == null || entity.StandardWorkdays <= 0)
                throw new ValidateException("StandardWorkdays invalid", "Số ngày công chuẩn phải lớn hơn 0");

            if (entity.EffectiveFrom == null)
                throw new ValidateException("EffectiveFrom required", "Ngày hiệu lực không được để trống");

            if (entity.EffectiveTo != null && entity.EffectiveTo < entity.EffectiveFrom)
                throw new ValidateException("Effective range invalid", "Ngày kết thúc hiệu lực phải lớn hơn hoặc bằng ngày bắt đầu");

            if (await _salaryPolicyRepository.IsValueExistAsync(nameof(SalaryPolicy.PolicyCode), entity.PolicyCode, ignoreId))
                throw new ValidateException("PolicyCode duplicate", "Mã chính sách đã tồn tại");
        }
    }
}
