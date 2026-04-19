using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MISA.QLSX.Core.Entities;
using MISA.QLSX.Core.Exceptions;
using MISA.QLSX.Core.Interfaces.Repository;
using MISA.QLSX.Core.Interfaces.Service;
using OfficeOpenXml;

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

        public async Task<byte[]> ExportShiftExcelAsync()
        {
            var shifts = await GetAllAsync();

            ExcelPackage.License.SetNonCommercialPersonal("TMHieu");

            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Shifts");

            // Header
            ws.Cells[1, 1].Value = "Mã ca";
            ws.Cells[1, 2].Value = "Tên ca";
            ws.Cells[1, 3].Value = "Giờ vào";
            ws.Cells[1, 4].Value = "Giờ ra";
            ws.Cells[1, 5].Value = "Giờ nghỉ bắt đầu";
            ws.Cells[1, 6].Value = "Giờ nghỉ kết thúc";
            ws.Cells[1, 7].Value = "Thời gian làm việc";
            ws.Cells[1, 8].Value = "Thời gian nghỉ";
            ws.Cells[1, 9].Value = "Trạng thái";

            int row = 2;

            foreach (var s in shifts)
            {
                ws.Cells[row, 1].Value = s.ShiftCode;
                ws.Cells[row, 2].Value = s.ShiftName;
                ws.Cells[row, 3].Value = s.StartTime?.ToString(@"hh\:mm");
                ws.Cells[row, 4].Value = s.EndTime?.ToString(@"hh\:mm");
                ws.Cells[row, 5].Value = s.BreakStartTime?.ToString(@"hh\:mm");
                ws.Cells[row, 6].Value = s.BreakEndTime?.ToString(@"hh\:mm");
                ws.Cells[row, 7].Value = s.WorkingHours;
                ws.Cells[row, 8].Value = s.BreakHours;
                ws.Cells[row, 9].Value =
                    s.IsActive == true ? "Đang sử dụng" : "Ngừng sử dụng";

                row++;
            }

            ws.Cells.AutoFitColumns();

            return package.GetAsByteArray();
        }

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
            var begin = shift.StartTime!.Value;
            var end = shift.EndTime!.Value;

            var shiftDuration = CalculateDuration(begin, end);

            TimeSpan breakDuration = TimeSpan.Zero;

            if (
                shift.BreakStartTime != null
                && shift.BreakEndTime != null
            )
            {
                breakDuration = CalculateDuration(
                    shift.BreakStartTime.Value,
                    shift.BreakEndTime.Value
                );
            }

            shift.BreakHours = (decimal)breakDuration.TotalHours;

            shift.WorkingHours = (decimal)(shiftDuration - breakDuration).TotalHours;
        }

        /// <summary>
        /// Tùy chỉnh cho thực thể (có thể override ở lớp con) trước khi save
        /// tính toán hoặc xử lý thêm gì đó
        /// Created by: TMHieu (07/12/2025)
        /// </summary>
        /// <param name="shift">Thực thể cần xử lí.</param>
        /// <param name="id">ID tùy chọn để xử lí (ví dụ khi update).</param>
        /// <returns>Task hoàn thành sau xử lí.</returns>
        protected override Task BeforeSaveAsync(Shift shift, bool isUpdate = false)
        {
            CalculateWorkingAndBreakTime(shift);
            if (!isUpdate)
                shift.CreatedAt = DateTime.Now;
            shift.UpdatedAt = DateTime.Now;

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

            if (string.IsNullOrWhiteSpace(shift.ShiftCode))
                throw new ValidateException("ShiftCode required", "Mã ca không được để trống");

            if (shift.ShiftCode.Length > 20)
                throw new ValidateException("ShiftCode max length", "Mã ca tối đa 20 ký tự");

            // 2. Tên ca - bắt buộc

            if (string.IsNullOrWhiteSpace(shift.ShiftName))
                throw new ValidateException("ShiftName required", "Tên ca không được để trống");

            if (shift.ShiftName.Length > 100)
                throw new ValidateException("ShiftName max length", "Tên ca tối đa 100 ký tự");

            // 3. Giờ vào ca - bắt buộc

            if (shift.StartTime == null)
                throw new ValidateException("BeginTime required", "Giờ vào ca không được để trống");

            // 4. Giờ hết ca - bắt buộc

            if (shift.EndTime == null)
                throw new ValidateException("EndTime required", "Giờ hết ca không được để trống");

            // 5. Kiểm tra mã ca trùng (trừ chính bản ghi đang sửa)

            if (
                await _shiftRepo.IsValueExistAsync(
                    nameof(Shift.ShiftCode),
                    shift.ShiftCode,
                    ignoreId
                )
            )
            {
                throw new ValidateException("ShiftCode duplicate", "Mã ca đã tồn tại");
            }

            // ===== VALIDATE THỜI GIAN =====

            var begin = shift.StartTime!.Value;
            var end = shift.EndTime!.Value;

            // Không cho 2 mốc ca trùng nhau
            if (begin == end)
            {
                throw new ValidateException(
                    "Shift time invalid",
                    "Giờ vào ca và giờ hết ca không được trùng nhau"
                );
            }

            // Chuẩn hóa ca làm lên trục 48h (tính theo phút)
            int shiftBegin = (int)begin.TotalMinutes;
            int shiftEnd = (int)end.TotalMinutes;

            if (shiftEnd <= shiftBegin)
            {
                shiftEnd += 24 * 60;
            }

            int shiftDuration = shiftEnd - shiftBegin;

            // ===== VALIDATE NGHỈ =====

            var breakBegin = shift.BreakStartTime;
            var breakEnd = shift.BreakEndTime;

            // Nếu có khai báo nghỉ thì phải đủ cặp
            if (breakBegin != null && breakEnd == null)
            {
                throw new ValidateException(
                    "BreakEnd required",
                    "Phải nhập giờ kết thúc nghỉ giữa ca"
                );
            }

            if (breakBegin == null && breakEnd != null)
            {
                throw new ValidateException(
                    "BreakBegin required",
                    "Phải nhập giờ bắt đầu nghỉ giữa ca"
                );
            }

            if (breakBegin != null && breakEnd != null)
            {
                // Không cho 2 mốc nghỉ trùng nhau
                if (breakBegin.Value == breakEnd.Value)
                {
                    throw new ValidateException(
                        "Break time invalid",
                        "Giờ bắt đầu nghỉ và giờ kết thúc nghỉ không được trùng nhau"
                    );
                }

                int bb = (int)breakBegin.Value.TotalMinutes;
                int be = (int)breakEnd.Value.TotalMinutes;

                // Đưa break về cùng trục thời gian với ca
                if (bb < shiftBegin)
                {
                    bb += 24 * 60;
                }

                if (be <= bb)
                {
                    be += 24 * 60;
                }

                int breakDuration = be - bb;

                // Nghỉ phải nằm trong ca
                if (bb < shiftBegin || be > shiftEnd)
                {
                    throw new ValidateException(
                        "Break out of range",
                        "Thời gian nghỉ phải nằm trong khoảng thời gian làm việc"
                    );
                }
            }
        }

        public Task<int> UpdateIsActiveMany(List<Guid> ids, bool isActive)
        {
            var columnName = "is_active";
            return _repo.BulkUpdateSameValueAsync(ids, columnName, isActive);
        }

        #endregion Method
    }
}
