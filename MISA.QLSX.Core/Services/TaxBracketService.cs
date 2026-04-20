using MISA.QLSX.Core.Entities;
using MISA.QLSX.Core.Exceptions;
using MISA.QLSX.Core.Interfaces.Repository;
using MISA.QLSX.Core.Interfaces.Service;

namespace MISA.QLSX.Core.Services
{
    public class TaxBracketService : BaseServices<TaxBracket>, ITaxBracketService
    {
        private readonly ITaxBracketRepository _taxBracketRepository;

        public TaxBracketService(ITaxBracketRepository repo)
            : base(repo)
        {
            _taxBracketRepository = repo;
        }

        protected override Task BeforeSaveAsync(TaxBracket entity, bool isUpdate = false)
        {
            if (!isUpdate)
                entity.CreatedAt = DateTime.Now;

            entity.UpdatedAt = DateTime.Now;
            return Task.CompletedTask;
        }

        protected override async Task ValidateAsync(TaxBracket entity, Guid? ignoreId = null)
        {
            if (entity == null)
                throw new ValidateException("TaxBracket object is null", "Dữ liệu bậc thuế không được để trống");

            if (string.IsNullOrWhiteSpace(entity.BracketCode))
                throw new ValidateException("BracketCode required", "Mã bậc thuế không được để trống");

            if (string.IsNullOrWhiteSpace(entity.BracketName))
                throw new ValidateException("BracketName required", "Tên bậc thuế không được để trống");

            if (entity.LowerBound == null || entity.LowerBound < 0)
                throw new ValidateException("LowerBound invalid", "Ngưỡng dưới phải lớn hơn hoặc bằng 0");

            if (entity.UpperBound != null && entity.UpperBound <= entity.LowerBound)
                throw new ValidateException("UpperBound invalid", "Ngưỡng trên phải lớn hơn ngưỡng dưới");

            if (entity.TaxRate == null || entity.TaxRate < 0 || entity.TaxRate > 100)
                throw new ValidateException("TaxRate invalid", "Thuế suất phải trong khoảng 0 đến 100");

            if (entity.EffectiveFrom == null)
                throw new ValidateException("EffectiveFrom required", "Ngày hiệu lực không được để trống");

            if (entity.EffectiveTo != null && entity.EffectiveTo < entity.EffectiveFrom)
                throw new ValidateException("Effective range invalid", "Ngày kết thúc hiệu lực phải lớn hơn hoặc bằng ngày bắt đầu");

            if (await _taxBracketRepository.IsValueExistAsync(nameof(TaxBracket.BracketCode), entity.BracketCode, ignoreId))
                throw new ValidateException("BracketCode duplicate", "Mã bậc thuế đã tồn tại");
        }
    }
}
