# Module Nhân Viên (Staff) - Hệ thống PetCare

## Tổng quan
Module này cung cấp đầy đủ chức năng cho nhân viên quản lý và xử lý lịch hẹn của khách hàng.

## Các chức năng đã hoàn thành

### ✅ 4.1. Dashboard Staff
- **URL**: `/Staff/Index`
- **Chức năng**:
  - Hiển thị số lịch hẹn hôm nay
  - Hiển thị số lịch hẹn tuần này
  - Danh sách chi tiết lịch hẹn hôm nay

### ✅ 4.2. Lịch hẹn được giao
- **URL**: `/Staff/Appointments`
- **Chức năng**:
  - Lọc theo thời gian: hôm nay, tuần này, tháng này
  - Lọc theo trạng thái: Pending, Confirmed, Completed, Cancelled
  - Hiển thị danh sách lịch hẹn với đầy đủ thông tin

### ✅ 4.3. Chi tiết lịch hẹn
- **URL**: `/Staff/Details/{id}`
- **Chức năng**:
  - Thông tin khách hàng (tên, email, SĐT, địa chỉ)
  - Thông tin thú cưng (tên, loài, giống, tuổi, cân nặng, ghi chú đặc biệt)
  - Danh sách dịch vụ đã đặt
  - Ghi chú từ khách hàng

### ✅ 4.4. Quy trình xử lý

#### Xác nhận lịch hẹn
- **Điều kiện**: Chỉ khi status = "Pending"
- **Action**: POST `/Staff/Confirm`
- **Kết quả**: Chuyển status sang "Confirmed"

#### Từ chối lịch hẹn
- **Action**: POST `/Staff/Cancel`
- **Yêu cầu**: Nhập lý do từ chối
- **Kết quả**: 
  - Cập nhật status = "Cancelled"
  - Lưu lý do vào CancelReason

#### Hoàn thành lịch hẹn
- **URL**: `/Staff/Complete/{id}`
- **Yêu cầu nhập**:
  - Tình trạng thú cưng (bắt buộc)
  - Ghi chú bổ sung
  - Upload ảnh (tùy chọn)
- **Kết quả**: 
  - Cập nhật status = "Completed"
  - Lưu thông tin vào StaffNotes

## Cấu trúc Files

### Models
- `Models/Appointment.cs` - Model lịch hẹn
- `Models/Pet.cs` - Model thú cưng
- `Models/AppointmentService.cs` - Model dịch vụ trong lịch hẹn

### ViewModels
- `Models/ViewModels/StaffDashboardViewModel.cs` - Dashboard
- `Models/ViewModels/AppointmentListItemViewModel.cs` - Danh sách lịch hẹn
- `Models/ViewModels/AppointmentDetailViewModel.cs` - Chi tiết lịch hẹn
- `Models/ViewModels/CompleteAppointmentViewModel.cs` - Form hoàn thành

### Controller
- `Controllers/StaffController.cs` - Xử lý tất cả logic Staff

### Views
- `Views/Staff/Index.cshtml` - Dashboard
- `Views/Staff/Appointments.cshtml` - Danh sách lịch hẹn
- `Views/Staff/Details.cshtml` - Chi tiết lịch hẹn
- `Views/Staff/Complete.cshtml` - Form hoàn thành

### CSS
- `Content/css/style.css` - Đã thêm styles cho Staff module

## Cách sử dụng

### 1. Đăng nhập với tài khoản Staff
```
Email: huong.staff@petcare.com
Password: staff123
```
hoặc
```
Email: cuong.staff@petcare.com
Password: staff123
```

### 2. Truy cập Dashboard
- Sau khi đăng nhập, click vào menu dropdown và chọn "Dashboard"
- Hoặc truy cập trực tiếp: `/Staff/Index`

### 3. Xem danh sách lịch hẹn
- Click "Xem tất cả lịch hẹn" hoặc menu "Lịch hẹn"
- Sử dụng bộ lọc để tìm lịch hẹn theo thời gian và trạng thái

### 4. Xử lý lịch hẹn
- Click "Chi tiết" để xem thông tin đầy đủ
- Với lịch Pending: Có thể Xác nhận hoặc Từ chối
- Với lịch Confirmed: Có thể Hoàn thành

## Trạng thái lịch hẹn

| Trạng thái | Mô tả | Màu hiển thị |
|------------|-------|--------------|
| Pending | Chờ xử lý | Vàng (Warning) |
| Confirmed | Đã xác nhận | Xanh dương (Info) |
| Completed | Hoàn thành | Xanh lá (Success) |
| Cancelled | Đã hủy | Đỏ (Danger) |

## Lưu ý kỹ thuật

### Database Context
- Đã cập nhật `PetCareContext.cs` để include các DbSet mới:
  - `DbSet<Pet> Pets`
  - `DbSet<Appointment> Appointments`
  - `DbSet<AppointmentService> AppointmentServices`

### Navigation Properties
- Sử dụng `Include()` để load related data (Customer, Pet, Staff, Services)
- Tránh N+1 query problem

### Session Management
- Kiểm tra đăng nhập: `Session["UserId"]`
- Kiểm tra role: `Session["Role"] == "Staff"`
- Redirect về Login nếu chưa đăng nhập hoặc không phải Staff

### AJAX Calls
- Xác nhận lịch hẹn: POST với jQuery
- Từ chối lịch hẹn: POST với jQuery + Modal
- Sử dụng JSON response để thông báo kết quả

## Testing

### Test Cases cần kiểm tra:

1. **Dashboard**
   - [ ] Hiển thị đúng số lịch hôm nay
   - [ ] Hiển thị đúng số lịch tuần này
   - [ ] Danh sách lịch hôm nay hiển thị đầy đủ

2. **Danh sách lịch hẹn**
   - [ ] Lọc theo hôm nay
   - [ ] Lọc theo tuần này
   - [ ] Lọc theo tháng này
   - [ ] Lọc theo trạng thái

3. **Chi tiết lịch hẹn**
   - [ ] Hiển thị đầy đủ thông tin khách hàng
   - [ ] Hiển thị đầy đủ thông tin thú cưng
   - [ ] Hiển thị danh sách dịch vụ
   - [ ] Hiển thị ghi chú

4. **Xác nhận lịch hẹn**
   - [ ] Chỉ hiển thị nút với lịch Pending
   - [ ] Cập nhật status thành công
   - [ ] Hiển thị thông báo

5. **Từ chối lịch hẹn**
   - [ ] Bắt buộc nhập lý do
   - [ ] Cập nhật status và lý do
   - [ ] Hiển thị thông báo

6. **Hoàn thành lịch hẹn**
   - [ ] Chỉ hiển thị với lịch Confirmed
   - [ ] Validate form đầy đủ
   - [ ] Lưu thông tin vào StaffNotes
   - [ ] Cập nhật status thành Completed

## Mở rộng trong tương lai

- [ ] Thêm chức năng upload ảnh thực tế (hiện tại chỉ nhập URL)
- [ ] Thêm notification realtime khi có lịch hẹn mới
- [ ] Thêm báo cáo thống kê cho nhân viên
- [ ] Thêm lịch sử xử lý của nhân viên
- [ ] Thêm đánh giá từ khách hàng cho nhân viên
