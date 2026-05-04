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
        private readonly IContractAllowanceRepository _contractAllowanceRepository;
        private readonly IAllowanceRepository _allowanceRepository;

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
            ITaxBracketRepository taxBracketRepository,
            IContractAllowanceRepository contractAllowanceRepository,
            IAllowanceRepository allowanceRepository
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
            _contractAllowanceRepository = contractAllowanceRepository;
            _allowanceRepository = allowanceRepository;
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

            var periodStart = period.StartDate.Value.Date;
            var periodEnd = period.EndDate.Value.Date;
            var employees = await _employeeRepository.GetAllAsync();
            var existingPayrolls = await _payrollRepository.GetBySalaryPeriodAsync(salaryPeriodId);

            var employeeIds = employees
                .Where(e => e.EmployeeId != null)
                .Select(e => e.EmployeeId!.Value)
                .Distinct()
                .ToList();

            var effectiveContracts = await _contractRepository.GetEffectiveByEmployeesAsync(employeeIds, periodStart, periodEnd);
            var eligibleEmployeeIds = effectiveContracts
                .Where(IsContractEligibleForPayroll)
                .Where(c => c.EmployeeId != null)
                .Select(c => c.EmployeeId!.Value)
                .ToHashSet();

            // Loại trừ nhân viên đã có payroll trong kỳ để đảm bảo idempotent khi generate nhiều lần.
            var existingEmployeeIds = existingPayrolls
                .Where(p => p.SalaryPeriodId == salaryPeriodId && p.EmployeeId != null)
                .Select(p => p.EmployeeId!.Value)
                .ToHashSet();

            var targetEmployees = employees
                .Where(e =>
                    e.EmployeeId != null
                    && eligibleEmployeeIds.Contains(e.EmployeeId.Value)
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
        ///
        /// Luồng xử lý:
        ///   0) Validate trạng thái kỳ lương (chỉ cho phép khi status = "draft").
        ///   1) Tải danh sách payroll cần tính trong kỳ (toàn bộ hoặc 1 nhân viên).
        ///   2) Batch-load dữ liệu nguồn (hợp đồng, chấm công, policy, thuế).
        ///   3) Chọn policy/bảng thuế hiệu lực tại ngày bắt đầu kỳ.
        ///   4–5) Với từng payroll: lọc hợp đồng/chấm công → chia đoạn pro-rate.
        ///   6) Tổng hợp phát sinh chấm công (bonus, penalty, overtime).
        ///   7) Tính bảo hiểm (BHXH + BHYT + BHTN) và thu nhập chịu thuế.
        ///   8) Tính thuế TNCN lũy tiến và lương thực nhận (net).
        ///   9–10) Cập nhật bản ghi payroll và đồng bộ lại payroll item.
        /// </summary>
        /// <param name="salaryPeriodId">
        ///   ID kỳ lương cần tính (bắt buộc).
        ///   Kỳ phải tồn tại và có status = "draft" thì mới được tính.
        /// </param>
        /// <param name="employeeId">
        ///   ID nhân viên tùy chọn.
        ///   Nếu null  → tính toàn bộ payroll trong kỳ.
        ///   Nếu có giá trị → chỉ tính payroll của nhân viên đó.
        /// </param>
        /// <returns>
        ///   Số payroll đã được tính thành công trong lần gọi này.
        ///   Payroll ở trạng thái locked/paid sẽ bị bỏ qua và không được đếm.
        /// </returns>
        public async Task<int> CalculatePayrollsAsync(Guid salaryPeriodId, Guid? employeeId = null)
        {
            // ----------------------------------------------------------------
            // BƯỚC 0: Validate kỳ lương đầu vào
            // ----------------------------------------------------------------
            // period: đối tượng SalaryPeriod lấy từ DB.
            // Ném NotFoundException nếu không tìm thấy, ValidateException nếu không đủ điều kiện.
            var period = await EnsureSalaryPeriodAsync(salaryPeriodId);

            if (!string.Equals(period.Status, "draft", StringComparison.OrdinalIgnoreCase))
                throw new ValidateException("Salary period invalid status", "Chỉ được tính lương khi kỳ lương đang ở trạng thái draft");

            if (period.StartDate == null || period.EndDate == null)
                throw new ValidateException("Salary period date invalid", "Kỳ lương thiếu ngày bắt đầu hoặc kết thúc");

            // periodStart / periodEnd: biên ngày tính lương (Date-only, bỏ phần giờ).
            // Dùng xuyên suốt toàn bộ logic lọc dữ liệu nguồn bên dưới.
            var periodStart = period.StartDate.Value.Date;
            var periodEnd = period.EndDate.Value.Date;

            // ----------------------------------------------------------------
            // BƯỚC 1: Lấy danh sách payroll mục tiêu trong kỳ
            // ----------------------------------------------------------------
            // payrolls: list Payroll cần tính; đã lọc theo employeeId nếu được truyền vào.
            var payrolls = await _payrollRepository.GetBySalaryPeriodAsync(salaryPeriodId, employeeId);

            if (payrolls.Count == 0)
                throw new ValidateException("Payroll not found", "Không có bảng lương nào để tính cho kỳ đã chọn");

            // employeeIds: distinct list EmployeeId → dùng để batch-load hợp đồng và chấm công.
            var employeeIds = payrolls
                .Where(p => p.EmployeeId != null)
                .Select(p => p.EmployeeId!.Value)
                .Distinct()
                .ToList();

            // payrollIds: distinct list PayrollId → dùng để tải payroll item hiện có cần xóa.
            var payrollIds = payrolls
                .Where(p => p.PayrollId != null)
                .Select(p => p.PayrollId!.Value)
                .Distinct()
                .ToList();

            // ----------------------------------------------------------------
            // BƯỚC 2: Batch-load dữ liệu nguồn (1 lần, tránh query lặp trong foreach)
            // ----------------------------------------------------------------
            // contracts      : hợp đồng có hiệu lực giao với kỳ → căn cứ tính lương cơ bản và BH.
            // attendances    : chấm công trong kỳ → bonus, penalty, overtime, giờ công thực tế.
            // salaryPolicy   : chính sách lương hiệu lực tại đầu kỳ.
            // deductionPolicy: chính sách giảm trừ/bảo hiểm hiệu lực tại đầu kỳ.
            // taxProfiles    : hồ sơ thuế hiệu lực theo từng nhân viên tại cuối kỳ.
            // effectiveTaxBrackets: biểu thuế lũy tiến hiệu lực tại đầu kỳ.
            // allPayrollItems: item hiện có của các payroll → dùng để xóa trước khi tạo lại.
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
            var salaryPolicy = await _salaryPolicyRepository.GetEffectiveAtAsync(periodStart);
            var deductionPolicy = await _deductionPolicyRepository.GetEffectiveAtAsync(periodStart);
            var taxProfiles = await _employeeTaxProfileRepository.GetEffectiveByEmployeesAsync(employeeIds, periodEnd);
            var effectiveTaxBrackets = await _taxBracketRepository.GetEffectiveAtAsync(periodStart);
            var allPayrollItems = await _payrollItemRepository.GetByPayrollIdsAsync(payrollIds);
            
            // Tải phụ cấp của các hợp đồng
            var contractIds = contracts
                .Where(c => c.ContractId != null)
                .Select(c => c.ContractId!.Value)
                .Distinct()
                .ToList();
            var contractAllowances = contractIds.Count > 0 
                ? await _contractAllowanceRepository.GetByContractIdsAsync(contractIds)
                : new List<ContractAllowance>();
            
            // Tải thông tin chi tiết phụ cấp
            var allowanceIds = contractAllowances
                .Where(ca => ca.AllowanceId != null)
                .Select(ca => ca.AllowanceId!.Value)
                .Distinct()
                .ToList();
            var allowances = allowanceIds.Count > 0
                ? await _allowanceRepository.GetByIdsAsync(allowanceIds)
                : new List<Allowance>();
            
            var allowanceByIdMap = allowances
                .Where(a => a.AllowanceId != null)
                .ToDictionary(a => a.AllowanceId!.Value);

            // ----------------------------------------------------------------
            // BƯỚC 3: Chọn policy/bảng thuế hiệu lực tại ngày bắt đầu kỳ
            // ----------------------------------------------------------------
            // Quy ước: áp dụng cấu hình tại thời điểm periodStart (snapshot đầu tháng).
            // salaryPolicy      : cấu hình số ngày công chuẩn và hệ số làm thêm giờ.
            // deductionPolicy   : tỷ lệ BHXH/BHYT/BHTN và mức giảm trừ gia cảnh/bản thân.
            // effectiveTaxBrackets: danh sách bậc thuế đang hiệu lực, đã sắp theo lowerBound tăng dần.

            // affected: đếm số payroll được tính thành công trong lần gọi này.
            var affected = 0;

            foreach (var payroll in payrolls)
            {
                // ----------------------------------------------------------------
                // BƯỚC 4: Bỏ qua payroll đã đóng trạng thái nghiệp vụ
                // ----------------------------------------------------------------
                // locked / paid → đã được chốt; không được tính lại để bảo toàn dữ liệu.
                if (string.Equals(payroll.Status, "locked", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(payroll.Status, "paid", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (payroll.EmployeeId == null)
                    continue;

                // employeeContracts: hợp đồng của nhân viên này giao với kỳ lương và có status hợp lệ.
                // Điều kiện giao kỳ: EffectiveDate <= periodEnd  AND  (EndDate == null OR EndDate >= periodStart).
                // Status được chấp nhận: active, signed, draft (null/empty cũng được).
                // Hợp đồng sắp theo EffectiveDate tăng dần → cần thiết cho BuildContractSegments.
                var employeeContracts = contracts
                    .Where(c => c.EmployeeId == payroll.EmployeeId)
                    .Where(c => c.EffectiveDate != null)
                    .Where(c => c.EffectiveDate!.Value.Date <= periodEnd)
                    .Where(c => c.EndDate == null || c.EndDate.Value.Date >= periodStart)
                    .Where(IsContractEligibleForPayroll)
                    .OrderBy(c => c.EffectiveDate)
                    .ToList();

                // Không có hợp đồng hiệu lực trong kỳ → không có căn cứ tính lương, bỏ qua.
                if (employeeContracts.Count == 0)
                    continue;

                // attendanceRows: bản ghi chấm công của nhân viên trong kỳ lương.
                // Có thể rỗng → khi đó bonus/penalty/overtime/giờ công thực tế đều = 0.
                var attendanceRows = attendances
                    .Where(a => a.EmployeeId == payroll.EmployeeId)
                    .Where(a => a.AttendanceDate != null)
                    .Where(a => a.AttendanceDate!.Value.Date >= periodStart && a.AttendanceDate.Value.Date <= periodEnd)
                    .ToList();

                // ----------------------------------------------------------------
                // BƯỚC 5: Chia kỳ lương thành đoạn hợp đồng (pro-rate)
                // ----------------------------------------------------------------
                // contractSegments: mỗi đoạn là khoảng ngày liên tục ứng với 1 hợp đồng cụ thể.
                // Xử lý trường hợp nhân viên đổi hợp đồng giữa tháng:
                //   ví dụ hợp đồng A: 01/04–14/04, hợp đồng B: 15/04–30/04 → 2 đoạn riêng biệt.
                var contractSegments = BuildContractSegments(employeeContracts, periodStart, periodEnd);
                if (contractSegments.Count == 0)
                    continue;

                // standardWorkdays: số ngày công chuẩn của kỳ lương (mẫu số pro-rate).
                //   - Lấy từ salaryPolicy nếu có.
                //   - Fallback: đếm ngày Mon–Fri trong kỳ.
                //   - Fallback cuối: 22 ngày (mặc định phổ biến).
                var standardWorkdays = salaryPolicy?.StandardWorkdays ?? CountBusinessDays(periodStart, periodEnd);
                if (standardWorkdays <= 0)
                    standardWorkdays = 22;

                // baseSalaryAmount      : lương cơ bản pro-rate cộng dồn qua các đoạn hợp đồng (VND).
                // insuranceSalaryBase   : lương căn cứ tính bảo hiểm pro-rate cộng dồn (VND).
                //                         Tách khỏi baseSalaryAmount vì hợp đồng có thể khai InsuranceSalary riêng.
                // contractBusinessDaysTotal: tổng số ngày công chuẩn thực tế theo hợp đồng hiệu lực.
                //                            Dùng làm mẫu số tính đơn giá giờ overtime (bước 6).
                decimal baseSalaryAmount = 0;
                decimal insuranceSalaryBase = 0;
                decimal contractBusinessDaysTotal = 0;
                decimal totalAllowanceAmount = 0;

                foreach (var segment in contractSegments)
                {
                    // segmentBusinessDays: số ngày công chuẩn (Mon–Fri) trong đoạn hợp đồng này.
                    var segmentBusinessDays = CountBusinessDays(segment.StartDate, segment.EndDate);
                    contractBusinessDaysTotal += segmentBusinessDays;

                    // salaryRatio: hệ số lương của hợp đồng (ví dụ 80 → 0.8).
                    //   Mặc định 100 (= 1.0) nếu hợp đồng không khai.
                    // segmentDailySalary: đơn giá ngày = baseSalary * salaryRatio / standardWorkdays.
                    // baseSalaryAmount  : cộng dồn lương ngày * số ngày trong đoạn → lương cơ bản kỳ.
                    var salaryRatio = (segment.Contract.SalaryRatio ?? 100m) / 100m;
                    var segmentDailySalary = (segment.Contract.BaseSalary ?? 0m) * salaryRatio / standardWorkdays;
                    baseSalaryAmount += segmentDailySalary * segmentBusinessDays;

                    // insuranceDaily: đơn giá ngày bảo hiểm = InsuranceSalary / standardWorkdays.
                    // insuranceSalaryBase: cộng dồn theo cùng logic pro-rate.
                    var insuranceDaily = (segment.Contract.InsuranceSalary ?? 0m) / standardWorkdays;
                    insuranceSalaryBase += insuranceDaily * segmentBusinessDays;

                    // Tính phụ cấp cho đoạn hợp đồng này
                    if (segment.Contract.ContractId != null)
                    {
                        var segmentAllowances = contractAllowances
                            .Where(ca => ca.ContractId == segment.Contract.ContractId)
                            .ToList();

                        foreach (var ca in segmentAllowances)
                        {
                            if (ca.AllowanceId != null && allowanceByIdMap.TryGetValue(ca.AllowanceId.Value, out var allowance))
                            {
                                decimal segmentAllowanceAmount = 0;
                                if (allowance.CalculationType == "FIXED")
                                {
                                    // Phụ cấp cố định: cộng toàn bộ
                                    segmentAllowanceAmount = allowance.Amount ?? 0m;
                                }
                                else if (allowance.CalculationType == "PERCENT")
                                {
                                    // Phụ cấp theo % lương: tính dựa trên lương cơ bản của đoạn
                                    var percentAllowance = segmentDailySalary * segmentBusinessDays * ((allowance.Percent ?? 0m) / 100m);
                                    segmentAllowanceAmount = percentAllowance;
                                }
                                totalAllowanceAmount += segmentAllowanceAmount;
                            }
                        }
                    }
                }

                // ----------------------------------------------------------------
                // BƯỚC 6: Tổng hợp phát sinh chấm công (bonus, penalty, overtime)
                // ----------------------------------------------------------------
                // totalAddition  : tổng phát sinh cộng từ chấm công (thưởng).
                //                  Sẽ được cộng thêm overtimeAddition ở cuối bước này.
                // totalDeduction : tổng phát sinh trừ từ chấm công (phạt).
                // overtimeHours  : tổng giờ làm thêm trong kỳ.
                var hourlySalary = contractBusinessDaysTotal > 0 ? baseSalaryAmount / (contractBusinessDaysTotal * 8m) : 0m;
                var overtimeMultiplier = salaryPolicy?.OvertimeMultiplierWeekday ?? 1.5m;
                var attendanceBreakdowns = attendanceRows
                    .Where(a => a.AttendanceId != null)
                    .Select(a =>
                    {
                        var bonusAmount = a.BonusAmount ?? 0m;
                        var penaltyAmount = a.PenaltyAmount ?? 0m;
                        var overtimeAmount = (a.OvertimeHours ?? 0m) * hourlySalary * overtimeMultiplier;
                        return new AttendanceBreakdown(a.AttendanceId!.Value, bonusAmount, penaltyAmount, overtimeAmount);
                    })
                    .ToList();

                var overtimeAddition = attendanceBreakdowns.Sum(a => a.OvertimeAmount);
                var totalAddition = attendanceBreakdowns.Sum(a => a.BonusAmount) + overtimeAddition;
                var totalDeduction = attendanceBreakdowns.Sum(a => a.PenaltyAmount);

                // hourlySalary: đơn giá giờ = baseSalaryAmount / (số ngày hợp đồng thực tế * 8h/ngày).
                //   Dùng contractBusinessDaysTotal (ngày thực tế hiệu lực) thay vì standardWorkdays
                //   để tránh làm giảm đơn giá overtime khi nhân viên mới vào hoặc nghỉ giữa tháng.
                // overtimeAddition: tiền làm thêm = giờ OT * đơn giá giờ * hệ số OT ngày thường.
                //   OvertimeMultiplierWeekday mặc định 1.5 nếu policy không khai.

                // totalAllowance: phụ cấp cố định + theo % lương được tính từ contract_allowance.
                // grossSalary   : lương gộp = baseSalary + allowance + addition - deduction.
                var totalAllowance = totalAllowanceAmount;
                var grossSalary = baseSalaryAmount + totalAllowance + totalAddition - totalDeduction;

                // ----------------------------------------------------------------
                // BƯỚC 7: Tính bảo hiểm và thu nhập chịu thuế (taxable salary)
                // ----------------------------------------------------------------
                // insuranceRate     : tỷ lệ BH tổng cộng = (BHXH% + BHYT% + BHTN%) / 100.
                // insuranceDeduction: số tiền BH nhân viên phải đóng = insuranceSalaryBase * insuranceRate.
                var insuranceRate = ((deductionPolicy?.SocialInsuranceRate ?? 0m)
                    + (deductionPolicy?.HealthInsuranceRate ?? 0m)
                    + (deductionPolicy?.UnemploymentInsuranceRate ?? 0m)) / 100m;
                var insuranceDeduction = insuranceSalaryBase * insuranceRate;

                // employeeTaxProfile: hồ sơ thuế hiệu lực tại cuối kỳ (phản ánh cấu hình gần nhất).
                //   Nếu null → dependentCount mặc định = 0 (không có người phụ thuộc).
                var employeeTaxProfile = taxProfiles
                    .Where(p => p.EmployeeId == payroll.EmployeeId)
                    .OrderByDescending(p => p.EffectiveFrom)
                    .FirstOrDefault();

                // personalDeduction : mức giảm trừ bản thân (số tiền cố định/tháng, từ deductionPolicy).
                // dependentDeduction: mức giảm trừ gia cảnh = mức/người * số người phụ thuộc.
                // taxableSalary     : thu nhập chịu thuế = gross - BH - giảm trừ bản thân - giảm trừ gia cảnh.
                //                     Luôn >= 0 để tránh thuế âm (Math.Max bảo vệ trường hợp lương nhỏ).
                var personalDeduction = deductionPolicy?.PersonalDeductionAmount ?? 0m;
                var dependentDeduction = (deductionPolicy?.DependentDeductionAmount ?? 0m) * (employeeTaxProfile?.DependentCount ?? 0);
                var taxableSalary = Math.Max(0m, grossSalary - insuranceDeduction - personalDeduction - dependentDeduction);

                // ----------------------------------------------------------------
                // BƯỚC 8: Tính thuế TNCN lũy tiến và lương thực nhận
                // ----------------------------------------------------------------
                // pitTaxAmount: thuế TNCN = tra bảng bậc thuế lũy tiến theo taxableSalary.
                //               Công thức: (taxable - lowerBound) * rate + quickDeduction.
                // netSalary   : lương thực nhận = gross - BH - thuế TNCN.
                //               Có thể âm nếu dữ liệu phát sinh khấu trừ bất thường;
                //               hệ thống không chặn net âm để không che giấu sai lệch nguồn.
                var pitTaxAmount = CalculatePitTax(taxableSalary, effectiveTaxBrackets);
                var netSalary = grossSalary - insuranceDeduction - pitTaxAmount;

                // ----------------------------------------------------------------
                // BƯỚC 9: Cập nhật aggregate payroll
                // ----------------------------------------------------------------
                // Tất cả giá trị tiền được làm tròn về số nguyên (RoundMoney, AwayFromZero).
                // WorkingDaysActual: số ngày công thực tế = tổng giờ công chấm công / 8h.
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

                // ----------------------------------------------------------------
                // BƯỚC 10: Lưu vào DB và đồng bộ lại payroll item
                // ----------------------------------------------------------------
                // UpdateAsync     : lưu lại bản ghi payroll tổng hợp vừa tính.
                // ReplacePayrollItemsAsync: xóa item cũ và tạo lại item chi tiết theo từng nguồn phát sinh.
                await _payrollRepository.UpdateAsync(payroll.PayrollId!.Value, payroll);
                await ReplacePayrollItemsAsync(
                    payroll,
                    allPayrollItems,
                    baseSalaryAmount,
                    totalAllowance,
                    insuranceDeduction,
                    pitTaxAmount,
                    attendanceBreakdowns
                );
                affected++;
            }

            // Trả về số payroll đã tính thành công (payroll locked/paid không được đếm).
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

            if (period.StartDate == null || period.EndDate == null)
                throw new ValidateException("Salary period date invalid", "Kỳ lương thiếu ngày bắt đầu hoặc kết thúc");

            var payrolls = await _payrollRepository.GetBySalaryPeriodAsync(salaryPeriodId);

            var payrollIds = payrolls
                .Where(p => p.PayrollId != null)
                .Select(p => p.PayrollId!.Value)
                .Distinct()
                .ToList();

            var employeeIds = payrolls
                .Where(p => p.EmployeeId != null)
                .Select(p => p.EmployeeId!.Value)
                .Distinct()
                .ToList();

            var allPayrollItems = await _payrollItemRepository.GetByPayrollIdsAsync(payrollIds);
            var allContracts = await _contractRepository.GetEffectiveByEmployeesAsync(
                employeeIds,
                period.StartDate.Value.Date,
                period.EndDate.Value.Date
            );
            var salaryPolicy = await _salaryPolicyRepository.GetEffectiveAtAsync(period.StartDate.Value.Date);
            var deductionPolicy = await _deductionPolicyRepository.GetEffectiveAtAsync(period.StartDate.Value.Date);
            var taxProfiles = await _employeeTaxProfileRepository.GetEffectiveByEmployeesAsync(employeeIds, period.EndDate.Value.Date);
            var allSnapshots = await _payrollSnapshotRepository.GetByPayrollIdsAsync(payrollIds);

            var taxProfileByEmployee = taxProfiles
                .Where(p => p.EmployeeId != null)
                .GroupBy(p => p.EmployeeId!.Value)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.EffectiveFrom).First());

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
                    salaryPolicy,
                    deductionPolicy,
                    taxProfileByEmployee,
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

            var payrolls = await _payrollRepository.GetBySalaryPeriodAsync(salaryPeriodId);

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

        /// <summary>
        /// Cập nhật bảng lương theo ID, chặn chỉnh sửa khi bảng lương đã khóa hoặc đã chi trả.
        /// </summary>
        /// <param name="id">ID bảng lương cần cập nhật.</param>
        /// <param name="entity">Dữ liệu bảng lương cập nhật.</param>
        /// <returns>ID bảng lương đã cập nhật.</returns>
        public override async Task<Guid> UpdateAsync(Guid id, Payroll entity)
        {
            await EnsurePayrollEditableAsync(id);
            return await base.UpdateAsync(id, entity);
        }

        /// <summary>
        /// Xóa bảng lương theo ID, chặn xóa khi bảng lương đã khóa hoặc đã chi trả.
        /// </summary>
        /// <param name="id">ID bảng lương cần xóa.</param>
        /// <returns>Số bản ghi đã xóa.</returns>
        public override async Task<int> DeleteAsync(Guid id)
        {
            await EnsurePayrollEditableAsync(id);
            return await base.DeleteAsync(id);
        }

        /// <summary>
        /// Thiết lập metadata mặc định trước khi lưu bảng lương.
        /// </summary>
        /// <param name="entity">Đối tượng bảng lương cần lưu.</param>
        /// <param name="isUpdate">Xác định thao tác hiện tại là cập nhật hay tạo mới.</param>
        /// <returns>Task bất đồng bộ hoàn thành ngay sau khi gán metadata.</returns>
        protected override Task BeforeSaveAsync(Payroll entity, bool isUpdate = false)
        {
            if (!isUpdate)
                entity.CreatedAt = DateTime.Now;

            entity.UpdatedAt = DateTime.Now;
            entity.Status ??= "draft";
            return Task.CompletedTask;
        }

        /// <summary>
        /// Kiểm tra tính hợp lệ dữ liệu bảng lương trước khi ghi xuống cơ sở dữ liệu.
        /// </summary>
        /// <param name="entity">Đối tượng bảng lương cần kiểm tra.</param>
        /// <param name="ignoreId">ID cần bỏ qua khi kiểm tra trùng mã trong trường hợp cập nhật.</param>
        /// <returns>Task bất đồng bộ hoàn thành khi dữ liệu hợp lệ.</returns>
        /// <exception cref="ValidateException">Ném ra khi dữ liệu đầu vào không hợp lệ hoặc vi phạm ràng buộc nghiệp vụ.</exception>
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
        /// Kiểm tra bảng lương có thể chỉnh sửa/xóa hay không.
        /// </summary>
        /// <param name="payrollId">ID bảng lương cần kiểm tra.</param>
        /// <returns>Task hoàn thành khi bảng lương còn editable.</returns>
        private async Task EnsurePayrollEditableAsync(Guid payrollId)
        {
            var current = await _payrollRepository.GetById(payrollId);
            if (current == null)
                throw new NotFoundException("Payroll not found", "Không tìm thấy bảng lương");

            if (string.Equals(current.Status, "locked", StringComparison.OrdinalIgnoreCase)
                || string.Equals(current.Status, "paid", StringComparison.OrdinalIgnoreCase))
            {
                throw new ValidateException("Payroll status invalid", "Bảng lương đã khóa hoặc đã chi trả, không được phép chỉnh sửa");
            }
        }

        /// <summary>
        /// Kiểm tra hợp đồng có đủ điều kiện tham gia tính lương hay không.
        /// Điều kiện: đã ký (is_signed=true), có signed_at và contract_status=active.
        /// </summary>
        /// <param name="contract">Hợp đồng cần kiểm tra.</param>
        /// <returns>True nếu đủ điều kiện tính lương, ngược lại False.</returns>
        private static bool IsContractEligibleForPayroll(Contract contract)
        {
            return contract.IsSigned == true
                && contract.SignedAt != null
                && (string.Equals(contract.ContractStatus, "active", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(contract.ContractStatus, "signed", StringComparison.OrdinalIgnoreCase));
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
        private async Task ReplacePayrollItemsAsync(
            Payroll payroll,
            List<PayrollItem> allExistingItems,
            decimal baseSalaryAmount,
            decimal totalAllowance,
            decimal insuranceDeduction,
            decimal pitTaxAmount,
            List<AttendanceBreakdown> attendanceBreakdowns
        )
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
                    Amount = RoundMoney(baseSalaryAmount),
                    SourceTable = "manual",
                    SourceId = null,
                    Note = "Lương cơ bản theo hợp đồng hiệu lực",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                },
            };

            if (totalAllowance > 0)
            {
                items.Add(new PayrollItem
                {
                    PayrollItemId = Guid.NewGuid(),
                    PayrollItemCode = BuildPayrollItemCode(payroll.PayrollCode, "ALLW"),
                    PayrollId = payroll.PayrollId,
                    ItemType = "addition",
                    ItemName = "Phụ cấp cố định",
                    FormulaComponent = "allowance",
                    Amount = RoundMoney(totalAllowance),
                    SourceTable = "manual",
                    SourceId = null,
                    Note = "Phụ cấp áp dụng theo kỳ",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                });
            }

            foreach (var attendance in attendanceBreakdowns)
            {
                if (attendance.BonusAmount > 0)
                {
                    items.Add(new PayrollItem
                    {
                        PayrollItemId = Guid.NewGuid(),
                        PayrollItemCode = BuildPayrollItemCode(payroll.PayrollCode, "BONUS"),
                        PayrollId = payroll.PayrollId,
                        ItemType = "addition",
                        ItemName = "Thưởng chấm công",
                        FormulaComponent = "bonus",
                        Amount = RoundMoney(attendance.BonusAmount),
                        SourceTable = "attendance",
                        SourceId = attendance.AttendanceId,
                        Note = "Khoản cộng phát sinh từ chấm công",
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                    });
                }

                if (attendance.OvertimeAmount > 0)
                {
                    items.Add(new PayrollItem
                    {
                        PayrollItemId = Guid.NewGuid(),
                        PayrollItemCode = BuildPayrollItemCode(payroll.PayrollCode, "OT"),
                        PayrollId = payroll.PayrollId,
                        ItemType = "addition",
                        ItemName = "Tiền làm thêm giờ",
                        FormulaComponent = "other",
                        Amount = RoundMoney(attendance.OvertimeAmount),
                        SourceTable = "attendance",
                        SourceId = attendance.AttendanceId,
                        Note = "Khoản cộng từ giờ làm thêm",
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                    });
                }

                if (attendance.PenaltyAmount > 0)
                {
                    items.Add(new PayrollItem
                    {
                        PayrollItemId = Guid.NewGuid(),
                        PayrollItemCode = BuildPayrollItemCode(payroll.PayrollCode, "PEN"),
                        PayrollId = payroll.PayrollId,
                        ItemType = "deduction",
                        ItemName = "Phạt chấm công",
                        FormulaComponent = "penalty",
                        Amount = RoundMoney(attendance.PenaltyAmount),
                        SourceTable = "attendance",
                        SourceId = attendance.AttendanceId,
                        Note = "Khoản trừ phát sinh từ chấm công",
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                    });
                }
            }

            items.Add(new PayrollItem
            {
                PayrollItemId = Guid.NewGuid(),
                PayrollItemCode = BuildPayrollItemCode(payroll.PayrollCode, "INS"),
                PayrollId = payroll.PayrollId,
                ItemType = "deduction",
                ItemName = "Khấu trừ bảo hiểm",
                FormulaComponent = "insurance",
                Amount = RoundMoney(insuranceDeduction),
                SourceTable = "manual",
                SourceId = null,
                Note = "Khấu trừ bảo hiểm theo policy",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
            });

            items.Add(new PayrollItem
            {
                PayrollItemId = Guid.NewGuid(),
                PayrollItemCode = BuildPayrollItemCode(payroll.PayrollCode, "TAX"),
                PayrollId = payroll.PayrollId,
                ItemType = "deduction",
                ItemName = "Thuế thu nhập cá nhân",
                FormulaComponent = "tax",
                Amount = RoundMoney(pitTaxAmount),
                SourceTable = "manual",
                SourceId = null,
                Note = "Thuế TNCN theo bậc lũy tiến",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
            });

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
            SalaryPolicy? salaryPolicy,
            DeductionPolicy? deductionPolicy,
            Dictionary<Guid, EmployeeTaxProfile> taxProfiles,
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
            var taxProfile = payroll.EmployeeId != null && taxProfiles.TryGetValue(payroll.EmployeeId.Value, out var profile)
                ? profile
                : null;

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

        private record AttendanceBreakdown(Guid AttendanceId, decimal BonusAmount, decimal PenaltyAmount, decimal OvertimeAmount);

        private record ContractSegment(Contract Contract, DateTime StartDate, DateTime EndDate);
    }
}