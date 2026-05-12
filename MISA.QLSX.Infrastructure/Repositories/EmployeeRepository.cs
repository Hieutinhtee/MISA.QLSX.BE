using System.ComponentModel.DataAnnotations.Schema;
using MISA.QLSX.Core.DTOs.Requests;
using MISA.QLSX.Core.Entities;
using MISA.QLSX.Core.Interfaces.Repository;
using MISA.QLSX.Infrastructure.Connection;
using Dapper;

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
                ["placeOfBirth"] = new()
                {
                    Column = "place_of_birth",
                    DataType = typeof(string),
                    Operators = new() { "eq", "contains" },
                },
                ["hometown"] = new()
                {
                    Column = "hometown",
                    DataType = typeof(string),
                    Operators = new() { "eq", "contains" },
                },
                ["ethnic"] = new()
                {
                    Column = "ethnic",
                    DataType = typeof(string),
                    Operators = new() { "eq" },
                },
                ["religion"] = new()
                {
                    Column = "religion",
                    DataType = typeof(string),
                    Operators = new() { "eq" },
                },
                ["nationality"] = new()
                {
                    Column = "nationality",
                    DataType = typeof(string),
                    Operators = new() { "eq" },
                },
                ["maritalStatus"] = new()
                {
                    Column = "marital_status",
                    DataType = typeof(string),
                    Operators = new() { "eq" },
                },
                ["personalEmail"] = new()
                {
                    Column = "personal_email",
                    DataType = typeof(string),
                    Operators = new() { "eq", "contains" },
                },
                ["socialInsuranceNumber"] = new()
                {
                    Column = "social_insurance_number",
                    DataType = typeof(string),
                    Operators = new() { "eq", "contains" },
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

        /// <summary>
        /// Lấy danh sách nhân viên theo mã vai trò.
        /// </summary>
        /// <param name="roleCode">Mã vai trò.</param>
        /// <returns>Danh sách nhân viên.</returns>
        public async Task<List<Employee>> GetEmployeesByRoleAsync(string roleCode)
        {
            using var conn = Connection;
            var sql = $@"SELECT e.*, d.department_name AS DepartmentName, p.position_name AS PositionName
                         FROM employee e
                         INNER JOIN account a ON e.account_id = a.account_id
                         INNER JOIN role r ON a.role_id = r.role_id
                         LEFT JOIN department d ON e.department_id = d.department_id
                         LEFT JOIN position p ON e.position_id = p.position_id
                         WHERE r.role_code = @RoleCode AND e.is_deleted = '00000000-0000-0000-0000-000000000000'";
            var result = await conn.QueryAsync<Employee>(sql, new { RoleCode = roleCode });
            return result.ToList();
        }

        /// <summary>
        /// Lấy ID của trưởng phòng thuộc phòng ban của nhân viên.
        /// </summary>
        /// <param name="employeeId">ID nhân viên.</param>
        /// <returns>ID của trưởng phòng, hoặc null nếu không có.</returns>
        public async Task<Guid?> GetDepartmentManagerIdAsync(Guid employeeId)
        {
            using var conn = Connection;
            var sql = @"SELECT d.manager_employee_id
                        FROM employee e
                        INNER JOIN department d ON e.department_id = d.department_id
                        WHERE e.employee_id = @EmployeeId
                        AND e.is_deleted = '00000000-0000-0000-0000-000000000000'
                        AND d.is_deleted = '00000000-0000-0000-0000-000000000000'";
            return await conn.QueryFirstOrDefaultAsync<Guid?>(sql, new { EmployeeId = employeeId });
        }
    }
}
