# Mô tả Use Case — Dự án MISA.QLSX (Quản lý Sản xuất / Nhân sự)

> Tài liệu này liệt kê và tóm tắt toàn bộ các use case được thi công trong dự án, bao gồm các chức năng quản lý nhân sự, hợp đồng, chấm công, tính lương và phê duyệt.

---

## Danh mục Use Case

| STT | Tên Use Case | Module |
|-----|-------------|--------|
| 1 | Xác thực người dùng (Đăng nhập / Đăng xuất) | Auth |
| 2 | Quản lý nhân viên | Nhân sự |
| 3 | Quản lý ca làm việc | Nhân sự |
| 4 | Quản lý phòng ban | Danh mục |
| 5 | Quản lý chức vụ | Danh mục |
| 6 | Quản lý bằng cấp | Danh mục |
| 7 | Quản lý phụ cấp | Danh mục |
| 8 | Quản lý mẫu hợp đồng | Hợp đồng |
| 9 | Quản lý hợp đồng | Hợp đồng |
| 10 | Quản lý chấm công | Chấm công |
| 11 | Quản lý công tác | Chấm công |
| 12 | Quản lý đánh giá nhân viên | Nhân sự |
| 13 | Quản lý hồ sơ thuế nhân viên | Tiền lương |
| 14 | Quản lý khoản mục lương | Tiền lương |
| 15 | Quản lý yêu cầu phê duyệt | Phê duyệt |
| 16 | Xem tổng quan hệ thống (Dashboard) | Báo cáo |

---

## UC-01: Xác thực người dùng

### 1. Tên Use Case
Xác thực người dùng (Đăng nhập / Đăng xuất)

### 2. Mô tả vắn tắt
Use case này cho phép người dùng đăng nhập vào hệ thống bằng tên đăng nhập và mật khẩu, nhận token xác thực để truy cập các chức năng được phân quyền, và đăng xuất khỏi hệ thống.

### 3. Luồng các sự kiện

#### 3.1. Luồng cơ bản
1. Người dùng truy cập trang đăng nhập và nhập tên đăng nhập (username) cùng mật khẩu.
2. Hệ thống kiểm tra thông tin đăng nhập trong bảng `account`:
   - Xác minh tên đăng nhập tồn tại.
   - Xác minh mật khẩu khớp với hash đã lưu (BCrypt).
   - Kiểm tra tài khoản đang ở trạng thái hoạt động (`is_active = 1`).
3. Hệ thống cập nhật thời điểm đăng nhập cuối (`last_login_at`) và trả về token xác thực cùng thông tin tài khoản.
4. Đăng xuất: Người dùng kích "Đăng xuất"; hệ thống hủy phiên làm việc và chuyển về trang đăng nhập.

#### 3.2. Các luồng rẽ nhánh
1. Tại bước 2, nếu username hoặc password để trống, hệ thống hiển thị lỗi "Username và password không được để trống".
2. Tại bước 2, nếu tên đăng nhập không tồn tại hoặc mật khẩu sai, hệ thống trả về lỗi 401 Unauthorized.
3. Tại bước 2, nếu tài khoản bị khóa (`is_active = 0`), hệ thống trả về lỗi 403 "Tài khoản đã bị khóa".

### 4. Các yêu cầu đặc biệt
Không yêu cầu vai trò đặc biệt — bất kỳ người dùng nào cũng có thể đăng nhập.

### 5. Tiền điều kiện
Tài khoản người dùng đã được khởi tạo trong bảng `account`.

### 6. Hậu điều kiện
Nếu đăng nhập thành công, hệ thống lưu thời điểm đăng nhập và cấp token truy cập. Nếu đăng xuất, phiên làm việc bị hủy.

### 7. Điểm mở rộng
Không có.

---

## UC-02: Quản lý nhân viên

### 1. Tên Use Case
Quản lý nhân viên

### 2. Mô tả vắn tắt
Use case này cho phép người quản trị / HR xem, thêm, sửa, xóa hồ sơ nhân viên trong bảng `employee`. Khi tạo nhân viên mới, hệ thống tự động tạo tài khoản đăng nhập mặc định cho nhân viên đó.

### 3. Luồng các sự kiện

#### 3.1. Luồng cơ bản
1. Người dùng kích vào mục "Nhân viên" trên menu. Hệ thống lấy danh sách nhân viên từ view `vw_employee_detail` (bao gồm tên phòng ban, chức vụ, bằng cấp, ca làm việc) và hiển thị lên màn hình với chức năng phân trang, tìm kiếm, lọc.
2. **Thêm nhân viên:**
   a. Người dùng kích "Thêm mới". Hệ thống hiển thị form nhập thông tin.
   b. Người dùng nhập thông tin: mã nhân viên, họ tên, giới tính, ngày sinh, địa chỉ, số điện thoại, email, ngày vào làm, CCCD/CMND, bằng cấp, phòng ban, chức vụ, ca làm việc, thông tin ngân hàng, người liên hệ khẩn cấp, v.v.
   c. Hệ thống kiểm tra hợp lệ, tạo tài khoản đăng nhập mặc định (username = mã nhân viên, password = "123456"), lưu nhân viên vào bảng `employee` và cập nhật danh sách.
