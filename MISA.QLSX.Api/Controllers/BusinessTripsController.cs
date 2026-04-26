using Microsoft.AspNetCore.Mvc;
using MISA.QLSX.Core.Entities;
using MISA.QLSX.Core.Interfaces.Service;

namespace MISA.QLSX.Api.Controllers
{
    public class BusinessTripsController : BaseController<BusinessTrip>
    {
        private readonly IBusinessTripService _businessTripService;

        public BusinessTripsController(IBusinessTripService businessTripService)
            : base(businessTripService)
        {
            _businessTripService = businessTripService;
        }
    }
}
