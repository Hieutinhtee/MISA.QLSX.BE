using MISA.QLSX.Core.Entities;
using MISA.QLSX.Core.Exceptions;
using MISA.QLSX.Core.Interfaces.Repository;
using MISA.QLSX.Core.Interfaces.Service;
using System.Text.Json;

namespace MISA.QLSX.Core.Services
{
    public class PayrollService : BaseServices<Payroll>, IPayrollService
    {
        // Repository dependency set for the end-to-end payroll workflow.
        private readonly IPayrollRepository _payrollRepository;
        private readonly ISalaryPeriodRepository _salaryPeriodRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IContractRepository _contractRepository;
        private readonly IAttendanceRepository _attendanceRepository;
        private readonly IPayrollItemRepository _payrollItemRepository;
        private readonly IPayrollSnapshotRepository _payrollSnapshotRepository;
        private readonly ISalaryPolicyRepository _salaryPolicyRepository;
        private readonly IDeductionPolicyRepository _deductionPolicyRepository;
        private readonly IEmployeeTaxProfileRepository _employeeTaxProfileRepository;
        private readonly ITaxBracketRepository _taxBracketRepository;

        private static readonly HashSet<string> AllowedStatuses = new(StringComparer.OrdinalIgnoreCase)
        {
            "draft",
            "processing",
            "locked",
            "paid",
        };

        public PayrollService(
            IPayrollRepository repo,
            ISalaryPeriodRepository salaryPeriodRepository,
            IEmployeeRepository employeeRepository,
            IContractRepository contractRepository,
            IAttendanceRepository attendanceRepository,
            IPayrollItemRepository payrollItemRepository,
            IPayrollSnapshotRepository payrollSnapshotRepository,
            ISalaryPolicyRepository salaryPolicyRepository,
            IDeductionPolicyRepository deductionPolicyRepository,
            IEmployeeTaxProfileRepository employeeTaxProfileRepository,
            ITaxBracketRepository taxBracketRepository
        )
            : base(repo)
        {
            _payrollRepository = repo;
            _salaryPeriodRepository = salaryPeriodRepository;
            _employeeRepository = employeeRepository;
            _contractRepository = contractRepository;
            _attendanceRepository = attendanceRepository;
            _payrollItemRepository = payrollItemRepository;
            _payrollSnapshotRepository = payrollSnapshotRepository;
            _salaryPolicyRepository = salaryPolicyRepository;
            _deductionPolicyRepository = deductionPolicyRepository;
            _employeeTaxProfileRepository = employeeTaxProfileRepository;
            _taxBracketRepository = taxBracketRepository;
        }

        /// <summary>
        /// Tạo payroll nháp cho toàn bộ nhân viên đủ điều kiện trong kỳ lương.
        /// Chỉ tạo khi kỳ ở trạng thái draft và chưa tồn tại payroll của nhân viên trong kỳ.
        /// </summary>
        public async Task<int> GenerateDraftPayrollsAsync(Guid salaryPeriodId)
        {
            var period = await EnsureSalaryPeriodAsync(salaryPeriodId);

            if (!string.Equals(period.Status, "draft", StringComparison.OrdinalIgnoreCase))
                throw new ValidateException("Salary period invalid status", "Chỉ được tạo bảng lương khi kỳ lương đang ở trạng thái draft");

            if (period.StartDate == null || period.EndDate == null)
                throw new ValidateException("Salary period date invalid", "Kỳ lương thiếu ngày bắt đầu hoặc kết thúc");

            var employees = await _employeeRepository.GetAllAsync();
            var existingPayrolls = await _payrollRepository.GetBySalaryPeriodAsync(salaryPeriodId);

            // Loại trừ nhân viên đã có payroll trong kỳ để đảm bảo idempotent khi generate nhiều lần.
            var existingEmployeeIds = existingPayrolls
                .Where(p => p.SalaryPeriodId == salaryPeriodId && p.EmployeeId != null)
                .Select(p => p.EmployeeId!.Value)
                .ToHashSet();

            var targetEmployees = employees
                .Where(e =>
                    e.EmployeeId != null
                    && e.ContractId != null
                    && (e.JoinDate == null || e.JoinDate.Value.Date <= period.EndDate.Value.Date)
                    && !existingEmployeeIds.Contains(e.EmployeeId.Value)
                )
                .ToList();

            var created = 0;
            foreach (var employee in targetEmployees)
            {
                var payrollCode = $"PR-{period.StartDate:yyyyMM}-{employee.EmployeeCode ?? employee.EmployeeId!.Value.ToString("N").Substring(0, 8).ToUpper()}";

                var payroll = new Payroll
                {
                    PayrollId = Guid.NewGuid(),
                    PayrollCode = payrollCode,
                    SalaryPeriodId = salaryPeriodId,
                    EmployeeId = employee.EmployeeId,
                    Status = "draft",
                    GrossSalary = 0,
                    NetSalary = 0,
                    TaxableSalary = 0,
                    PitTaxAmount = 0,
                    InsuranceDeduction = 0,
                    WorkingDaysActual = 0,
                    WorkingDaysStandard = 0,
                    TotalAllowance = 0,
                    TotalAddition = 0,
                    TotalDeduction = 0,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                };

                await _payrollRepository.InsertAsync(payroll);
                created++;
            }

            return created;
        }

