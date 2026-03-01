using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MISA.QLSX.Core.Entities;
using MISA.QLSX.Core.Exceptions;
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
        /// Hàm khởi tạo Service ca làm việc
        /// </summary>
        /// <param name="repo">Repository xử lý ca làm việc được tiêm vào (Dependency Injection)</param>
        /// Created by TMHieu - 7/12/2025
        public ShiftService(IShiftRepository repo)
            : base(repo)
        {
            _shiftRepo = repo;
        }

        #endregion Constructor

        #region Method

        /// <summary>
        /// Tính toán khoảng thời gian giữa 2 mốc thời gian
        /// </summary>
        /// <param name="begin">Mốc bắt đầu</param>
        /// <param name="end">Mốc kết thúc</param>
        /// <returns></returns>
        private TimeSpan CalculateDuration(TimeSpan begin, TimeSpan end)
        {
            if (begin == end)
                return TimeSpan.FromHours(24);

            if (end > begin)
                return end - begin;

            // Qua ngày
            return (TimeSpan.FromHours(24) - begin) + end;
        }

        /// <summary>
        /// Tính toán thời gian làm và thời gian nghỉ
        /// </summary>
        /// <param name="shift">thực thể ca làm việc</param>
        private void CalculateWorkingAndBreakTime(Shift shift)
        {
            var begin = shift.ProductionShiftBeginTime!.Value;
            var end = shift.ProductionShiftEndTime!.Value;

            var shiftDuration = CalculateDuration(begin, end);

            TimeSpan breakDuration = TimeSpan.Zero;

            if (
                shift.ProductionShiftBeginBreakTime != null
                && shift.ProductionShiftEndBreakTime != null
            )
            {
                breakDuration = CalculateDuration(
                    shift.ProductionShiftBeginBreakTime.Value,
                    shift.ProductionShiftEndBreakTime.Value
                );
            }

            shift.ProductionShiftBreakTime = (decimal)breakDuration.TotalHours;

            shift.ProductionShiftWorkingTime = (decimal)(shiftDuration - breakDuration).TotalHours;
        }

        /// <summary>
        /// Tùy chỉnh cho thực thể (có thể override ở lớp con) trước khi save
        /// tính toán hoặc xử lý thêm gì đó
        /// Created by: TMHieu (07/12/2025)
        /// </summary>
        /// <param name="shift">Thực thể cần xử lí.</param>
        /// <param name="id">ID tùy chọn để xử lí (ví dụ khi update).</param>
        /// <returns>Task hoàn thành sau xử lí.</returns>
        protected override Task BeforeSaveAsync(Shift shift)
        {
            CalculateWorkingAndBreakTime(shift);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Thực hiện validate các thông tin bắt buộc và các quy tắc nghiệp vụ của đối tượng ca làm việc
        /// </summary>
        /// <param name="shift">Đối tượng ca làm việc cần validate</param>
        /// <param name="ignoreId">ID của bản ghi (entity) cần bỏ qua khi kiểm tra trùng lặp (dùng trong trường hợp sửa)</param>
        /// <returns>Task</returns>
        /// <exception cref="ValidateException">Ném ra ngoại lệ ValidateException nếu có lỗi</exception>
        /// Created by TMHieu - 7/12/2025
        protected override async Task ValidateAsync(Shift shift, Guid? ignoreId = null)
        {
            if (shift == null)
                throw new ValidateException(
                    "Shift object is null",
                    "Dữ liệu ca làm việc không được để trống"
                );

            // 1. Mã ca - bắt buộc

            if (string.IsNullOrWhiteSpace(shift.ProductionShiftCode))
                throw new ValidateException("ShiftCode required", "Mã ca không được để trống");

            if (shift.ProductionShiftCode.Length > 20)
                throw new ValidateException("ShiftCode max length", "Mã ca tối đa 20 ký tự");

            // 2. Tên ca - bắt buộc

            if (string.IsNullOrWhiteSpace(shift.ProductionShiftName))
                throw new ValidateException("ShiftName required", "Tên ca không được để trống");

            if (shift.ProductionShiftName.Length > 50)
                throw new ValidateException("ShiftName max length", "Tên ca tối đa 50 ký tự");

            // 3. Giờ vào ca - bắt buộc

            if (shift.ProductionShiftBeginTime == null)
                throw new ValidateException("BeginTime required", "Giờ vào ca không được để trống");

            // 4. Giờ hết ca - bắt buộc

            if (shift.ProductionShiftEndTime == null)
                throw new ValidateException("EndTime required", "Giờ hết ca không được để trống");

            // 5. Kiểm tra mã ca trùng (trừ chính bản ghi đang sửa)

            if (
                await _shiftRepo.IsValueExistAsync(
                    nameof(Shift.ProductionShiftCode),
                    shift.ProductionShiftCode,
                    ignoreId
                )
            )
            {
                throw new ValidateException("ShiftCode duplicate", "Mã ca đã tồn tại");
            }

            //Validate nếu có nhập thời gian nghỉ
            var begin = shift.ProductionShiftBeginTime!.Value;
            var end = shift.ProductionShiftEndTime!.Value;

            var breakBegin = shift.ProductionShiftBeginBreakTime;
            var breakEnd = shift.ProductionShiftEndBreakTime;

            // Nếu có khai báo nghỉ thì phải đủ cặp
            if (breakBegin != null && breakEnd == null)
                throw new ValidateException(
                    "BreakEnd required",
                    "Phải nhập giờ kết thúc nghỉ giữa ca"
                );

            if (breakBegin == null && breakEnd != null)
                throw new ValidateException(
                    "BreakBegin required",
                    "Phải nhập giờ bắt đầu nghỉ giữa ca"
                );

            // Nếu có nghỉ
            if (breakBegin != null && breakEnd != null)
            {
                var bb = breakBegin.Value;
                var be = breakEnd.Value;

                // BreakEnd phải lớn hơn BreakBegin
                if (be < bb)
                    throw new ValidateException(
                        "Break time invalid",
                        "Giờ kết thúc nghỉ phải lớn hơn giờ bắt đầu nghỉ"
                    );

                // Hai mốc thời gian Không được bằng nhau
                if (be == bb)
                    throw new ValidateException(
                        "Break time equal",
                        "Thời gian bắt đầu và kết thúc nghỉ không được trùng nhau"
                    );

                // Break phải nằm trong ca làm
                if (bb < begin || be > end)
                    throw new ValidateException(
                        "Break out of range",
                        "Thời gian nghỉ phải nằm trong khoảng thời gian làm việc"
                    );
            }
        }
        #endregion Method
    }
}
