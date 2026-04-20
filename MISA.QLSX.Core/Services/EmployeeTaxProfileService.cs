using MISA.QLSX.Core.Entities;
using MISA.QLSX.Core.Exceptions;
using MISA.QLSX.Core.Interfaces.Repository;
using MISA.QLSX.Core.Interfaces.Service;

namespace MISA.QLSX.Core.Services
{
    public class EmployeeTaxProfileService : BaseServices<EmployeeTaxProfile>, IEmployeeTaxProfileService
    {
        public EmployeeTaxProfileService(IEmployeeTaxProfileRepository repo)
            : base(repo) { }

        protected override Task BeforeSaveAsync(EmployeeTaxProfile entity, bool isUpdate = false)
        {
            if (!isUpdate)
                entity.CreatedAt = DateTime.Now;

            entity.UpdatedAt = DateTime.Now;
            return Task.CompletedTask;
        }

        protected override Task ValidateAsync(EmployeeTaxProfile entity, Guid? ignoreId = null)
        {
            if (entity == null)
                throw new ValidateException("EmployeeTaxProfile object is null", "Dữ liệu hồ sơ thuế nhân viên không được để trống");

            if (entity.EmployeeId == null)
                throw new ValidateException("EmployeeId required", "Nhân viên không được để trống");

            if (entity.DependentCount == null || entity.DependentCount < 0)
                throw new ValidateException("DependentCount invalid", "Số người phụ thuộc phải lớn hơn hoặc bằng 0");

            if (entity.EffectiveFrom == null)
                throw new ValidateException("EffectiveFrom required", "Ngày hiệu lực không được để trống");

            if (entity.EffectiveTo != null && entity.EffectiveTo < entity.EffectiveFrom)
                throw new ValidateException("Effective range invalid", "Ngày kết thúc hiệu lực phải lớn hơn hoặc bằng ngày bắt đầu");

            return Task.CompletedTask;
        }
    }
}
