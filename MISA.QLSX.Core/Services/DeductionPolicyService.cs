using MISA.QLSX.Core.Entities;
using MISA.QLSX.Core.Exceptions;
using MISA.QLSX.Core.Interfaces.Repository;
using MISA.QLSX.Core.Interfaces.Service;

namespace MISA.QLSX.Core.Services
{
    public class DeductionPolicyService : BaseServices<DeductionPolicy>, IDeductionPolicyService
    {
        private readonly IDeductionPolicyRepository _deductionPolicyRepository;

        public DeductionPolicyService(IDeductionPolicyRepository repo)
            : base(repo)
        {
            _deductionPolicyRepository = repo;
        }

        protected override Task BeforeSaveAsync(DeductionPolicy entity, bool isUpdate = false)
        {
            if (!isUpdate)
                entity.CreatedAt = DateTime.Now;

            entity.UpdatedAt = DateTime.Now;
            return Task.CompletedTask;
        }

        protected override async Task ValidateAsync(DeductionPolicy entity, Guid? ignoreId = null)
        {
            if (entity == null)
                throw new ValidateException("DeductionPolicy object is null", "Dữ liệu chính sách giảm trừ không được để trống");

            if (string.IsNullOrWhiteSpace(entity.PolicyCode))
                throw new ValidateException("PolicyCode required", "Mã chính sách không được để trống");

            if (string.IsNullOrWhiteSpace(entity.PolicyName))
                throw new ValidateException("PolicyName required", "Tên chính sách không được để trống");

            if (entity.SocialInsuranceRate == null || entity.SocialInsuranceRate < 0 || entity.SocialInsuranceRate > 100)
                throw new ValidateException("SocialInsuranceRate invalid", "Tỷ lệ BHXH phải trong khoảng 0 đến 100");

            if (entity.HealthInsuranceRate == null || entity.HealthInsuranceRate < 0 || entity.HealthInsuranceRate > 100)
                throw new ValidateException("HealthInsuranceRate invalid", "Tỷ lệ BHYT phải trong khoảng 0 đến 100");

            if (entity.UnemploymentInsuranceRate == null || entity.UnemploymentInsuranceRate < 0 || entity.UnemploymentInsuranceRate > 100)
                throw new ValidateException("UnemploymentInsuranceRate invalid", "Tỷ lệ BHTN phải trong khoảng 0 đến 100");

            if (entity.PersonalDeductionAmount == null || entity.PersonalDeductionAmount < 0)
                throw new ValidateException("PersonalDeductionAmount invalid", "Mức giảm trừ bản thân phải lớn hơn hoặc bằng 0");

            if (entity.DependentDeductionAmount == null || entity.DependentDeductionAmount < 0)
                throw new ValidateException("DependentDeductionAmount invalid", "Mức giảm trừ người phụ thuộc phải lớn hơn hoặc bằng 0");

            if (entity.EffectiveFrom == null)
                throw new ValidateException("EffectiveFrom required", "Ngày hiệu lực không được để trống");

            if (entity.EffectiveTo != null && entity.EffectiveTo < entity.EffectiveFrom)
                throw new ValidateException("Effective range invalid", "Ngày kết thúc hiệu lực phải lớn hơn hoặc bằng ngày bắt đầu");

            if (await _deductionPolicyRepository.IsValueExistAsync(nameof(DeductionPolicy.PolicyCode), entity.PolicyCode, ignoreId))
                throw new ValidateException("PolicyCode duplicate", "Mã chính sách đã tồn tại");
        }
    }
}