3. **Sửa nhân viên:**
   a. Người dùng kích "Sửa" trên một dòng nhân viên. Hệ thống lấy thông tin cũ và hiển thị lên form.
   b. Người dùng cập nhật thông tin và kích "Lưu". Hệ thống cập nhật bảng `employee`.
4. **Xóa nhân viên:**
   a. Người dùng kích "Xóa" hoặc chọn nhiều nhân viên rồi kích "Xóa hàng loạt".
   b. Hệ thống hiển thị xác nhận xóa.
   c. Người dùng xác nhận; hệ thống thực hiện soft-delete và cập nhật danh sách.
5. **Tìm kiếm / Lọc:** Người dùng nhập từ khóa hoặc chọn điều kiện lọc; hệ thống trả về danh sách phù hợp theo phân trang.

#### 3.2. Các luồng rẽ nhánh
1. Nếu mã nhân viên trống hoặc vượt quá 20 ký tự, hệ thống hiển thị lỗi tương ứng.
2. Nếu họ tên trống hoặc vượt quá 120 ký tự, hệ thống báo lỗi.
3. Nếu thiếu giới tính, ngày sinh, địa chỉ, số điện thoại, email, ngày vào làm, CCCD, bằng cấp, hệ thống báo lỗi bắt buộc.
4. Nếu mã nhân viên đã tồn tại, hệ thống báo lỗi "Mã nhân viên đã tồn tại".
5. Nếu mã nhân viên đã được dùng cho một tài khoản khác, hệ thống báo lỗi.
6. Người dùng kích "Hủy" để bỏ qua thao tác và quay lại danh sách.

### 4. Các yêu cầu đặc biệt
Chức năng thêm/sửa/xóa chỉ dành cho vai trò Admin và HR.

### 5. Tiền điều kiện
Người dùng đã đăng nhập với vai trò có quyền quản lý nhân viên.

### 6. Hậu điều kiện
Thông tin nhân viên và tài khoản đăng nhập tương ứng được cập nhật trong cơ sở dữ liệu.

### 7. Điểm mở rộng
- Xem danh sách nhân viên chưa có hợp đồng.
- Xem danh sách cán bộ đại diện ký hợp đồng (vai trò HR).
- Xuất danh sách nhân viên ra file Excel.

---

## UC-03: Quản lý ca làm việc

### 1. Tên Use Case
Quản lý ca làm việc

### 2. Mô tả vắn tắt
Use case này cho phép người quản trị xem, thêm, sửa, xóa và kích hoạt/vô hiệu hóa các ca làm việc trong bảng `shift`.

### 3. Luồng các sự kiện

#### 3.1. Luồng cơ bản
1. Người dùng kích vào mục "Ca làm việc". Hệ thống lấy danh sách ca từ bảng `shift` và hiển thị với phân trang, tìm kiếm.
2. **Thêm ca:** Người dùng nhập mã ca, tên ca, giờ bắt đầu, giờ kết thúc, giờ nghỉ giữa ca, tổng giờ làm, mô tả, trạng thái; hệ thống lưu vào bảng `shift`.
3. **Sửa ca:** Người dùng chỉnh sửa thông tin ca và lưu; hệ thống cập nhật bảng `shift`.
4. **Xóa ca / Xóa hàng loạt:** Người dùng xác nhận xóa; hệ thống thực hiện soft-delete.
5. **Kích hoạt / Vô hiệu hóa hàng loạt:** Người dùng chọn nhiều ca và cập nhật trạng thái `is_active`.
6. **Xuất Excel:** Người dùng kích "Xuất"; hệ thống tạo file Excel danh sách ca và trả về cho người dùng tải.

#### 3.2. Các luồng rẽ nhánh
1. Nếu thông tin nhập không hợp lệ, hệ thống hiển thị thông báo lỗi.
2. Người dùng kích "Hủy" để bỏ qua thao tác.

### 4. Các yêu cầu đặc biệt
Chỉ Admin và HR mới có quyền thêm, sửa, xóa.

### 5. Tiền điều kiện
Người dùng đã đăng nhập.

### 6. Hậu điều kiện
Thông tin ca làm việc được cập nhật trong cơ sở dữ liệu.

### 7. Điểm mở rộng
Xuất danh sách ca ra file Excel.

---

## UC-04: Quản lý phòng ban

### 1. Tên Use Case
Quản lý phòng ban

### 2. Mô tả vắn tắt
Use case này cho phép quản trị viên xem, thêm, sửa, xóa các phòng ban trong bảng `department`.

### 3. Luồng các sự kiện

#### 3.1. Luồng cơ bản
1. Người dùng vào mục "Phòng ban". Hệ thống hiển thị danh sách phòng ban (mã, tên, trưởng phòng, mô tả) với phân trang.
2. **Thêm:** Người dùng nhập thông tin và lưu.
3. **Sửa:** Người dùng chọn phòng ban, chỉnh sửa và lưu.
4. **Xóa / Xóa hàng loạt:** Người dùng xác nhận xóa; hệ thống soft-delete.

#### 3.2. Các luồng rẽ nhánh
1. Nếu thông tin không hợp lệ, hệ thống hiển thị lỗi.
2. Người dùng kích "Hủy" để bỏ qua.

