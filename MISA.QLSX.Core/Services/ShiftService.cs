using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MISA.QLSX.Core.Entities;
using MISA.QLSX.Core.Interfaces.Repository;
using MISA.QLSX.Core.Interfaces.Service;

namespace MISA.QLSX.Core.Services
{
    public class ShiftService : BaseServices<Shift>, IShiftService
    {
        #region Declaration

        private readonly IShiftRepository _shiftRepo;

        #endregion Declaration

        #region Constructor

        /// <summary>
        /// Hàm khởi tạo Service Khách hàng
        /// </summary>
        /// <param name="repo">Repository xử lý Khách hàng được tiêm vào (Dependency Injection)</param>
        /// Created by TMHieu - 7/12/2025
        public ShiftService(IShiftRepository repo)
            : base(repo)
        {
            _shiftRepo = repo;
        }

        #endregion Constructor
    }
}
