using MISA.QLSX.Core.Entities;
using MISA.QLSX.Core.Interfaces.Repository;
using MISA.QLSX.Core.Interfaces.Service;

namespace MISA.QLSX.Core.Services
{
    public class DependentService : BaseServices<Dependent>, IDependentService
    {
        private readonly IDependentRepository _dependentRepository;

        public DependentService(IDependentRepository repository) : base(repository)
        {
            _dependentRepository = repository;
        }

        public async Task<List<Dependent>> GetByEmployeeIdAsync(Guid employeeId)
        {
            return await _dependentRepository.GetByEmployeeIdAsync(employeeId);
        }
    }
}