### 4. Các yêu cầu đặc biệt
Chỉ Admin và HR mới có quyền thêm, sửa, xóa.

### 5. Tiền điều kiện
Người dùng đã đăng nhập.

### 6. Hậu điều kiện
Thông tin phòng ban được cập nhật trong cơ sở dữ liệu.

### 7. Điểm mở rộng
Không có.

---

## UC-05: Quản lý chức vụ

### 1. Tên Use Case
Quản lý chức vụ

### 2. Mô tả vắn tắt
Use case này cho phép quản trị viên xem, thêm, sửa, xóa các chức vụ trong bảng `position`.

### 3. Luồng các sự kiện

#### 3.1. Luồng cơ bản
1. Người dùng vào mục "Chức vụ". Hệ thống hiển thị danh sách chức vụ (mã, tên, mô tả) với phân trang.
2. **Thêm / Sửa / Xóa:** Tương tự UC-04.

#### 3.2. Các luồng rẽ nhánh
1. Nếu thông tin không hợp lệ, hệ thống hiển thị lỗi.

### 4. Các yêu cầu đặc biệt
Chỉ Admin và HR mới có quyền thêm, sửa, xóa.

### 5. Tiền điều kiện
Người dùng đã đăng nhập.

### 6. Hậu điều kiện
Thông tin chức vụ được cập nhật trong cơ sở dữ liệu.

### 7. Điểm mở rộng
Không có.

---

## UC-06: Quản lý bằng cấp

### 1. Tên Use Case
Quản lý bằng cấp

### 2. Mô tả vắn tắt
Use case này cho phép quản trị viên xem, thêm, sửa, xóa các loại bằng cấp trong bảng `degree`.

### 3. Luồng các sự kiện

#### 3.1. Luồng cơ bản
1. Người dùng vào mục "Bằng cấp". Hệ thống hiển thị danh sách bằng cấp (mã, tên) với phân trang.
2. **Thêm / Sửa / Xóa:** Tương tự UC-04.

#### 3.2. Các luồng rẽ nhánh
1. Nếu thông tin không hợp lệ, hệ thống hiển thị lỗi.

### 4. Các yêu cầu đặc biệt
Chỉ Admin và HR mới có quyền thêm, sửa, xóa.

### 5. Tiền điều kiện
Người dùng đã đăng nhập.

### 6. Hậu điều kiện
Thông tin bằng cấp được cập nhật trong cơ sở dữ liệu.

### 7. Điểm mở rộng
Không có.

---

## UC-07: Quản lý phụ cấp

### 1. Tên Use Case
Quản lý phụ cấp

### 2. Mô tả vắn tắt
Use case này cho phép quản trị viên xem, thêm, sửa, xóa các khoản phụ cấp (allowance) dùng để gắn vào hợp đồng nhân viên.

### 3. Luồng các sự kiện

#### 3.1. Luồng cơ bản
1. Người dùng vào mục "Phụ cấp". Hệ thống hiển thị danh sách phụ cấp (mã, tên, loại tính — cố định hoặc theo phần trăm, số tiền/tỷ lệ) với phân trang.
2. **Thêm:** Người dùng nhập mã, tên, loại tính (`fixed` hoặc `percent`), giá trị tương ứng và lưu.
3. **Sửa / Xóa:** Tương tự UC-04.

#### 3.2. Các luồng rẽ nhánh
1. Nếu thông tin không hợp lệ, hệ thống hiển thị lỗi.

### 4. Các yêu cầu đặc biệt
Chỉ Admin và HR mới có quyền thêm, sửa, xóa.

### 5. Tiền điều kiện
Người dùng đã đăng nhập.

### 6. Hậu điều kiện
Danh mục phụ cấp được cập nhật trong cơ sở dữ liệu.

### 7. Điểm mở rộng
Phụ cấp được gắn vào hợp đồng thông qua bảng `contract_allowance`.

---

## UC-08: Quản lý mẫu hợp đồng

### 1. Tên Use Case
Quản lý mẫu hợp đồng

### 2. Mô tả vắn tắt
Use case này cho phép quản trị viên xem, thêm, sửa, xóa các mẫu hợp đồng lao động trong bảng `contract_template`.

### 3. Luồng các sự kiện

#### 3.1. Luồng cơ bản
1. Người dùng vào mục "Mẫu hợp đồng". Hệ thống hiển thị danh sách mẫu hợp đồng (mã, tên, loại hợp đồng, phiên bản, trạng thái) với phân trang.
2. **Thêm:** Người dùng nhập mã, tên, loại hợp đồng, nội dung mẫu, trạng thái và lưu.
3. **Sửa / Xóa:** Tương tự UC-04.

#### 3.2. Các luồng rẽ nhánh
1. Nếu thông tin không hợp lệ, hệ thống hiển thị lỗi.

### 4. Các yêu cầu đặc biệt
Chỉ Admin và HR mới có quyền thêm, sửa, xóa.

### 5. Tiền điều kiện
Người dùng đã đăng nhập.

### 6. Hậu điều kiện
Mẫu hợp đồng được cập nhật trong cơ sở dữ liệu.

