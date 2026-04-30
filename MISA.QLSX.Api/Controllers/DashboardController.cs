using Dapper;
using Microsoft.AspNetCore.Mvc;
using MISA.QLSX.Core.DTOs.Responses;
using MISA.QLSX.Infrastructure.Connection;

namespace MISA.QLSX.Api.Controllers
{
    /// <summary>
    /// Controller cho trang tổng quan (Dashboard).
    /// Trả về các chỉ số tổng hợp HR: tổng nhân sự, tổng lương, công tác, hợp đồng, ...
    /// </summary>
    [Route("api/v1/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly MySqlConnectionFactory _factory;

        public DashboardController(MySqlConnectionFactory factory)
        {
            _factory = factory;
        }

        /// <summary>
        /// Lấy dữ liệu tổng quan theo khoảng thời gian
        /// </summary>
        /// <param name="period">Khoảng thời gian: week, month, year (mặc định: month)</param>
        /// <returns>DashboardResponse chứa các chỉ số tổng hợp</returns>
        [HttpGet]
        public async Task<IActionResult> GetDashboard([FromQuery] string period = "month")
        {
            using var conn = _factory.CreateConnection();

            // Xác định khoảng thời gian lọc
            var now = DateTime.Now;
            DateTime periodStart;
            switch (period.ToLower())
            {
                case "week":
                    // Lấy ngày đầu tuần (thứ 2)
                    int diff = (7 + (now.DayOfWeek - DayOfWeek.Monday)) % 7;
                    periodStart = now.AddDays(-diff).Date;
                    break;
                case "year":
                    periodStart = new DateTime(now.Year, 1, 1);
                    break;
                case "month":
                default:
                    periodStart = new DateTime(now.Year, now.Month, 1);
                    break;
            }

            var param = new { PeriodStart = periodStart, Now = now, Next30Days = now.AddDays(30) };

            // 1. Tổng nhân viên
            var totalEmployees = await conn.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM employee", param
            );

            // 2. Tổng chi lương (net_salary) trong kỳ
            var totalSalary = await conn.ExecuteScalarAsync<decimal?>(
                @"SELECT COALESCE(SUM(p.net_salary), 0)
                  FROM payroll p
                  JOIN salary_period sp ON p.salary_period_id = sp.salary_period_id
                  WHERE sp.start_date >= @PeriodStart AND sp.start_date <= @Now",
                param
            ) ?? 0;

            // 3. Người đang đi công tác (có chuyến công tác trùng ngày hiện tại)
            var onBusinessTrip = await conn.ExecuteScalarAsync<int>(
                @"SELECT COUNT(DISTINCT bt.employee_id)
                  FROM business_trip bt
                  WHERE bt.start_date <= @Now AND bt.end_date >= @Now",
                param
            );

            // 4. Hợp đồng sắp hết hạn (trong 30 ngày tới)
            var contractsExpiring = await conn.ExecuteScalarAsync<int>(
                @"SELECT COUNT(*)
                  FROM contract c
                  WHERE c.end_date IS NOT NULL
                    AND c.end_date BETWEEN @Now AND @Next30Days
                    AND (c.contract_status IS NULL OR c.contract_status != 'TERMINATED')",
                param
            );

            // 5. Nhân viên chưa có hợp đồng
            var withoutContract = await conn.ExecuteScalarAsync<int>(
                @"SELECT COUNT(*)
                  FROM employee e
                  WHERE e.contract_id IS NULL",
                param
            );

            // 6. Hợp đồng chưa ký
            var unsignedContracts = await conn.ExecuteScalarAsync<int>(
                @"SELECT COUNT(*)
                  FROM contract c
                  WHERE c.is_signed = 0 OR c.is_signed IS NULL",
                param
            );

            // 7. Nhân viên có nguy cơ nghỉ việc (bị kỷ luật trong kỳ)
            var atRisk = await conn.ExecuteScalarAsync<int>(
                @"SELECT COUNT(DISTINCT ev.employee_id)
                  FROM evaluation ev
                  WHERE ev.evaluation_type = 'KỶ LUẬT'
                    AND ev.evaluation_date >= @PeriodStart",
                param
            );

            // 8. Nhân viên mới trong kỳ
            var newEmployees = await conn.ExecuteScalarAsync<int>(
                @"SELECT COUNT(*)
                  FROM employee e
                  WHERE e.join_date >= @PeriodStart",
                param
            );

            // === Danh sách chi tiết ===

            // Danh sách hợp đồng sắp hết hạn
            var expiringList = (await conn.QueryAsync<ExpiringContractItem>(
                @"SELECT
                    c.contract_id AS ContractId,
                    c.contract_code AS ContractCode,
                    e.full_name AS EmployeeName,
                    e.employee_code AS EmployeeCode,
                    c.end_date AS EndDate,
                    DATEDIFF(c.end_date, @Now) AS DaysRemaining
                  FROM contract c
                  LEFT JOIN employee e ON c.employee_id = e.employee_id
                  WHERE c.end_date IS NOT NULL
                    AND c.end_date BETWEEN @Now AND @Next30Days
                    AND (c.contract_status IS NULL OR c.contract_status != 'TERMINATED')
                  ORDER BY c.end_date ASC
                  LIMIT 10",
                param
            )).ToList();

            // Danh sách nhân viên chưa có hợp đồng
            var noContractList = (await conn.QueryAsync<EmployeeSimpleItem>(
                @"SELECT
                    e.employee_id AS EmployeeId,
                    e.employee_code AS EmployeeCode,
                    e.full_name AS FullName,
                    e.join_date AS JoinDate
                  FROM employee e
                  WHERE e.contract_id IS NULL
                  ORDER BY e.join_date DESC
                  LIMIT 10",
                param
            )).ToList();

            // Danh sách hợp đồng chưa ký
            var unsignedList = (await conn.QueryAsync<UnsignedContractItem>(
                @"SELECT
                    c.contract_id AS ContractId,
                    c.contract_code AS ContractCode,
                    e.full_name AS EmployeeName,
                    e.employee_code AS EmployeeCode,
                    c.effective_date AS EffectiveDate
                  FROM contract c
                  LEFT JOIN employee e ON c.employee_id = e.employee_id
                  WHERE c.is_signed = 0 OR c.is_signed IS NULL
                  ORDER BY c.effective_date DESC
                  LIMIT 10",
                param
            )).ToList();

            // Danh sách nhân viên có nguy cơ nghỉ việc
            var atRiskList = (await conn.QueryAsync<AtRiskEmployeeItem>(
                @"SELECT
                    ev.employee_id AS EmployeeId,
                    e.employee_code AS EmployeeCode,
                    e.full_name AS FullName,
                    ev.reason AS Reason,
                    ev.amount AS Amount,
                    ev.evaluation_date AS EvaluationDate
                  FROM evaluation ev
                  LEFT JOIN employee e ON ev.employee_id = e.employee_id
                  WHERE ev.evaluation_type = 'KỶ LUẬT'
                    AND ev.evaluation_date >= @PeriodStart
                  ORDER BY ev.evaluation_date DESC
                  LIMIT 10",
                param
            )).ToList();

            // Danh sách nhân viên đang đi công tác
            var tripList = (await conn.QueryAsync<BusinessTripItem>(
                @"SELECT
                    bt.business_trip_id AS BusinessTripId,
                    e.full_name AS EmployeeName,
                    e.employee_code AS EmployeeCode,
                    bt.location AS Location,
                    bt.start_date AS StartDate,
                    bt.end_date AS EndDate
                  FROM business_trip bt
                  LEFT JOIN employee e ON bt.employee_id = e.employee_id
                  WHERE bt.start_date <= @Now AND bt.end_date >= @Now
                  ORDER BY bt.end_date ASC
                  LIMIT 10",
                param
            )).ToList();

            var response = new DashboardResponse
            {
                TotalEmployees = totalEmployees,
                TotalSalaryExpense = totalSalary,
                EmployeesOnBusinessTrip = onBusinessTrip,
                ContractsExpiringSoon = contractsExpiring,
                EmployeesWithoutContract = withoutContract,
                UnsignedContracts = unsignedContracts,
                EmployeesAtRisk = atRisk,
                NewEmployees = newEmployees,
                ExpiringContracts = expiringList,
                EmployeesWithoutContractList = noContractList,
                UnsignedContractsList = unsignedList,
                AtRiskEmployeesList = atRiskList,
                BusinessTripsList = tripList,
            };

            return Ok(new { data = response });
        }
    }
}
