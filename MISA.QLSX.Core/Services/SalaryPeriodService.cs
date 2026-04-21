using MISA.QLSX.Core.Entities;
using MISA.QLSX.Core.Exceptions;
using MISA.QLSX.Core.Interfaces.Repository;
using MISA.QLSX.Core.Interfaces.Service;

namespace MISA.QLSX.Core.Services
{
    public class SalaryPeriodService : BaseServices<SalaryPeriod>, ISalaryPeriodService
    {
        private static readonly HashSet<string> AllowedStatuses = new(StringComparer.OrdinalIgnoreCase)
        {
            "draft",
            "locked",
            "paid",
        };

        public SalaryPeriodService(ISalaryPeriodRepository repo)
            : base(repo) { }

        protected override Task BeforeSaveAsync(SalaryPeriod entity, bool isUpdate = false)
        {
            if (!isUpdate)
                entity.CreatedAt = DateTime.Now;

            entity.UpdatedAt = DateTime.Now;
            entity.Status ??= "draft";
            return Task.CompletedTask;
        }

        protected override Task ValidateAsync(SalaryPeriod entity, Guid? ignoreId = null)
        {
            if (entity == null)
                throw new ValidateException("SalaryPeriod object is null", "Dữ liệu kỳ lương không được để trống");

            if (entity.StartDate == null)
                throw new ValidateException("StartDate required", "Ngày bắt đầu kỳ lương không được để trống");

            if (entity.EndDate == null)
                throw new ValidateException("EndDate required", "Ngày kết thúc kỳ lương không được để trống");

            var startDate = entity.StartDate.Value.Date;
            var endDate = entity.EndDate.Value.Date;

            if (startDate.Day != 1)
                throw new ValidateException("StartDate invalid", "Ngày bắt đầu kỳ lương phải là ngày đầu tháng");

            if (endDate != new DateTime(startDate.Year, startDate.Month, DateTime.DaysInMonth(startDate.Year, startDate.Month)))
                throw new ValidateException("EndDate invalid", "Ngày kết thúc kỳ lương phải là ngày cuối tháng");

            if (!string.IsNullOrWhiteSpace(entity.Status) && !AllowedStatuses.Contains(entity.Status))
                throw new ValidateException("Status invalid", "Trạng thái kỳ lương không hợp lệ");

            return Task.CompletedTask;
        }
    }
}