### 7. Điểm mở rộng
Mẫu hợp đồng được sử dụng khi tạo hợp đồng mới (UC-09).

---

## UC-09: Quản lý hợp đồng

### 1. Tên Use Case
Quản lý hợp đồng

### 2. Mô tả vắn tắt
Use case này cho phép quản trị viên / HR xem, thêm, sửa, xóa hợp đồng lao động trong bảng `contract`, bao gồm việc gắn phụ cấp, ký hợp đồng và theo dõi trạng thái hợp đồng.

### 3. Luồng các sự kiện

#### 3.1. Luồng cơ bản
1. Người dùng vào mục "Hợp đồng". Hệ thống hiển thị danh sách hợp đồng (mã, nhân viên, ngày hiệu lực, lương cơ bản, trạng thái, ngày kết thúc) với phân trang và lọc.
2. **Thêm hợp đồng:**
   a. Người dùng kích "Thêm mới". Hệ thống hiển thị form; danh sách nhân viên chưa có hợp đồng và danh sách mẫu hợp đồng được nạp sẵn.
   b. Người dùng chọn mẫu hợp đồng, nhân viên, đại diện công ty ký, chức danh người ký, ngày hiệu lực, thời hạn hợp đồng (tháng), lương cơ bản, lương đóng bảo hiểm, tỷ lệ lương, ca làm việc, danh sách phụ cấp đính kèm.
   c. Hệ thống kiểm tra hợp lệ và lưu hợp đồng cùng danh sách phụ cấp vào bảng `contract` và `contract_allowance`.
3. **Ký hợp đồng:** Người dùng đánh dấu `is_signed = true`; hệ thống tự động chuyển trạng thái sang `active` và ghi nhận ngày ký.
4. **Sửa / Xóa:** Tương tự UC-04.

#### 3.2. Các luồng rẽ nhánh
1. Nếu mã hợp đồng trống hoặc vượt quá 50 ký tự, hệ thống hiển thị lỗi.
2. Nếu không chọn mẫu hợp đồng, nhân viên hoặc đại diện công ty, hệ thống báo lỗi bắt buộc.
3. Nếu lương cơ bản, lương bảo hiểm hoặc tỷ lệ lương ≤ 0, hệ thống báo lỗi.
4. Nếu nhân viên ký trùng với đại diện công ty, hệ thống báo lỗi "Đại diện công ty không được trùng nhân viên ký hợp đồng".
5. Nếu mã hợp đồng đã tồn tại, hệ thống báo lỗi.

### 4. Các yêu cầu đặc biệt
Chỉ Admin và HR mới có quyền tạo và sửa hợp đồng.

### 5. Tiền điều kiện
Đã có mẫu hợp đồng, nhân viên, phụ cấp trong hệ thống.

### 6. Hậu điều kiện
Hợp đồng và các phụ cấp liên kết được lưu vào cơ sở dữ liệu; trạng thái hợp đồng được cập nhật khi ký.

### 7. Điểm mở rộng
Xuất hợp đồng đã chọn ra file Excel.

---

## UC-10: Quản lý chấm công

### 1. Tên Use Case
Quản lý chấm công

### 2. Mô tả vắn tắt
Use case này cho phép HR / quản trị viên xem, thêm, sửa, xóa bản ghi chấm công của nhân viên, xem bảng thống kê chấm công ngày hôm nay (dashboard) và xem lịch chấm công theo tháng của từng nhân viên.

### 3. Luồng các sự kiện

#### 3.1. Luồng cơ bản
1. Người dùng vào mục "Chấm công". Hệ thống hiển thị danh sách bản ghi chấm công (mã, nhân viên, ngày, giờ vào, giờ ra, số giờ công, giờ tăng ca, trạng thái) với phân trang.
2. **Thêm chấm công:**
   a. Người dùng nhập mã chấm công, nhân viên, ca làm việc, ngày, giờ check-in, giờ check-out, số giờ công, số giờ tăng ca, tiền phạt/thưởng, trạng thái.
   b. Hệ thống kiểm tra hợp lệ và lưu vào bảng `attendance`.
3. **Sửa / Xóa:** Tương tự UC-04.
4. **Xem Dashboard:** Người dùng kích "Dashboard"; hệ thống hiển thị:
   - Danh sách nhân viên vắng mặt hôm nay.
   - Bảng xếp hạng nhân viên đi muộn trong tháng.
   - Tổng số lần đi muộn trong tháng.
5. **Xem lịch chấm công:** Người dùng chọn nhân viên và tháng/năm; hệ thống hiển thị lịch chấm công dạng calendar kèm thống kê tổng ngày công, tổng giờ tăng ca, số lần đi muộn, số ngày nghỉ.

#### 3.2. Các luồng rẽ nhánh
1. Nếu mã chấm công trống, hệ thống báo lỗi.
2. Nếu nhân viên hoặc ngày chấm công không được chọn, hệ thống báo lỗi.
3. Nếu số giờ công hoặc tăng ca âm, hệ thống báo lỗi.
4. Nếu tiền phạt/thưởng âm, hệ thống báo lỗi.
5. Nếu mã chấm công đã tồn tại, hệ thống báo lỗi trùng.

