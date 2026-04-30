namespace MISA.QLSX.Core.DTOs.Responses
{
    /// <summary>
    /// DTO phản hồi cho trang tổng quan (Dashboard)
    /// </summary>
    public class DashboardResponse
    {
        /// <summary>
        /// Tổng số nhân viên
        /// </summary>
        public int TotalEmployees { get; set; }

        /// <summary>
        /// Tổng chi lương (tổng net_salary của kỳ lương gần nhất)
        /// </summary>
        public decimal TotalSalaryExpense { get; set; }

        /// <summary>
        /// Số người đang đi công tác
        /// </summary>
        public int EmployeesOnBusinessTrip { get; set; }

        /// <summary>
        /// Số hợp đồng sắp hết hạn (trong 30 ngày tới)
        /// </summary>
        public int ContractsExpiringSoon { get; set; }

        /// <summary>
        /// Số nhân viên chưa có hợp đồng
        /// </summary>
        public int EmployeesWithoutContract { get; set; }

        /// <summary>
        /// Số hợp đồng chưa ký
        /// </summary>
        public int UnsignedContracts { get; set; }

        /// <summary>
        /// Số nhân viên có nguy cơ nghỉ việc (đánh giá kỷ luật gần đây)
        /// </summary>
        public int EmployeesAtRisk { get; set; }

        /// <summary>
        /// Nhân viên mới trong kỳ
        /// </summary>
        public int NewEmployees { get; set; }

        /// <summary>
        /// Danh sách hợp đồng sắp hết hạn
        /// </summary>
        public List<ExpiringContractItem>? ExpiringContracts { get; set; }

        /// <summary>
        /// Danh sách nhân viên chưa có hợp đồng
        /// </summary>
        public List<EmployeeSimpleItem>? EmployeesWithoutContractList { get; set; }

        /// <summary>
        /// Danh sách hợp đồng chưa ký
        /// </summary>
        public List<UnsignedContractItem>? UnsignedContractsList { get; set; }

        /// <summary>
        /// Danh sách nhân viên có nguy cơ nghỉ việc
        /// </summary>
        public List<AtRiskEmployeeItem>? AtRiskEmployeesList { get; set; }

        /// <summary>
        /// Danh sách nhân viên đang đi công tác
        /// </summary>
        public List<BusinessTripItem>? BusinessTripsList { get; set; }
    }

    public class ExpiringContractItem
    {
        public Guid? ContractId { get; set; }
        public string? ContractCode { get; set; }
        public string? EmployeeName { get; set; }
        public string? EmployeeCode { get; set; }
        public DateTime? EndDate { get; set; }
        public int DaysRemaining { get; set; }
    }

    public class EmployeeSimpleItem
    {
        public Guid? EmployeeId { get; set; }
        public string? EmployeeCode { get; set; }
        public string? FullName { get; set; }
        public DateTime? JoinDate { get; set; }
    }

    public class UnsignedContractItem
    {
        public Guid? ContractId { get; set; }
        public string? ContractCode { get; set; }
        public string? EmployeeName { get; set; }
        public string? EmployeeCode { get; set; }
        public DateTime? EffectiveDate { get; set; }
    }

    public class AtRiskEmployeeItem
    {
        public Guid? EmployeeId { get; set; }
        public string? EmployeeCode { get; set; }
        public string? FullName { get; set; }
        public string? Reason { get; set; }
        public decimal? Amount { get; set; }
        public DateTime? EvaluationDate { get; set; }
    }

    public class BusinessTripItem
    {
        public Guid? BusinessTripId { get; set; }
        public string? EmployeeName { get; set; }
        public string? EmployeeCode { get; set; }
        public string? Location { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
