using MISA.QLSX.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MISA.QLSX.Core.Interfaces.Repository
{
    /// <summary>
    /// Interface để truy vấn dữ liệu liên kết giữa hợp đồng và phụ cấp.
    /// </summary>
    public interface IContractAllowanceRepository : IBaseRepository<ContractAllowance>
    {
        /// <summary>
        /// Lấy danh sách phụ cấp theo hợp đồng ID.
        /// </summary>
        /// <param name="contractId">ID của hợp đồng.</param>
        /// <returns>Danh sách ContractAllowance của hợp đồng.</returns>
        Task<List<ContractAllowance>> GetByContractIdAsync(Guid contractId);

        /// <summary>
        /// Lấy danh sách phụ cấp theo danh sách hợp đồng ID.
        /// </summary>
        /// <param name="contractIds">Danh sách ID hợp đồng.</param>
        /// <returns>Danh sách ContractAllowance của các hợp đồng.</returns>
        Task<List<ContractAllowance>> GetByContractIdsAsync(List<Guid> contractIds);
    }
}
