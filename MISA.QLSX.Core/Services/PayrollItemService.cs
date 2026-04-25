using MISA.QLSX.Core.Entities;
using MISA.QLSX.Core.Exceptions;
using MISA.QLSX.Core.Interfaces.Repository;
using MISA.QLSX.Core.Interfaces.Service;

namespace MISA.QLSX.Core.Services
{
    public class PayrollItemService : BaseServices<PayrollItem>, IPayrollItemService
    {
        private readonly IPayrollItemRepository _payrollItemRepository;
        private readonly IPayrollRepository _payrollRepository;

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

        public PayrollItemService(IPayrollItemRepository repo, IPayrollRepository payrollRepository)
            : base(repo)
        {
            _payrollItemRepository = repo;
            _payrollRepository = payrollRepository;
        }

        /// <summary>
        /// Tạo mới khoản mục lương và chặn thao tác nếu bảng lương cha đã khóa/đã chi trả.
        /// </summary>
        /// <param name="entity">Khoản mục lương cần tạo.</param>
        /// <returns>ID khoản mục lương vừa tạo.</returns>
        public override async Task<Guid> CreateAsync(PayrollItem entity)
        {
            if (entity == null)
                throw new ValidateException("PayrollItem object is null", "Dữ liệu khoản mục lương không được để trống");

            await EnsurePayrollEditableAsync(entity.PayrollId);
            return await base.CreateAsync(entity);
        }

        /// <summary>
        /// Cập nhật khoản mục lương và chặn thao tác nếu bảng lương cha đã khóa/đã chi trả.
        /// </summary>
        /// <param name="id">ID khoản mục cần cập nhật.</param>
        /// <param name="entity">Dữ liệu khoản mục cập nhật.</param>
        /// <returns>ID khoản mục đã cập nhật.</returns>
        public override async Task<Guid> UpdateAsync(Guid id, PayrollItem entity)
        {
            if (entity == null)
                throw new ValidateException("PayrollItem object is null", "Dữ liệu khoản mục lương không được để trống");

            await EnsurePayrollEditableAsync(entity.PayrollId);
            return await base.UpdateAsync(id, entity);
        }

        /// <summary>
        /// Xóa khoản mục lương và chặn thao tác nếu bảng lương cha đã khóa/đã chi trả.
        /// </summary>
        /// <param name="id">ID khoản mục cần xóa.</param>
        /// <returns>Số bản ghi bị ảnh hưởng.</returns>
        public override async Task<int> DeleteAsync(Guid id)
        {
            var current = await _payrollItemRepository.GetById(id);
            if (current == null)
                throw new NotFoundException("PayrollItem not found", "Không tìm thấy khoản mục lương");

            await EnsurePayrollEditableAsync(current.PayrollId);
            return await base.DeleteAsync(id);
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

        /// <summary>
        /// Kiểm tra bảng lương cha còn cho phép chỉnh sửa khoản mục hay không.
        /// </summary>
        /// <param name="payrollId">ID bảng lương cha.</param>
        /// <returns>Task hoàn thành khi bảng lương cha còn editable.</returns>
        private async Task EnsurePayrollEditableAsync(Guid? payrollId)
        {
            if (payrollId == null)
                throw new ValidateException("PayrollId required", "Bảng lương cha không được để trống");

            var payroll = await _payrollRepository.GetById(payrollId.Value);
            if (payroll == null)
                throw new NotFoundException("Payroll not found", "Không tìm thấy bảng lương cha");

            if (string.Equals(payroll.Status, "locked", StringComparison.OrdinalIgnoreCase)
                || string.Equals(payroll.Status, "paid", StringComparison.OrdinalIgnoreCase))
            {
                throw new ValidateException("Payroll status invalid", "Bảng lương đã khóa hoặc đã chi trả, không được phép chỉnh sửa khoản mục");
            }
        }
    }
}