        /// <summary>
        /// Tính payroll cho một kỳ lương theo luồng nghiệp vụ tháng.
        /// Luồng xử lý gồm: chọn policy hiệu lực, pro-rate hợp đồng theo đoạn ngày,
        /// tổng hợp phát sinh chấm công, tính bảo hiểm, tính thuế TNCN và cập nhật lại payroll item.
        /// </summary>
        /// <param name="salaryPeriodId">ID kỳ lương cần tính.</param>
        /// <param name="employeeId">ID nhân viên tùy chọn; nếu null thì tính toàn bộ payroll trong kỳ.</param>
        /// <returns>Số payroll được tính thành công.</returns>
        public async Task<int> CalculatePayrollsAsync(Guid salaryPeriodId, Guid? employeeId = null)
        {
            // 0) Validate kỳ lương đầu vào và chỉ cho phép calculate khi kỳ đang draft.
            var period = await EnsureSalaryPeriodAsync(salaryPeriodId);

            if (!string.Equals(period.Status, "draft", StringComparison.OrdinalIgnoreCase))
                throw new ValidateException("Salary period invalid status", "Chỉ được tính lương khi kỳ lương đang ở trạng thái draft");

            if (period.StartDate == null || period.EndDate == null)
                throw new ValidateException("Salary period date invalid", "Kỳ lương thiếu ngày bắt đầu hoặc kết thúc");

            var periodStart = period.StartDate.Value.Date;
            var periodEnd = period.EndDate.Value.Date;
            // periodStart/periodEnd là biên tính lương chính thức cho toàn bộ phép lọc dữ liệu nguồn.

            // 1) Lấy danh sách payroll mục tiêu trong kỳ.
            var payrolls = await _payrollRepository.GetBySalaryPeriodAsync(salaryPeriodId, employeeId);

            if (payrolls.Count == 0)
                throw new ValidateException("Payroll not found", "Không có bảng lương nào để tính cho kỳ đã chọn");

            var employeeIds = payrolls
                .Where(p => p.EmployeeId != null)
                .Select(p => p.EmployeeId!.Value)
                .Distinct()
                .ToList();

            var payrollIds = payrolls
                .Where(p => p.PayrollId != null)
                .Select(p => p.PayrollId!.Value)
                .Distinct()
                .ToList();

            // 2) Tải dữ liệu nguồn một lần để tránh query lặp trong vòng foreach.
            // contracts: phục vụ tính lương cơ bản/insurance theo hiệu lực hợp đồng.
            // attendances: phục vụ bonus, penalty, overtime.
            // policies/tax profiles/tax brackets: phục vụ insurance + PIT.
            // allPayrollItems: phục vụ đồng bộ lại payroll item sau khi tính.
            var contracts = await _contractRepository.GetEffectiveByEmployeesAsync(
                employeeIds,
                periodStart,
                periodEnd
            );
            var attendances = await _attendanceRepository.GetByEmployeesAndDateRangeAsync(
                employeeIds,
                periodStart,
                periodEnd
            );
            var salaryPolicies = await _salaryPolicyRepository.GetAllAsync();
            var deductionPolicies = await _deductionPolicyRepository.GetAllAsync();
            var taxProfiles = await _employeeTaxProfileRepository.GetAllAsync();
            var taxBrackets = await _taxBracketRepository.GetAllAsync();
            var allPayrollItems = await _payrollItemRepository.GetByPayrollIdsAsync(payrollIds);

            // 3) Chọn policy hiệu lực tại thời điểm kỳ lương bắt đầu.
            // Quy ước hiện tại: cấu hình lương/bảo hiểm/thuế áp theo chính sách hiệu lực lúc bắt đầu kỳ.
            var salaryPolicy = GetEffectiveSalaryPolicy(salaryPolicies, periodStart);
            var deductionPolicy = GetEffectiveDeductionPolicy(deductionPolicies, periodStart);
            var effectiveTaxBrackets = GetEffectiveTaxBrackets(taxBrackets, periodStart);

            var affected = 0;
            foreach (var payroll in payrolls)
            {
                // 4) Không tính lại các bản ghi đã đóng trạng thái nghiệp vụ.
                if (string.Equals(payroll.Status, "locked", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(payroll.Status, "paid", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (payroll.EmployeeId == null)
                    continue;

                // Lọc các hợp đồng có giao với kỳ lương và đủ điều kiện trạng thái để tham gia tính.
                // Các hợp đồng ngoài kỳ hoặc không có hiệu lực sẽ bị loại.
                var employeeContracts = contracts
                    .Where(c => c.EmployeeId == payroll.EmployeeId)
                    .Where(c => c.EffectiveDate != null)
                    .Where(c => c.EffectiveDate!.Value.Date <= periodEnd)
                    .Where(c => c.EndDate == null || c.EndDate.Value.Date >= periodStart)
                    .Where(c => string.IsNullOrWhiteSpace(c.ContractStatus)
                        || string.Equals(c.ContractStatus, "active", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(c.ContractStatus, "signed", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(c.ContractStatus, "draft", StringComparison.OrdinalIgnoreCase))
                    .OrderBy(c => c.EffectiveDate)
                    .ToList();

                // Không có hợp đồng hiệu lực trong kỳ thì bỏ qua payroll này.
                if (employeeContracts.Count == 0)
                    continue;

                var attendanceRows = attendances
                    .Where(a => a.EmployeeId == payroll.EmployeeId)
                    .Where(a => a.AttendanceDate != null)
                    .Where(a => a.AttendanceDate!.Value.Date >= periodStart && a.AttendanceDate.Value.Date <= periodEnd)
                    .ToList();
                // attendanceRows có thể rỗng; khi đó bonus/penalty/overtime đều coi như 0.

                // 5) Chia kỳ lương thành các đoạn hợp đồng để xử lý pro-rate đổi hợp đồng giữa tháng.
                var contractSegments = BuildContractSegments(employeeContracts, periodStart, periodEnd);
                if (contractSegments.Count == 0)
                    continue;

                var standardWorkdays = salaryPolicy?.StandardWorkdays ?? CountBusinessDays(periodStart, periodEnd);
                if (standardWorkdays <= 0)
                    standardWorkdays = 22;

                decimal baseSalaryAmount = 0;
                decimal insuranceSalaryBase = 0;
                decimal contractBusinessDaysTotal = 0;
                foreach (var segment in contractSegments)
                {
                    // segmentBusinessDays là số ngày công chuẩn (Mon-Fri) trong đoạn hợp đồng.
                    var segmentBusinessDays = CountBusinessDays(segment.StartDate, segment.EndDate);
                    contractBusinessDaysTotal += segmentBusinessDays;

                    // Lương ngày đoạn = baseSalary * salaryRatio / standardWorkdays.
                    // Lương cơ bản kỳ = tổng lương ngày đoạn * số ngày đoạn.
                    var salaryRatio = (segment.Contract.SalaryRatio ?? 100m) / 100m;
                    var segmentDailySalary = (segment.Contract.BaseSalary ?? 0m) * salaryRatio / standardWorkdays;
                    baseSalaryAmount += segmentDailySalary * segmentBusinessDays;

                    // insuranceSalaryBase được tính độc lập từ insurance_salary để tách khỏi base_salary.
                    var insuranceDaily = (segment.Contract.InsuranceSalary ?? 0m) / standardWorkdays;
                    insuranceSalaryBase += insuranceDaily * segmentBusinessDays;
                }

                // 6) Tổng hợp phát sinh từ attendance cho khoản cộng/trừ và overtime.
                var totalAddition = attendanceRows.Sum(a => a.BonusAmount ?? 0m);
                var totalDeduction = attendanceRows.Sum(a => a.PenaltyAmount ?? 0m);
                var overtimeHours = attendanceRows.Sum(a => a.OvertimeHours ?? 0m);

                // Dùng đơn giá giờ trung bình theo số ngày hợp đồng hiệu lực thực tế,
                // tránh chia lại cho standardWorkdays lần hai gây giảm đơn giá overtime.
                var hourlySalary = contractBusinessDaysTotal > 0 ? baseSalaryAmount / (contractBusinessDaysTotal * 8m) : 0m;
                var overtimeAddition = overtimeHours * hourlySalary * (salaryPolicy?.OvertimeMultiplierWeekday ?? 1.5m);

                totalAddition += overtimeAddition;

                var totalAllowance = 0m;
                // Công thức gross hiện tại:
                // gross = baseSalaryAmount + totalAllowance + totalAddition - totalDeduction.
                // totalAllowance tạm để 0 vì chưa tích hợp contract_allowance ở vòng này.
                var grossSalary = baseSalaryAmount + totalAllowance + totalAddition - totalDeduction;

                // 7) Tính bảo hiểm theo policy giảm trừ hiệu lực.
                var insuranceRate = ((deductionPolicy?.SocialInsuranceRate ?? 0m)
                    + (deductionPolicy?.HealthInsuranceRate ?? 0m)
                    + (deductionPolicy?.UnemploymentInsuranceRate ?? 0m)) / 100m;
                var insuranceDeduction = insuranceSalaryBase * insuranceRate;

                // Chọn hồ sơ thuế hiệu lực tại thời điểm cuối kỳ để phản ánh cấu hình gần nhất trong tháng.
                var employeeTaxProfile = taxProfiles
                    .Where(p => p.EmployeeId == payroll.EmployeeId)
                    .Where(p => p.EffectiveFrom != null && p.EffectiveFrom.Value.Date <= periodEnd)
                    .Where(p => p.EffectiveTo == null || p.EffectiveTo.Value.Date >= periodEnd)
                    .Where(p => p.IsActive == null || p.IsActive == true)
                    .OrderByDescending(p => p.EffectiveFrom)
                    .FirstOrDefault();
                // Nếu không có tax profile thì dependentCount mặc định về 0.

                var personalDeduction = deductionPolicy?.PersonalDeductionAmount ?? 0m;
                var dependentDeduction = (deductionPolicy?.DependentDeductionAmount ?? 0m) * (employeeTaxProfile?.DependentCount ?? 0);
                var taxableSalary = Math.Max(0m, grossSalary - insuranceDeduction - personalDeduction - dependentDeduction);
                // taxableSalary luôn >= 0 để tránh thuế âm.

                // 8) Tính thuế TNCN theo biểu lũy tiến và ra lương thực nhận.
                var pitTaxAmount = CalculatePitTax(taxableSalary, effectiveTaxBrackets);
                var netSalary = grossSalary - insuranceDeduction - pitTaxAmount;
                // netSalary có thể âm nếu dữ liệu đầu vào phát sinh khấu trừ lớn bất thường.
                // Hệ thống hiện chưa chặn net âm để không che giấu sai lệch dữ liệu nguồn.

                // 9) Cập nhật aggregate payroll theo quy ước làm tròn của hệ thống.
                payroll.Status = "draft";
                payroll.GrossSalary = RoundMoney(grossSalary);
                payroll.NetSalary = RoundMoney(netSalary);
                payroll.TaxableSalary = RoundMoney(taxableSalary);
                payroll.PitTaxAmount = RoundMoney(pitTaxAmount);
                payroll.InsuranceDeduction = RoundMoney(insuranceDeduction);
                payroll.WorkingDaysStandard = standardWorkdays;
                payroll.WorkingDaysActual = attendanceRows.Sum(a => (a.WorkingHours ?? 0m) / 8m);
                payroll.TotalAllowance = RoundMoney(totalAllowance);
                payroll.TotalAddition = RoundMoney(totalAddition);
                payroll.TotalDeduction = RoundMoney(totalDeduction);
                payroll.SalaryPolicyId = salaryPolicy?.PolicyId;
                payroll.DeductionPolicyId = deductionPolicy?.DeductionPolicyId;
                payroll.EmployeeTaxProfileId = employeeTaxProfile?.EmployeeTaxProfileId;
                payroll.UpdatedAt = DateTime.Now;
                // workingDaysActual quy đổi theo 8h/ngày công.

                // 10) Lưu payroll tổng và đồng bộ lại các payroll item đi kèm.
                // ReplacePayrollItemsAsync sẽ xóa item cũ và tạo item mới theo aggregate hiện tại.
                await _payrollRepository.UpdateAsync(payroll.PayrollId!.Value, payroll);
                await ReplacePayrollItemsAsync(payroll, allPayrollItems);
                affected++;
            }

            return affected;
        }

        /// <summary>
        /// Khóa toàn bộ payroll trong kỳ và ghi snapshot dữ liệu phục vụ truy vết lịch sử.
        /// </summary>
        public async Task<int> LockPayrollsAsync(Guid salaryPeriodId)
        {
            var period = await EnsureSalaryPeriodAsync(salaryPeriodId);
            if (!string.Equals(period.Status, "draft", StringComparison.OrdinalIgnoreCase))
                throw new ValidateException("Salary period invalid status", "Chỉ được khóa khi kỳ lương đang ở trạng thái draft");

            var payrolls = (await _payrollRepository.GetAllAsync())
                .Where(p => p.SalaryPeriodId == salaryPeriodId)
                .ToList();

            var allPayrollItems = await _payrollItemRepository.GetAllAsync();
            var allContracts = await _contractRepository.GetAllAsync();
            var salaryPolicies = await _salaryPolicyRepository.GetAllAsync();
            var deductionPolicies = await _deductionPolicyRepository.GetAllAsync();
            var taxProfiles = await _employeeTaxProfileRepository.GetAllAsync();
            var allSnapshots = await _payrollSnapshotRepository.GetAllAsync();

            if (payrolls.Count == 0)
                throw new ValidateException("Payroll not found", "Không có bảng lương để khóa trong kỳ đã chọn");

            var now = DateTime.Now;
            var affected = 0;
            foreach (var payroll in payrolls)
            {
                if (string.Equals(payroll.Status, "paid", StringComparison.OrdinalIgnoreCase))
                    throw new ValidateException("Payroll status invalid", "Không thể khóa bảng lương đã thanh toán");

                payroll.Status = "locked";
                payroll.LockedAt = now;
                payroll.UpdatedAt = now;
                await _payrollRepository.UpdateAsync(payroll.PayrollId!.Value, payroll);

                // Snapshot bao gồm payroll + items + contract/policy/tax profile để không bị lệch lịch sử.
                await UpsertPayrollSnapshotAsync(
                    payroll,
                    allPayrollItems,
                    allContracts,
                    salaryPolicies,
                    deductionPolicies,
                    taxProfiles,
                    allSnapshots,
                    now
                );
                affected++;
            }

            period.Status = "locked";
            period.UpdatedAt = now;
            await _salaryPeriodRepository.UpdateAsync(period.SalaryPeriodId!.Value, period);

            return affected;
        }

        /// <summary>
        /// Đánh dấu đã chi trả cho toàn bộ payroll thuộc kỳ đang locked.
        /// </summary>
        public async Task<int> MarkPayrollsPaidAsync(Guid salaryPeriodId)
        {
            var period = await EnsureSalaryPeriodAsync(salaryPeriodId);
            if (!string.Equals(period.Status, "locked", StringComparison.OrdinalIgnoreCase))
                throw new ValidateException("Salary period invalid status", "Chỉ được đánh dấu chi trả khi kỳ lương đang ở trạng thái locked");

            var payrolls = (await _payrollRepository.GetAllAsync())
                .Where(p => p.SalaryPeriodId == salaryPeriodId)
                .ToList();

            if (payrolls.Count == 0)
                throw new ValidateException("Payroll not found", "Không có bảng lương để chi trả trong kỳ đã chọn");

            var now = DateTime.Now;
            var affected = 0;
            foreach (var payroll in payrolls)
            {
                if (!string.Equals(payroll.Status, "locked", StringComparison.OrdinalIgnoreCase))
                    throw new ValidateException("Payroll status invalid", "Tất cả bảng lương phải ở trạng thái locked trước khi chi trả");

                payroll.Status = "paid";
                payroll.PaidAt = now;
                payroll.UpdatedAt = now;
                await _payrollRepository.UpdateAsync(payroll.PayrollId!.Value, payroll);
                affected++;
            }

            period.Status = "paid";
            period.UpdatedAt = now;
            await _salaryPeriodRepository.UpdateAsync(period.SalaryPeriodId!.Value, period);

            return affected;
        }

        protected override Task BeforeSaveAsync(Payroll entity, bool isUpdate = false)
        {
            if (!isUpdate)
                entity.CreatedAt = DateTime.Now;

            entity.UpdatedAt = DateTime.Now;
            entity.Status ??= "draft";
            return Task.CompletedTask;
        }

        protected override async Task ValidateAsync(Payroll entity, Guid? ignoreId = null)
        {
            if (entity == null)
                throw new ValidateException("Payroll object is null", "Dữ liệu bảng lương không được để trống");

            if (string.IsNullOrWhiteSpace(entity.PayrollCode))
                throw new ValidateException("PayrollCode required", "Mã bảng lương không được để trống");

            if (entity.SalaryPeriodId == null)
                throw new ValidateException("SalaryPeriodId required", "Kỳ lương không được để trống");

            if (entity.EmployeeId == null)
                throw new ValidateException("EmployeeId required", "Nhân viên không được để trống");

            if (!string.IsNullOrWhiteSpace(entity.Status) && !AllowedStatuses.Contains(entity.Status))
                throw new ValidateException("Status invalid", "Trạng thái bảng lương không hợp lệ");

            if ((entity.WorkingDaysActual ?? 0) < 0)
                throw new ValidateException("WorkingDaysActual invalid", "Số ngày công thực tế không hợp lệ");

            if ((entity.WorkingDaysStandard ?? 0) < 0)
                throw new ValidateException("WorkingDaysStandard invalid", "Số ngày công chuẩn không hợp lệ");

            if (await _payrollRepository.IsValueExistAsync(nameof(Payroll.PayrollCode), entity.PayrollCode, ignoreId))
                throw new ValidateException("PayrollCode duplicate", "Mã bảng lương đã tồn tại");
        }

        /// <summary>
        /// Kiểm tra kỳ lương có tồn tại theo ID trước khi thực hiện workflow payroll.
        /// </summary>
        /// <param name="salaryPeriodId">ID kỳ lương cần kiểm tra.</param>
        /// <returns>Đối tượng kỳ lương nếu tồn tại.</returns>
        /// <exception cref="NotFoundException">Ném ra khi không tìm thấy kỳ lương theo ID.</exception>
        private async Task<SalaryPeriod> EnsureSalaryPeriodAsync(Guid salaryPeriodId)
        {
            var period = await _salaryPeriodRepository.GetById(salaryPeriodId);
            if (period == null)
                throw new NotFoundException("Salary period not found", "Không tìm thấy kỳ lương");

            return period;
        }

        /// <summary>
        /// Làm tròn tiền theo quy ước hệ thống: 0 chữ số thập phân, midpoint away from zero.
        /// </summary>
        /// <param name="value">Giá trị tiền cần làm tròn.</param>
        /// <returns>Giá trị đã làm tròn.</returns>
        private static decimal RoundMoney(decimal value)
        {
            return Math.Round(value, 0, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Đếm số ngày công chuẩn (thứ 2 đến thứ 6) trong một khoảng thời gian.
        /// </summary>
        /// <param name="startDate">Ngày bắt đầu (bao gồm).</param>
        /// <param name="endDate">Ngày kết thúc (bao gồm).</param>
        /// <returns>Số ngày làm việc trong khoảng; trả về 0 nếu endDate nhỏ hơn startDate.</returns>
        private static int CountBusinessDays(DateTime startDate, DateTime endDate)
        {
            if (endDate < startDate)
                return 0;

            var count = 0;
            for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            {
                if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
                    count++;
            }

            return count;
        }

        /// <summary>
        /// Lấy chính sách lương hiệu lực tại một thời điểm.
        /// Ưu tiên bản ghi có EffectiveFrom gần nhất và còn trong khoảng hiệu lực.
        /// </summary>
        /// <param name="policies">Danh sách chính sách lương.</param>
        /// <param name="atDate">Thời điểm cần tra cứu hiệu lực.</param>
        /// <returns>Chính sách lương phù hợp; null nếu không tìm thấy.</returns>
        private static SalaryPolicy? GetEffectiveSalaryPolicy(List<SalaryPolicy> policies, DateTime atDate)
        {
            return policies
                .Where(p => p.IsActive == null || p.IsActive == true)
                .Where(p => p.EffectiveFrom != null && p.EffectiveFrom.Value.Date <= atDate)
                .Where(p => p.EffectiveTo == null || p.EffectiveTo.Value.Date >= atDate)
                .OrderByDescending(p => p.EffectiveFrom)
                .FirstOrDefault();
        }

        /// <summary>
        /// Lấy chính sách giảm trừ/bảo hiểm hiệu lực tại một thời điểm.
        /// Ưu tiên bản ghi có EffectiveFrom gần nhất và còn trong khoảng hiệu lực.
        /// </summary>
        /// <param name="policies">Danh sách chính sách giảm trừ/bảo hiểm.</param>
        /// <param name="atDate">Thời điểm cần tra cứu hiệu lực.</param>
        /// <returns>Chính sách giảm trừ phù hợp; null nếu không tìm thấy.</returns>
        private static DeductionPolicy? GetEffectiveDeductionPolicy(List<DeductionPolicy> policies, DateTime atDate)
        {
            return policies
                .Where(p => p.IsActive == null || p.IsActive == true)
                .Where(p => p.EffectiveFrom != null && p.EffectiveFrom.Value.Date <= atDate)
                .Where(p => p.EffectiveTo == null || p.EffectiveTo.Value.Date >= atDate)
                .OrderByDescending(p => p.EffectiveFrom)
                .FirstOrDefault();
        }

        /// <summary>
        /// Lấy danh sách bậc thuế hiệu lực tại một thời điểm và sắp theo cận dưới tăng dần.
        /// </summary>
        /// <param name="brackets">Danh sách bậc thuế.</param>
        /// <param name="atDate">Thời điểm cần tra cứu hiệu lực.</param>
        /// <returns>Danh sách bậc thuế đang hiệu lực tại thời điểm chỉ định.</returns>
        private static List<TaxBracket> GetEffectiveTaxBrackets(List<TaxBracket> brackets, DateTime atDate)
        {
            return brackets
                .Where(b => b.IsActive == null || b.IsActive == true)
                .Where(b => b.EffectiveFrom != null && b.EffectiveFrom.Value.Date <= atDate)
                .Where(b => b.EffectiveTo == null || b.EffectiveTo.Value.Date >= atDate)
                .OrderBy(b => b.LowerBound)
                .ToList();
        }

        /// <summary>
        /// Tính thuế TNCN theo bậc thuế phù hợp với thu nhập chịu thuế.
        /// Công thức: (taxable - lowerBound) * rate + quickDeduction.
        /// </summary>
        /// <param name="taxableSalary">Thu nhập chịu thuế.</param>
        /// <param name="effectiveTaxBrackets">Danh sách bậc thuế đang hiệu lực.</param>
        /// <returns>Số thuế TNCN phải nộp; luôn không âm.</returns>
        private static decimal CalculatePitTax(decimal taxableSalary, List<TaxBracket> effectiveBrackets)
        {
            if (taxableSalary <= 0 || effectiveBrackets.Count == 0)
                return 0;

            var bracket = effectiveBrackets
                .Where(b => (b.LowerBound ?? 0) <= taxableSalary)
                .Where(b => b.UpperBound == null || taxableSalary <= b.UpperBound.Value)
                .OrderByDescending(b => b.LowerBound)
                .FirstOrDefault();

            if (bracket == null)
                return 0;

            var lowerBound = bracket.LowerBound ?? 0;
            var rate = (bracket.TaxRate ?? 0) / 100m;
            var quickDeduction = bracket.QuickDeduction ?? 0;
            var pit = ((taxableSalary - lowerBound) * rate) + quickDeduction;
            return pit < 0 ? 0 : pit;
        }

        /// <summary>
        /// Tạo các đoạn hợp đồng giao với kỳ lương để phục vụ tính pro-rate.
        /// Mỗi đoạn biểu diễn một khoảng ngày liên tục ứng với một hợp đồng hiệu lực.
        /// </summary>
        /// <param name="contracts">Danh sách hợp đồng của nhân viên, đã sắp theo EffectiveDate tăng dần.</param>
        /// <param name="periodStart">Ngày bắt đầu kỳ lương.</param>
        /// <param name="periodEnd">Ngày kết thúc kỳ lương.</param>
        /// <returns>Danh sách đoạn hợp đồng hợp lệ trong kỳ.</returns>
        private static List<ContractSegment> BuildContractSegments(
            List<Contract> contracts,
            DateTime periodStart,
            DateTime periodEnd
        )
        {
            var segments = new List<ContractSegment>();
            if (contracts.Count == 0)
                return segments;

            for (var i = 0; i < contracts.Count; i++)
            {
                var contract = contracts[i];
                if (contract.EffectiveDate == null)
                    continue;

                var start = contract.EffectiveDate.Value.Date;
                if (start < periodStart)
                    start = periodStart;

                var end = contract.EndDate?.Date ?? periodEnd;
                if (end > periodEnd)
                    end = periodEnd;

                if (i < contracts.Count - 1)
                {
                    var nextStart = contracts[i + 1].EffectiveDate?.Date;
                    if (nextStart != null)
                    {
                        var candidateEnd = nextStart.Value.AddDays(-1);
                        if (candidateEnd < end)
                            end = candidateEnd;
                    }
                }

                if (end < start)
                    continue;

                segments.Add(new ContractSegment(contract, start, end));
            }

            return segments;
        }

        /// <summary>
        /// Đồng bộ lại toàn bộ payroll item theo kết quả aggregate mới nhất của payroll.
        /// Thực hiện xóa item cũ và tạo lại các item chuẩn (base, insurance, tax).
        /// </summary>
        /// <param name="payroll">Bảng lương tổng đã được tính toán.</param>
        /// <param name="allExistingItems">Danh sách payroll item hiện có trong hệ thống.</param>
        /// <returns>Task bất đồng bộ.</returns>
        private async Task ReplacePayrollItemsAsync(Payroll payroll, List<PayrollItem> allExistingItems)
        {
            if (payroll.PayrollId == null || payroll.PayrollCode == null)
                return;

            var existingItems = allExistingItems
                .Where(i => i.PayrollId == payroll.PayrollId)
                .Where(i => i.PayrollItemId != null)
                .ToList();

            foreach (var existingItem in existingItems)
            {
                await _payrollItemRepository.DeleteAsync(existingItem.PayrollItemId!.Value);
            }

            var items = new List<PayrollItem>
            {
                new()
                {
                    PayrollItemId = Guid.NewGuid(),
                    PayrollItemCode = BuildPayrollItemCode(payroll.PayrollCode, "BASE"),
                    PayrollId = payroll.PayrollId,
                    ItemType = "addition",
                    ItemName = "Lương cơ bản",
                    FormulaComponent = "base_salary",
                    Amount = RoundMoney(payroll.GrossSalary ?? 0m),
                    SourceTable = "manual",
                    SourceId = null,
                    Note = "Khoản lương tổng hợp theo kỳ",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                },
                new()
                {
                    PayrollItemId = Guid.NewGuid(),
                    PayrollItemCode = BuildPayrollItemCode(payroll.PayrollCode, "INS"),
                    PayrollId = payroll.PayrollId,
                    ItemType = "deduction",
                    ItemName = "Khấu trừ bảo hiểm",
                    FormulaComponent = "insurance",
                    Amount = RoundMoney(payroll.InsuranceDeduction ?? 0m),
                    SourceTable = "manual",
                    SourceId = null,
                    Note = "Khấu trừ bảo hiểm theo policy",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                },
                new()
                {
                    PayrollItemId = Guid.NewGuid(),
                    PayrollItemCode = BuildPayrollItemCode(payroll.PayrollCode, "TAX"),
                    PayrollId = payroll.PayrollId,
                    ItemType = "deduction",
                    ItemName = "Thuế thu nhập cá nhân",
                    FormulaComponent = "tax",
                    Amount = RoundMoney(payroll.PitTaxAmount ?? 0m),
                    SourceTable = "manual",
                    SourceId = null,
                    Note = "Thuế TNCN theo bậc lũy tiến",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                }
            };

            foreach (var item in items)
            {
                await _payrollItemRepository.InsertAsync(item);
            }
        }

        /// <summary>
        /// Sinh mã payroll item duy nhất theo mẫu PI-{payrollCode}-{suffix}-{random}.
        /// </summary>
        /// <param name="payrollCode">Mã payroll tổng.</param>
        /// <param name="suffix">Hậu tố nhóm khoản mục (ví dụ: BASE, INS, TAX).</param>
        /// <returns>Mã payroll item đã chuẩn hóa.</returns>
        private static string BuildPayrollItemCode(string payrollCode, string suffix)
        {
            return $"PI-{payrollCode}-{suffix}-{Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper()}";
        }

        /// <summary>
        /// Upsert snapshot cho một payroll tại thời điểm khóa kỳ.
        /// Nếu đã tồn tại snapshot cũ thì xóa trước khi ghi snapshot mới.
        /// </summary>
        /// <param name="payroll">Bảng lương cần snapshot.</param>
        /// <param name="allPayrollItems">Danh sách payroll item toàn hệ thống.</param>
        /// <param name="allContracts">Danh sách hợp đồng toàn hệ thống.</param>
        /// <param name="salaryPolicies">Danh sách chính sách lương.</param>
        /// <param name="deductionPolicies">Danh sách chính sách giảm trừ/bảo hiểm.</param>
        /// <param name="taxProfiles">Danh sách hồ sơ thuế nhân viên.</param>
        /// <param name="allSnapshots">Danh sách snapshot hiện có.</param>
        /// <param name="snapshotAt">Thời điểm ghi snapshot.</param>
        /// <returns>Task bất đồng bộ.</returns>
        private async Task UpsertPayrollSnapshotAsync(
            Payroll payroll,
            List<PayrollItem> allPayrollItems,
            List<Contract> allContracts,
            List<SalaryPolicy> salaryPolicies,
            List<DeductionPolicy> deductionPolicies,
            List<EmployeeTaxProfile> taxProfiles,
            List<PayrollSnapshot> allSnapshots,
            DateTime snapshotAt
        )
        {
            if (payroll.PayrollId == null)
                return;

            var existing = allSnapshots
                .FirstOrDefault(s => s.PayrollId == payroll.PayrollId && s.PayrollSnapshotId != null);

            if (existing?.PayrollSnapshotId != null)
            {
                await _payrollSnapshotRepository.DeleteAsync(existing.PayrollSnapshotId.Value);
            }

            var relatedItems = allPayrollItems.Where(i => i.PayrollId == payroll.PayrollId).ToList();
            var relatedContracts = allContracts.Where(c => c.EmployeeId == payroll.EmployeeId).ToList();
            var salaryPolicy = salaryPolicies.FirstOrDefault(p => p.PolicyId == payroll.SalaryPolicyId);
            var deductionPolicy = deductionPolicies.FirstOrDefault(p => p.DeductionPolicyId == payroll.DeductionPolicyId);
            var taxProfile = taxProfiles.FirstOrDefault(p => p.EmployeeTaxProfileId == payroll.EmployeeTaxProfileId);

            var snapshot = new PayrollSnapshot
            {
                PayrollSnapshotId = Guid.NewGuid(),
                PayrollId = payroll.PayrollId,
                SnapshotAt = snapshotAt,
                ContractPayload = JsonSerializer.Serialize(relatedContracts),
                PolicyPayload = JsonSerializer.Serialize(new { salaryPolicy, deductionPolicy }),
                TaxProfilePayload = JsonSerializer.Serialize(taxProfile),
                PayrollPayload = JsonSerializer.Serialize(payroll),
                ItemsPayload = JsonSerializer.Serialize(relatedItems),
                CreatedAt = snapshotAt,
            };

            await _payrollSnapshotRepository.InsertAsync(snapshot);
        }

        private record ContractSegment(Contract Contract, DateTime StartDate, DateTime EndDate);
    }
}