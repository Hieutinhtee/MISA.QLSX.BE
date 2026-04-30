using System;
using System.Threading.Tasks;
using MISA.QLSX.Core.Entities;
using MISA.QLSX.Core.Exceptions;
using MISA.QLSX.Core.Interfaces.Repository;
using MISA.QLSX.Core.Interfaces.Service;

namespace MISA.QLSX.Core.Services
{
    /// <summary>
    /// Dịch vụ nghiệp vụ cho danh mục bằng cấp.
    /// </summary>
    public class DegreeService : BaseServices<Degree>, IDegreeService
    {
        private readonly IDegreeRepository _degreeRepository;

        /// <summary>
        /// Khởi tạo dịch vụ bằng cấp.
        /// </summary>
        /// <param name="degreeRepository">Repository bằng cấp được tiêm từ DI.</param>
        public DegreeService(IDegreeRepository degreeRepository)
            : base(degreeRepository)
        {
            _degreeRepository = degreeRepository;
        }

        /// <summary>
        /// Chuẩn hóa dữ liệu trước khi lưu bằng cấp.
        /// </summary>
        /// <param name="degree">Đối tượng bằng cấp cần chuẩn hóa.</param>
        /// <param name="isUpdate">Xác định luồng cập nhật hay thêm mới.</param>
        /// <returns>Task hoàn thành khi xử lý xong.</returns>
        protected override Task BeforeSaveAsync(Degree degree, bool isUpdate = false)
        {
            if (!isUpdate)
            {
                degree.CreatedAt = DateTime.Now;
            }

            degree.UpdatedAt = DateTime.Now;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Kiểm tra tính hợp lệ của dữ liệu bằng cấp.
        /// </summary>
        /// <param name="degree">Đối tượng bằng cấp cần kiểm tra.</param>
        /// <param name="ignoreId">ID bỏ qua khi kiểm tra trùng lặp ở luồng cập nhật.</param>
        /// <returns>Task hoàn thành khi validate xong.</returns>
        protected override async Task ValidateAsync(Degree degree, Guid? ignoreId = null)
        {
            if (degree == null)
                throw new ValidateException("Degree object is null", "Dữ liệu bằng cấp không được để trống");

            if (string.IsNullOrWhiteSpace(degree.DegreeCode))
                throw new ValidateException("DegreeCode required", "Mã bằng cấp không được để trống");

            if (degree.DegreeCode.Length > 20)
                throw new ValidateException("DegreeCode max length", "Mã bằng cấp tối đa 20 ký tự");

            if (string.IsNullOrWhiteSpace(degree.DegreeName))
                throw new ValidateException("DegreeName required", "Tên bằng cấp không được để trống");

            if (degree.DegreeName.Length > 100)
                throw new ValidateException("DegreeName max length", "Tên bằng cấp tối đa 100 ký tự");

            if (await _degreeRepository.IsValueExistAsync(nameof(Degree.DegreeCode), degree.DegreeCode, ignoreId))
                throw new ValidateException("DegreeCode duplicate", "Mã bằng cấp đã tồn tại");

            if (await _degreeRepository.IsValueExistAsync(nameof(Degree.DegreeName), degree.DegreeName, ignoreId))
                throw new ValidateException("DegreeName duplicate", "Tên bằng cấp đã tồn tại");
        }
    }
}