### 4. Các yêu cầu đặc biệt
Chỉ Admin và HR mới có quyền thêm, sửa, xóa.

### 5. Tiền điều kiện
Đã có nhân viên và ca làm việc trong hệ thống.

### 6. Hậu điều kiện
Bản ghi chấm công được cập nhật trong cơ sở dữ liệu và dữ liệu tính lương sẽ sử dụng dữ liệu chấm công này.

### 7. Điểm mở rộng
Dữ liệu chấm công được dùng để tính ngày công thực tế trong quá trình tính lương (UC-18).

---

## UC-11: Quản lý công tác

### 1. Tên Use Case
Quản lý công tác

### 2. Mô tả vắn tắt
Use case này cho phép HR / quản trị viên xem, thêm, sửa, xóa các chuyến công tác của nhân viên trong bảng `business_trip`.

### 3. Luồng các sự kiện

#### 3.1. Luồng cơ bản
1. Người dùng vào mục "Công tác". Hệ thống hiển thị danh sách chuyến công tác (mã, nhân viên, địa điểm, mục đích, ngày đi, ngày về, tiền hỗ trợ) với phân trang.
2. **Thêm:** Người dùng nhập thông tin chuyến công tác và lưu.
3. **Sửa / Xóa:** Tương tự UC-04.

#### 3.2. Các luồng rẽ nhánh
1. Nếu thông tin không hợp lệ, hệ thống hiển thị lỗi.

### 4. Các yêu cầu đặc biệt
Chỉ Admin và HR mới có quyền thêm, sửa, xóa.

### 5. Tiền điều kiện
Người dùng đã đăng nhập; nhân viên đã tồn tại trong hệ thống.

### 6. Hậu điều kiện
Thông tin công tác được lưu vào bảng `business_trip`.

### 7. Điểm mở rộng
Danh sách nhân viên đang công tác hiển thị trên Dashboard (UC-21).

---

## UC-12: Quản lý đánh giá nhân viên

### 1. Tên Use Case
Quản lý đánh giá nhân viên

### 2. Mô tả vắn tắt
Use case này cho phép quản lý xem, thêm, sửa, xóa các bản ghi đánh giá (khen thưởng / kỷ luật) nhân viên trong bảng `evaluation`.

### 3. Luồng các sự kiện

#### 3.1. Luồng cơ bản
1. Người dùng vào mục "Đánh giá". Hệ thống hiển thị danh sách đánh giá (loại đánh giá, nhân viên, lý do, số tiền, ngày đánh giá) với phân trang.
2. **Thêm:** Người dùng chọn nhân viên, loại đánh giá (KHEN THƯỞNG / KỶ LUẬT), nhập lý do, số tiền, ngày và lưu.
3. **Sửa / Xóa:** Tương tự UC-04.

#### 3.2. Các luồng rẽ nhánh
1. Nếu thông tin không hợp lệ, hệ thống hiển thị lỗi.

### 4. Các yêu cầu đặc biệt
Chỉ Admin và HR / Quản lý mới có quyền thực hiện.

### 5. Tiền điều kiện
Nhân viên đã tồn tại trong hệ thống.

### 6. Hậu điều kiện
Bản ghi đánh giá được lưu. Nhân viên bị kỷ luật sẽ xuất hiện trong danh sách "nguy cơ nghỉ việc" trên Dashboard.

### 7. Điểm mở rộng
Số tiền đánh giá được tính vào phần cộng / khấu trừ trong khoản mục lương (UC-19).

---

## UC-13: Quản lý kỳ lương

### 1. Tên Use Case
Quản lý kỳ lương

### 2. Mô tả vắn tắt
Use case này cho phép quản trị viên / kế toán xem, thêm, sửa, xóa các kỳ lương trong bảng `salary_period`.

### 3. Luồng các sự kiện

#### 3.1. Luồng cơ bản
1. Người dùng vào mục "Kỳ lương". Hệ thống hiển thị danh sách kỳ lương (mã, tên, ngày bắt đầu, ngày kết thúc, trạng thái) với phân trang.
2. **Thêm:** Người dùng nhập mã, tên, ngày bắt đầu, ngày kết thúc của kỳ và lưu.
3. **Sửa / Xóa:** Tương tự UC-04.

#### 3.2. Các luồng rẽ nhánh
1. Nếu thông tin không hợp lệ, hệ thống hiển thị lỗi.

### 4. Các yêu cầu đặc biệt
Chỉ Admin và Kế toán mới có quyền thêm, sửa, xóa.

### 5. Tiền điều kiện
Người dùng đã đăng nhập với quyền phù hợp.

### 6. Hậu điều kiện
Kỳ lương được lưu; dùng làm tham chiếu khi tạo bảng lương (UC-18).

### 7. Điểm mở rộng
Không có.

---

## UC-14: Quản lý chính sách lương

### 1. Tên Use Case
Quản lý chính sách lương

### 2. Mô tả vắn tắt
Use case này cho phép quản trị viên xem, thêm, sửa, xóa các chính sách lương (salary policy) trong bảng `salary_policy`, quy định các tham số tính lương như hệ số lương, phụ cấp theo chính sách.

### 3. Luồng các sự kiện

