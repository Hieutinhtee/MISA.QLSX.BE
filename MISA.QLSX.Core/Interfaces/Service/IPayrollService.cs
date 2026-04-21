using MISA.QLSX.Core.Entities;

namespace MISA.QLSX.Core.Interfaces.Service
{
    public interface IPayrollService : IBaseService<Payroll>
    {
        Task<int> GenerateDraftPayrollsAsync(Guid salaryPeriodId);

        Task<int> CalculatePayrollsAsync(Guid salaryPeriodId, Guid? employeeId = null);

        Task<int> LockPayrollsAsync(Guid salaryPeriodId);

        Task<int> MarkPayrollsPaidAsync(Guid salaryPeriodId);
    }
}