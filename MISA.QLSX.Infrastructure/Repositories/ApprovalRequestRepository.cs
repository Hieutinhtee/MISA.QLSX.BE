using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using MISA.QLSX.Core.DTOs.Requests;
using MISA.QLSX.Core.DTOs.Responses;
using MISA.QLSX.Core.Entities;
using MISA.QLSX.Core.Interfaces.Repository;
using MISA.QLSX.Infrastructure.Connection;

namespace MISA.QLSX.Infrastructure.Repositories
{
    /// <summary>
    /// Repository yêu cầu phê duyệt.
    /// </summary>
    public class ApprovalRequestRepository
        : BaseRepository<ApprovalRequest>,
            IApprovalRequestRepository
    {
        public ApprovalRequestRepository(MySqlConnectionFactory factory)
            : base(factory) { }

        protected override HashSet<string> GetSearchFields()
        {
            return new HashSet<string> { "request_code", "title", "request_type", "status" };
        }

        protected override Dictionary<string, FieldMapItem> FieldMap =>
            new()
            {
                ["requestCode"] = new()
                {
                    Column = "request_code",
                    DataType = typeof(string),
                    Operators = new() { "eq", "contains", "starts", "ends", "neq", "notcontains" },
                },
                ["title"] = new()
                {
                    Column = "title",
                    DataType = typeof(string),
                    Operators = new() { "eq", "contains", "starts", "ends" },
                },
                ["requestType"] = new()
                {
                    Column = "request_type",
                    DataType = typeof(string),
                    Operators = new() { "eq", "neq" },
                },
                ["status"] = new()
                {
                    Column = "status",
                    DataType = typeof(string),
                    Operators = new() { "eq", "neq" },
                },
                ["effectiveDate"] = new()
                {
                    Column = "effective_date",
                    DataType = typeof(DateTime),
                    Operators = new() { "eq", "lt", "lte", "gt", "gte" },
                },
                ["createdAt"] = new()
                {
                    Column = "created_at",
                    DataType = typeof(DateTime),
                    Operators = new() { "eq", "lt", "lte", "gt", "gte" },
                },
            };

        public override async Task<ApprovalRequest?> GetById(Guid id)
        {
            using var conn = Connection;
            var sql =
                $@"SELECT ar.*, e.full_name AS CreatedByName 
                        FROM approval_request ar
                        LEFT JOIN employee e ON ar.created_by = e.employee_id
                        WHERE ar.approval_request_id = @Id {SoftDeleteFilter("ar.")}";
            return await conn.QueryFirstOrDefaultAsync<ApprovalRequest>(sql, new { Id = id });
        }

        public override async Task<PagingResponse<ApprovalRequest>> QueryPagingAsync(
            QueryRequest request
        )
        {
            using var conn = Connection;

            // Build WHERE clause từ filters của BaseRepository
            var (where, parameters) = BuildWhereClause(request.Filters, request.Search, "ar.");
            var whereClause = string.IsNullOrEmpty(where) ? "" : "AND " + where;

            var sqlData =
                $@"SELECT ar.*, e.full_name AS CreatedByName 
                             FROM approval_request ar
                             LEFT JOIN employee e ON ar.created_by = e.employee_id
                             WHERE 1=1 {whereClause}
                             ORDER BY ar.created_at DESC
                             LIMIT @Offset, @PageSize";

            var sqlTotal =
                $@"SELECT COUNT(*) FROM approval_request ar
                              WHERE 1=1 {whereClause}";

            parameters.Add("Offset", ((request.Page ?? 1) - 1) * (request.PageSize ?? 20));
            parameters.Add("PageSize", request.PageSize ?? 20);

            var data = await conn.QueryAsync<ApprovalRequest>(sqlData, parameters);
            var total = await conn.ExecuteScalarAsync<int>(sqlTotal, parameters);

            return new PagingResponse<ApprovalRequest>
            {
                Data = data.AsList(),
                Meta = new Meta
                {
                    Total = total,
                    Page = request.Page ?? 1,
                    PageSize = request.PageSize ?? 20,
                },
            };
        }

        /// <inheritdoc />
        public async Task<List<ApprovalStep>> GetStepsByRequestIdAsync(Guid requestId)
        {
            using var conn = _factory.CreateConnection();
            var sql =
                @"SELECT s.*, e.full_name AS ActedByName 
                        FROM approval_step s
                        LEFT JOIN employee e ON s.acted_by = e.employee_id
                        WHERE s.approval_request_id = @RequestId 
                        ORDER BY s.step_order ASC";
            var result = await conn.QueryAsync<ApprovalStep>(sql, new { RequestId = requestId });
            return result.AsList();
        }

        /// <inheritdoc />
        public async Task<Guid> InsertStepAsync(ApprovalStep step)
        {
            using var conn = _factory.CreateConnection();
            var id = Guid.NewGuid();
            var sql =
                @"INSERT INTO approval_step 
                        (approval_step_id, approval_request_id, step_order, approver_role, approver_id, status)
                        VALUES (@Id, @ApprovalRequestId, @StepOrder, @ApproverRole, @ApproverId, @Status)";
            await conn.ExecuteAsync(
                sql,
                new
                {
                    Id = id,
                    step.ApprovalRequestId,
                    step.StepOrder,
                    step.ApproverRole,
                    step.ApproverId,
                    Status = step.Status ?? "pending",
                }
            );
            return id;
        }

        /// <inheritdoc />
        public async Task<int> UpdateStepAsync(ApprovalStep step)
        {
            using var conn = _factory.CreateConnection();
            var sql =
                @"UPDATE approval_step 
                        SET status = @Status, comment = @Comment, acted_at = @ActedAt, acted_by = @ActedBy
                        WHERE approval_step_id = @ApprovalStepId";
            return await conn.ExecuteAsync(
                sql,
                new
                {
                    step.Status,
                    step.Comment,
                    step.ActedAt,
                    step.ActedBy,
                    step.ApprovalStepId,
                }
            );
        }

        /// <inheritdoc />
        public async Task<ApprovalStep?> GetStepByIdAsync(Guid stepId)
        {
            using var conn = _factory.CreateConnection();
            var sql = "SELECT * FROM approval_step WHERE approval_step_id = @StepId";
            return await conn.QueryFirstOrDefaultAsync<ApprovalStep>(sql, new { StepId = stepId });
        }

        /// <inheritdoc />
        public async Task<int> UpdateEmployeeDepartmentAsync(Guid employeeId, Guid departmentId)
        {
            using var conn = _factory.CreateConnection();
            var sql =
                "UPDATE employee SET department_id = @DepartmentId WHERE employee_id = @EmployeeId AND (is_deleted = '00000000-0000-0000-0000-000000000000')";
            return await conn.ExecuteAsync(
                sql,
                new { DepartmentId = departmentId, EmployeeId = employeeId }
            );
        }

        /// <inheritdoc />
        public async Task<int> UpdateDepartmentManagerAsync(
            Guid departmentId,
            Guid managerEmployeeId
        )
        {
            using var conn = _factory.CreateConnection();
            var sql =
                "UPDATE department SET manager_employee_id = @ManagerEmployeeId WHERE department_id = @DepartmentId AND (is_deleted = '00000000-0000-0000-0000-000000000000')";
            return await conn.ExecuteAsync(
                sql,
                new { ManagerEmployeeId = managerEmployeeId, DepartmentId = departmentId }
            );
        }

        /// <inheritdoc />
        public async Task<Guid> InsertMemberHistoryAsync(
            Guid employeeId,
            Guid departmentId,
            string action,
            DateTime effectiveDate,
            Guid? approvalRequestId,
            Guid? createdBy
        )
        {
            using var conn = _factory.CreateConnection();
            var id = Guid.NewGuid();
            var sql =
                @"INSERT INTO department_member_history 
                        (history_id, employee_id, department_id, action, effective_date, approval_request_id, created_by)
                        VALUES (@Id, @EmployeeId, @DepartmentId, @Action, @EffectiveDate, @ApprovalRequestId, @CreatedBy)";
            await conn.ExecuteAsync(
                sql,
                new
                {
                    Id = id,
                    EmployeeId = employeeId,
                    DepartmentId = departmentId,
                    Action = action,
                    EffectiveDate = effectiveDate,
                    ApprovalRequestId = approvalRequestId,
                    CreatedBy = createdBy,
                }
            );
            return id;
        }

        /// <inheritdoc />
        public async Task<Guid> InsertManagerHistoryAsync(
            Guid departmentId,
            Guid managerEmployeeId,
            DateTime effectiveDate,
            Guid? approvalRequestId,
            Guid? createdBy
        )
        {
            using var conn = _factory.CreateConnection();
            var id = Guid.NewGuid();
            var sql =
                @"INSERT INTO department_manager_history 
                        (history_id, department_id, manager_employee_id, effective_date, approval_request_id, created_by)
                        VALUES (@Id, @DepartmentId, @ManagerEmployeeId, @EffectiveDate, @ApprovalRequestId, @CreatedBy)";
            await conn.ExecuteAsync(
                sql,
                new
                {
                    Id = id,
                    DepartmentId = departmentId,
                    ManagerEmployeeId = managerEmployeeId,
                    EffectiveDate = effectiveDate,
                    ApprovalRequestId = approvalRequestId,
                    CreatedBy = createdBy,
                }
            );
            return id;
        }

        /// <inheritdoc />
        public async Task<string> GenerateRequestCodeAsync()
        {
            using var conn = _factory.CreateConnection();
            var sql = "SELECT COUNT(*) FROM approval_request";
            var count = await conn.ExecuteScalarAsync<int>(sql);
            return $"REQ-{(count + 1):D5}";
        }
    }
}