#### 3.1. Luồng cơ bản
1. Người dùng vào mục "Chính sách lương". Hệ thống hiển thị danh sách chính sách lương với phân trang.
2. **Thêm / Sửa / Xóa:** Tương tự UC-04.

#### 3.2. Các luồng rẽ nhánh
1. Nếu thông tin không hợp lệ, hệ thống hiển thị lỗi.

### 4. Các yêu cầu đặc biệt
Chỉ Admin và Kế toán mới có quyền thêm, sửa, xóa.

### 5. Tiền điều kiện
Người dùng đã đăng nhập.

### 6. Hậu điều kiện
Chính sách lương được áp dụng vào quá trình tính lương.

### 7. Điểm mở rộng
Không có.

---

## UC-15: Quản lý chính sách khấu trừ

### 1. Tên Use Case
Quản lý chính sách khấu trừ

### 2. Mô tả vắn tắt
Use case này cho phép quản trị viên xem, thêm, sửa, xóa các chính sách khấu trừ (deduction policy) trong bảng `deduction_policy`, quy định các tỷ lệ bảo hiểm xã hội, bảo hiểm y tế, bảo hiểm thất nghiệp, giảm trừ gia cảnh.

### 3. Luồng các sự kiện

#### 3.1. Luồng cơ bản
1. Người dùng vào mục "Chính sách khấu trừ". Hệ thống hiển thị danh sách (mã, tên, tỷ lệ BHXH, BHYT, BHTN, mức giảm trừ bản thân, giảm trừ người phụ thuộc, thời hạn áp dụng) với phân trang.
2. **Thêm:** Người dùng nhập các tỷ lệ khấu trừ, mức giảm trừ, thời gian hiệu lực và lưu.
3. **Sửa / Xóa:** Tương tự UC-04.

#### 3.2. Các luồng rẽ nhánh
1. Nếu thông tin không hợp lệ, hệ thống hiển thị lỗi.

### 4. Các yêu cầu đặc biệt
Chỉ Admin và Kế toán mới có quyền thêm, sửa, xóa.

### 5. Tiền điều kiện
Người dùng đã đăng nhập.

### 6. Hậu điều kiện
Chính sách khấu trừ được sử dụng trong quá trình tính thuế và lương ròng.

### 7. Điểm mở rộng
Không có.

---

## UC-16: Quản lý khung thuế thu nhập cá nhân

### 1. Tên Use Case
Quản lý khung thuế thu nhập cá nhân

### 2. Mô tả vắn tắt
Use case này cho phép quản trị viên xem, thêm, sửa, xóa các bậc thuế thu nhập cá nhân (tax bracket) trong bảng `tax_bracket`.

### 3. Luồng các sự kiện

#### 3.1. Luồng cơ bản
1. Người dùng vào mục "Khung thuế". Hệ thống hiển thị danh sách các bậc thuế (thu nhập tối thiểu, thu nhập tối đa, thuế suất %) với phân trang.
2. **Thêm / Sửa / Xóa:** Tương tự UC-04.

#### 3.2. Các luồng rẽ nhánh
1. Nếu thông tin không hợp lệ, hệ thống hiển thị lỗi.

### 4. Các yêu cầu đặc biệt
Chỉ Admin và Kế toán mới có quyền thêm, sửa, xóa.

### 5. Tiền điều kiện
Người dùng đã đăng nhập.

### 6. Hậu điều kiện
Khung thuế được dùng để tính thuế TNCN trong quá trình tính lương.

### 7. Điểm mở rộng
Không có.

---

## UC-17: Quản lý hồ sơ thuế nhân viên

### 1. Tên Use Case
Quản lý hồ sơ thuế nhân viên

### 2. Mô tả vắn tắt
Use case này cho phép quản trị viên / kế toán xem, thêm, sửa, xóa hồ sơ thuế của từng nhân viên (employee tax profile) trong bảng `employee_tax_profile`, bao gồm số người phụ thuộc và các khoản giảm trừ cá nhân.

### 3. Luồng các sự kiện

#### 3.1. Luồng cơ bản
1. Người dùng vào mục "Hồ sơ thuế". Hệ thống hiển thị danh sách hồ sơ thuế theo nhân viên với phân trang.
2. **Thêm / Sửa / Xóa:** Tương tự UC-04.

#### 3.2. Các luồng rẽ nhánh
1. Nếu thông tin không hợp lệ, hệ thống hiển thị lỗi.

### 4. Các yêu cầu đặc biệt
Chỉ Admin và Kế toán mới có quyền thêm, sửa, xóa.

### 5. Tiền điều kiện
Nhân viên đã tồn tại trong hệ thống.

### 6. Hậu điều kiện
Hồ sơ thuế được áp dụng vào tính toán thuế TNCN của nhân viên trong kỳ lương.

### 7. Điểm mở rộng
Không có.

---

## UC-18: Tính lương và quản lý bảng lương

### 1. Tên Use Case
Tính lương và quản lý bảng lương

### 2. Mô tả vắn tắt
Use case này cho phép kế toán / Admin thực hiện toàn bộ quy trình tính lương cho nhân viên trong một kỳ lương: tạo nháp bảng lương, tính toán lương thực tế, khóa bảng lương và đánh dấu đã thanh toán.

