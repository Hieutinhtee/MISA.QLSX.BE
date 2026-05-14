using MISA.QLSX.Core.Entities;
using MISA.QLSX.Core.Interfaces.Service;

using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace MISA.QLSX.Api.Controllers
{
    public class ContractsController : BaseController<Contract>
    {
        private readonly IContractService _contractService;

        public ContractsController(IContractService contractService)
            : base(contractService) 
        {
            _contractService = contractService;
        }

        [HttpGet("{id}/allowances")]
        public async Task<IActionResult> GetAllowances(Guid id)
        {
            var allowances = await _contractService.GetAllowancesByContractIdAsync(id);
            return Ok(new { data = allowances });
        }
    }
}
