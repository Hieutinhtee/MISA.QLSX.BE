using System.ComponentModel.DataAnnotations.Schema;
using MISA.QLSX.Core.DTOs.Requests;
using MISA.QLSX.Core.Entities;
using MISA.QLSX.Core.Interfaces.Repository;
using MISA.QLSX.Infrastructure.Connection;

namespace MISA.QLSX.Infrastructure.Repositories
{
    public class EmployeeRepository : BaseRepository<Employee>, IEmployeeRepository
    {
        private const string ReadViewName = "vw_employee_detail";

        public EmployeeRepository(MySqlConnectionFactory factory)
            : base(factory) { }

        protected override string GetReadTableName() => ReadViewName;

        protected override HashSet<string> GetSearchFields()
        {
            return new HashSet<string>
            {
                "employee_code",
                "full_name",
                "phone_number",
                "email",
                "national_id",
                "department_name",
                "shift_name",
                "degree_name",
                "position_name",
                "address",
                "account_name",
            };
        }

        protected override Dictionary<string, FieldMapItem> FieldMap =>
            new()
            {
                ["employeeCode"] = new()
                {
                    Column = "employee_code",
                    DataType = typeof(string),
                    Operators = new() { "eq", "notcontains", "contains", "starts", "ends", "neq" },
                },
                ["employeeId"] = new()
                {
                    Column = "employee_id",
                    DataType = typeof(Guid),
                    Operators = new() { "eq", "neq", "isnull", "notnull" },
                },
                ["fullName"] = new()
                {
                    Column = "full_name",
                    DataType = typeof(string),
                    Operators = new() { "eq", "notcontains", "contains", "starts", "ends", "neq" },
                },
                ["gender"] = new()
                {
                    Column = "gender",
                    DataType = typeof(string),
                    Operators = new() { "eq", "notcontains", "contains", "starts", "ends", "neq" },
                },
                ["dateOfBirth"] = new()
                {
                    Column = "date_of_birth",
                    DataType = typeof(DateTime),
                    Operators = new() { "eq", "lt", "lte", "gt", "gte" },
                },
                ["address"] = new()
                {
                    Column = "address",
                    DataType = typeof(string),
                    Operators = new() { "eq", "notcontains", "contains", "starts", "ends", "neq" },
                },
                ["phoneNumber"] = new()
                {
                    Column = "phone_number",
                    DataType = typeof(string),
                    Operators = new() { "eq", "notcontains", "contains", "starts", "ends", "neq" },
                },
                ["email"] = new()
                {
                    Column = "email",
                    DataType = typeof(string),
                    Operators = new() { "eq", "notcontains", "contains", "starts", "ends", "neq" },
                },
                ["joinDate"] = new()
                {
                    Column = "join_date",
                    DataType = typeof(DateTime),
                    Operators = new() { "eq", "lt", "lte", "gt", "gte" },
                },
                ["departmentName"] = new()
                {
                    Column = "department_name",
                    DataType = typeof(string),
                    Operators = new() { "eq", "notcontains", "contains", "starts", "ends", "neq" },
                },
                ["departmentId"] = new()
                {
                    Column = "department_id",
                    DataType = typeof(Guid),
                    Operators = new() { "eq", "neq", "isnull", "notnull" },
                },
                ["shiftName"] = new()
                {
                    Column = "shift_name",
                    DataType = typeof(string),
                    Operators = new() { "eq", "notcontains", "contains", "starts", "ends", "neq" },
                },
                ["nationalId"] = new()
                {
                    Column = "national_id",
                    DataType = typeof(string),
                    Operators = new() { "eq", "notcontains", "contains", "starts", "ends", "neq" },
                },
                ["degreeName"] = new()
                {
                    Column = "degree_name",
                    DataType = typeof(string),
                    Operators = new() { "eq", "notcontains", "contains", "starts", "ends", "neq" },
                },
                ["positionName"] = new()
                {
                    Column = "position_name",
                    DataType = typeof(string),
                    Operators = new() { "eq", "notcontains", "contains", "starts", "ends", "neq" },
                },
                ["accountName"] = new()
                {
                    Column = "account_name",
                    DataType = typeof(string),
                    Operators = new() { "eq", "notcontains", "contains", "starts", "ends", "neq" },
                },

                ["createdAt"] = new()
                {
                    Column = "created_at",
                    DataType = typeof(DateTime),
                    Operators = new() { "eq", "lt", "lte", "gt", "gte" },
                },
                ["updatedAt"] = new()
                {
                    Column = "updated_at",
                    DataType = typeof(DateTime),
                    Operators = new() { "eq", "lt", "lte", "gt", "gte" },
                },
            };
    }
}