### 3. Luồng các sự kiện

#### 3.1. Luồng cơ bản
1. Người dùng vào mục "Bảng lương". Hệ thống hiển thị danh sách bảng lương theo kỳ lương (mã phiếu lương, nhân viên, lương gross, lương net, thuế TNCN, khấu trừ bảo hiểm, trạng thái) với phân trang.
2. **Tạo nháp (Generate):**
   a. Người dùng chọn kỳ lương và kích "Tạo bảng lương nháp".
   b. Hệ thống tự động tạo bản ghi bảng lương trạng thái `draft` cho tất cả nhân viên đang có hợp đồng active trong kỳ đó.
3. **Tính lương (Calculate):**
   a. Người dùng kích "Tính lương" cho cả kỳ hoặc chọn một nhân viên cụ thể.
   b. Hệ thống tính toán theo quy trình:
      - Lấy thông tin hợp đồng, chính sách lương, chính sách khấu trừ, hồ sơ thuế nhân viên.
      - Tính ngày công thực tế dựa trên dữ liệu chấm công.
      - Tính lương gross = (lương cơ bản / ngày chuẩn × ngày thực) + phụ cấp hợp đồng + thưởng.
      - Tính khấu trừ bảo hiểm (BHXH, BHYT, BHTN).
      - Tính thu nhập chịu thuế và thuế TNCN theo biểu thuế lũy tiến.
      - Tính lương net = gross − bảo hiểm − thuế.
      - Lưu kết quả và chi tiết các khoản mục vào `payroll` và `payroll_item`.
4. **Khóa bảng lương (Lock):** Người dùng xác nhận khóa kỳ lương; hệ thống chuyển trạng thái sang `locked`, ghi nhận thời điểm khóa. Không thể chỉnh sửa sau khi khóa.
5. **Đánh dấu đã thanh toán (Pay):** Người dùng xác nhận đã chi trả; hệ thống chuyển trạng thái sang `paid`, ghi nhận thời điểm thanh toán.

#### 3.2. Các luồng rẽ nhánh
1. Nếu kỳ lương chưa tồn tại hoặc nhân viên không có hợp đồng active, hệ thống bỏ qua.
2. Nếu bảng lương đã khóa, mọi thao tác sửa/xóa đều bị từ chối.
3. Nếu bảng lương đã thanh toán, không thể thực hiện thêm thao tác nào.

### 4. Các yêu cầu đặc biệt
Chỉ Admin và Kế toán mới có quyền thực hiện.

### 5. Tiền điều kiện
Đã có kỳ lương, hợp đồng nhân viên, chính sách lương, chính sách khấu trừ, khung thuế, hồ sơ thuế và dữ liệu chấm công.

### 6. Hậu điều kiện
Bảng lương được tính toán, lưu vào bảng `payroll` và `payroll_item`; trạng thái được cập nhật qua từng bước của quy trình.

### 7. Điểm mở rộng
Xem chi tiết khoản mục lương của từng phiếu lương (UC-19).

---

## UC-19: Quản lý khoản mục lương

### 1. Tên Use Case
Quản lý khoản mục lương

### 2. Mô tả vắn tắt
Use case này cho phép xem và quản lý các khoản mục chi tiết cấu thành phiếu lương (payroll item) trong bảng `payroll_item`, bao gồm các khoản cộng (phụ cấp, thưởng) và các khoản khấu trừ (bảo hiểm, thuế, kỷ luật).

### 3. Luồng các sự kiện

#### 3.1. Luồng cơ bản
1. Người dùng xem chi tiết một phiếu lương. Hệ thống lấy tất cả khoản mục lương theo `payroll_id` và hiển thị danh sách (tên khoản mục, loại — cộng/trừ, số tiền).
2. **Thêm / Sửa / Xóa khoản mục:** HR / Kế toán có thể điều chỉnh thủ công các khoản mục trước khi khóa bảng lương.

#### 3.2. Các luồng rẽ nhánh
1. Nếu bảng lương đã khóa, không thể thêm/sửa/xóa khoản mục.

### 4. Các yêu cầu đặc biệt
Chỉ Admin và Kế toán mới có quyền chỉnh sửa.

### 5. Tiền điều kiện
Bảng lương đã được tạo (UC-18).

### 6. Hậu điều kiện
Khoản mục lương được cập nhật; ảnh hưởng đến tổng lương gross/net của phiếu lương.

### 7. Điểm mở rộng
Không có.

---

## UC-20: Quản lý yêu cầu phê duyệt

### 1. Tên Use Case
Quản lý yêu cầu phê duyệt

### 2. Mô tả vắn tắt
Use case này cho phép nhân viên / HR tạo yêu cầu phê duyệt (thuyên chuyển phòng ban, đổi trưởng phòng, thay đổi hợp đồng, đơn nghỉ phép) và người có thẩm quyền duyệt / từ chối từng bước theo luồng phê duyệt định sẵn.

### 3. Luồng các sự kiện

