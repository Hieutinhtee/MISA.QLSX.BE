using MISA.QLSX.Core.DTOs.Requests;
using MISA.QLSX.Core.Entities;
using MISA.QLSX.Core.Interfaces.Repository;
using MISA.QLSX.Infrastructure.Connection;

namespace MISA.QLSX.Infrastructure.Repositories
{
    public class AttendanceRepository : BaseRepository<Attendance>, IAttendanceRepository
    {
        public AttendanceRepository(MySqlConnectionFactory factory)
            : base(factory) { }

        protected override HashSet<string> GetSearchFields()
        {
            return new HashSet<string> { "attendance_code" };
        }

        protected override Dictionary<string, FieldMapItem> FieldMap =>
            new()
            {
                ["attendanceCode"] = new() { Column = "attendance_code", DataType = typeof(string), Operators = new() { "eq", "neq", "contains", "notcontains", "starts", "ends" } },
                ["employeeId"] = new() { Column = "employee_id", DataType = typeof(Guid), Operators = new() { "eq", "neq" } },
                ["shiftId"] = new() { Column = "shift_id", DataType = typeof(Guid), Operators = new() { "eq", "neq", "isnull", "notnull" } },
                ["attendanceDate"] = new() { Column = "attendance_date", DataType = typeof(DateTime), Operators = new() { "eq", "lt", "lte", "gt", "gte" } },
                ["workingHours"] = new() { Column = "working_hours", DataType = typeof(decimal), Operators = new() { "eq", "lt", "lte", "gt", "gte", "neq" } },
                ["overtimeHours"] = new() { Column = "overtime_hours", DataType = typeof(decimal), Operators = new() { "eq", "lt", "lte", "gt", "gte", "neq" } },
                ["penaltyAmount"] = new() { Column = "penalty_amount", DataType = typeof(decimal), Operators = new() { "eq", "lt", "lte", "gt", "gte", "neq" } },
                ["bonusAmount"] = new() { Column = "bonus_amount", DataType = typeof(decimal), Operators = new() { "eq", "lt", "lte", "gt", "gte", "neq" } },
                ["netIncome"] = new() { Column = "net_income", DataType = typeof(decimal), Operators = new() { "eq", "lt", "lte", "gt", "gte", "neq" } },
            };
    }
}