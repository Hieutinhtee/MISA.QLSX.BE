using MISA.QLSX.Core.Entities;
using MISA.QLSX.Core.Exceptions;
using MISA.QLSX.Core.Interfaces.Repository;
using MISA.QLSX.Core.Interfaces.Service;

namespace MISA.QLSX.Core.Services
{
    public class PayrollItemService : BaseServices<PayrollItem>, IPayrollItemService
    {
        private readonly IPayrollItemRepository _payrollItemRepository;

        private static readonly HashSet<string> AllowedItemTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "addition",
            "deduction",
        };

        private static readonly HashSet<string> AllowedSourceTables = new(StringComparer.OrdinalIgnoreCase)
        {
            "attendance",
            "evaluation",
            "business_trip",
            "leave_request",
            "manual",
        };

        public PayrollItemService(IPayrollItemRepository repo)
            : base(repo)
        {
            _payrollItemRepository = repo;
        }

        protected override Task BeforeSaveAsync(PayrollItem entity, bool isUpdate = false)
        {
            if (!isUpdate)
                entity.CreatedAt = DateTime.Now;

            entity.UpdatedAt = DateTime.Now;
            entity.SourceTable ??= "manual";
            return Task.CompletedTask;
        }

        protected override async Task ValidateAsync(PayrollItem entity, Guid? ignoreId = null)
        {
            if (entity == null)
                throw new ValidateException("PayrollItem object is null", "Dữ liệu khoản mục lương không được để trống");

            if (string.IsNullOrWhiteSpace(entity.PayrollItemCode))
                throw new ValidateException("PayrollItemCode required", "Mã khoản mục lương không được để trống");

            if (entity.PayrollId == null)
                throw new ValidateException("PayrollId required", "Bảng lương cha không được để trống");

            if (string.IsNullOrWhiteSpace(entity.ItemName))
                throw new ValidateException("ItemName required", "Tên khoản mục lương không được để trống");

            if ((entity.Amount ?? 0) < 0)
                throw new ValidateException("Amount invalid", "Số tiền khoản mục không được âm");

            if (string.IsNullOrWhiteSpace(entity.ItemType) || !AllowedItemTypes.Contains(entity.ItemType))
                throw new ValidateException("ItemType invalid", "Loại khoản mục phải là addition hoặc deduction");

            if (string.IsNullOrWhiteSpace(entity.SourceTable) || !AllowedSourceTables.Contains(entity.SourceTable))
                throw new ValidateException("SourceTable invalid", "Nguồn khoản mục không hợp lệ");

            if (string.Equals(entity.SourceTable, "manual", StringComparison.OrdinalIgnoreCase) && entity.SourceId != null)
                throw new ValidateException("SourceId invalid", "Nguồn manual không được có sourceId");

            if (!string.Equals(entity.SourceTable, "manual", StringComparison.OrdinalIgnoreCase) && entity.SourceId == null)
                throw new ValidateException("SourceId required", "Khoản mục không phải manual phải có sourceId");

            if (await _payrollItemRepository.IsValueExistAsync(nameof(PayrollItem.PayrollItemCode), entity.PayrollItemCode, ignoreId))
                throw new ValidateException("PayrollItemCode duplicate", "Mã khoản mục lương đã tồn tại");
        }
    }
}