#### 3.1. Luồng cơ bản
1. Người dùng vào mục "Phê duyệt". Hệ thống hiển thị danh sách yêu cầu (mã, loại, tiêu đề, người tạo, trạng thái, bước hiện tại) với phân trang.
2. **Tạo yêu cầu:**
   a. Người dùng kích "Tạo yêu cầu", chọn loại yêu cầu, nhập tiêu đề, mô tả, ngày hiệu lực và payload dữ liệu thay đổi (JSON).
   b. Hệ thống tự sinh mã yêu cầu, xác định số bước duyệt theo loại yêu cầu:
      - `department_member_transfer`, `department_manager_change`, `contract_change`: 1 bước — ADMIN duyệt.
      - `leave_request`: 2 bước — MANAGER duyệt → HR duyệt.
   c. Hệ thống lưu yêu cầu và các bước phê duyệt vào bảng `approval_request` và `approval_step`.
3. **Phê duyệt bước:**
   a. Người có thẩm quyền xem yêu cầu đang pending và kích "Duyệt".
   b. Hệ thống kiểm tra đúng thứ tự bước và vai trò người duyệt, cập nhật bước sang `approved`.
   c. Nếu là bước cuối, hệ thống chuyển yêu cầu sang `approved` và tự động thực thi thay đổi (thuyên chuyển phòng ban, đổi trưởng phòng, v.v.).
   d. Nếu chưa phải bước cuối, hệ thống chuyển sang bước tiếp theo.
4. **Từ chối bước:**
   a. Người có thẩm quyền kích "Từ chối" và nhập lý do.
   b. Hệ thống cập nhật bước sang `rejected` và chuyển toàn bộ yêu cầu sang trạng thái `rejected`.

#### 3.2. Các luồng rẽ nhánh
1. Nếu loại yêu cầu, tiêu đề hoặc payload để trống, hệ thống báo lỗi.
2. Nếu loại yêu cầu không thuộc danh sách hỗ trợ, hệ thống báo lỗi.
3. Nếu yêu cầu đã được xử lý (approved/rejected), không thể duyệt/từ chối thêm.
4. Nếu duyệt không đúng thứ tự bước, hệ thống báo lỗi "Phải duyệt theo thứ tự".
5. Nếu bước đã được xử lý, không thể duyệt/từ chối lại.

### 4. Các yêu cầu đặc biệt
- Nhân viên chỉ có thể tạo `leave_request`.
- HR có thể tạo `department_member_transfer`, `department_manager_change`, `contract_change`.
- ADMIN duyệt các yêu cầu của HR; MANAGER và HR duyệt đơn nghỉ phép.

### 5. Tiền điều kiện
Người dùng đã đăng nhập với vai trò phù hợp.

### 6. Hậu điều kiện
Khi yêu cầu được phê duyệt hoàn tất, hệ thống tự động thực thi thay đổi tương ứng trong cơ sở dữ liệu (cập nhật phòng ban, trưởng phòng, v.v.) và ghi lịch sử thay đổi.

### 7. Điểm mở rộng
Xem chi tiết các bước phê duyệt của một yêu cầu.

---

## UC-21: Xem tổng quan hệ thống (Dashboard)

### 1. Tên Use Case
Xem tổng quan hệ thống (Dashboard)

### 2. Mô tả vắn tắt
Use case này cho phép người quản trị / HR xem nhanh các chỉ số tổng hợp về nhân sự và hoạt động của doanh nghiệp trong khoảng thời gian được chọn (tuần, tháng, năm).

### 3. Luồng các sự kiện

#### 3.1. Luồng cơ bản
1. Người dùng kích vào mục "Dashboard" hoặc truy cập trang chủ sau khi đăng nhập.
2. Người dùng chọn khoảng thời gian lọc (tuần / tháng / năm, mặc định: tháng).
3. Hệ thống truy vấn và hiển thị các chỉ số tổng hợp:
   - **Tổng nhân viên** đang có trong hệ thống.
   - **Tổng chi lương** (tổng lương net) trong kỳ được chọn.
   - **Số nhân viên đang công tác** (có chuyến công tác trùng ngày hôm nay).
   - **Hợp đồng sắp hết hạn** (trong vòng 30 ngày tới) kèm danh sách chi tiết.
   - **Nhân viên chưa có hợp đồng** kèm danh sách chi tiết.
   - **Hợp đồng chưa ký** kèm danh sách chi tiết.
   - **Nhân viên có nguy cơ nghỉ việc** (bị kỷ luật trong kỳ) kèm danh sách chi tiết.
   - **Nhân viên mới** gia nhập trong kỳ được chọn.
   - **Danh sách nhân viên đang công tác** (tối đa 10 bản ghi).

#### 3.2. Các luồng rẽ nhánh
1. Nếu không kết nối được cơ sở dữ liệu, hệ thống hiển thị thông báo lỗi.
2. Nếu không có dữ liệu cho kỳ được chọn, hệ thống hiển thị các chỉ số bằng 0.

### 4. Các yêu cầu đặc biệt
Chức năng này chỉ cho phép xem, không cho phép sửa dữ liệu.

### 5. Tiền điều kiện
Người dùng đã đăng nhập với vai trò Admin hoặc HR.

### 6. Hậu điều kiện
Không có thay đổi dữ liệu. Thông tin chỉ được đọc từ cơ sở dữ liệu.

### 7. Điểm mở rộng
Không có.
