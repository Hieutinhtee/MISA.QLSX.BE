using MISA.QLSX.Core.Entities;
using MISA.QLSX.Core.Interfaces.Repository;
using MISA.QLSX.Core.DTOs.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using MISA.QLSX.Infrastructure.Connection;

namespace MISA.QLSX.Infrastructure.Repositories
{
    /// <summary>
    /// Repository để quản lý dữ liệu liên kết giữa hợp đồng và phụ cấp.
    /// </summary>
    public class ContractAllowanceRepository : BaseRepository<ContractAllowance>, IContractAllowanceRepository
    {
        public ContractAllowanceRepository(MySqlConnectionFactory factory)
            : base(factory)
        {
        }

        /// <summary>
        /// Lấy danh sách phụ cấp theo hợp đồng ID.
        /// </summary>
        public async Task<List<ContractAllowance>> GetByContractIdAsync(Guid contractId)
        {
            var sql = @"
                SELECT ca.* 
                FROM contract_allowance ca
                WHERE ca.contract_id = @contractId
            ";

            using (var connection = _factory.CreateConnection())
            {
                var result = await connection.QueryAsync<ContractAllowance>(sql, new { contractId });
                return result.ToList();
            }
        }

        /// <summary>
        /// Lấy danh sách phụ cấp theo danh sách hợp đồng ID.
        /// </summary>
        public async Task<List<ContractAllowance>> GetByContractIdsAsync(List<Guid> contractIds)
        {
            if (contractIds == null || contractIds.Count == 0)
                return new List<ContractAllowance>();

            var sql = @"
                SELECT ca.* 
                FROM contract_allowance ca
                WHERE ca.contract_id IN ({0})
            ";

            var paramNames = string.Join(",", contractIds.Select((_, i) => $"@p{i}"));
            sql = string.Format(sql, paramNames);

            var parameters = new Dictionary<string, object>();
            for (int i = 0; i < contractIds.Count; i++)
            {
                parameters[$"p{i}"] = contractIds[i];
            }

            using (var connection = _factory.CreateConnection())
            {
                var dynamicParams = new DynamicParameters();
                foreach (var param in parameters)
                {
                    dynamicParams.Add(param.Key, param.Value);
                }

                var result = await connection.QueryAsync<ContractAllowance>(sql, dynamicParams);
                return result.ToList();
            }
        }

        protected override HashSet<string> GetSearchFields()
        {
            return new HashSet<string>
            {
                "contract_id",
                "allowance_id"
            };
        }

        protected override Dictionary<string, FieldMapItem> FieldMap =>
            new Dictionary<string, FieldMapItem>
            {
                ["contractId"] = new()
                {
                    Column = "contract_id",
                    DataType = typeof(Guid),
                    Operators = new() { "eq", "neq" }
                },
                ["allowanceId"] = new()
                {
                    Column = "allowance_id",
                    DataType = typeof(Guid),
                    Operators = new() { "eq", "neq" }
                }
            };
    }
}
