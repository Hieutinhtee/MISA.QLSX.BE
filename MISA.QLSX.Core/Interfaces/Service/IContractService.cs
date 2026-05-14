using MISA.QLSX.Core.Entities;

namespace MISA.QLSX.Core.Interfaces.Service
{
    public interface IContractService : IBaseService<Contract> 
    {
        Task<List<Allowance>> GetAllowancesByContractIdAsync(Guid contractId);
    }
}
