# HƯỚNG DẪN TEST MODULE STAFF

## Cách test nhanh (không cần đăng nhập thật)

### Bước 1: Chạy project
1. Build project trong Visual Studio (Ctrl + Shift + B)
2. Chạy project (F5 hoặc Ctrl + F5)

### Bước 2: Truy cập trang Test
Mở trình duyệt và truy cập:
```
https://localhost:44336/Test
```
hoặc
```
http://localhost:[port]/Test
```

### Bước 3: Chọn tài khoản Staff để test
Trên trang Test, click vào nút **"Đăng nhập"** của:
- **Nhân viên 1**: Trần Thị Hương (Staff ID: 2)
- **Nhân viên 2**: Lê Văn Cường (Staff ID: 3)

### Bước 4: Test các chức năng

Sau khi "đăng nhập", bạn sẽ được chuyển đến Staff Dashboard. Từ đó có thể test:

#### ✅ Dashboard (Trang chủ Staff)
- Xem số lịch hẹn hôm nay
- Xem số lịch hẹn tuần này
- Xem danh sách lịch hẹn hôm nay

#### ✅ Danh sách lịch hẹn
- Click "Xem tất cả lịch hẹn" hoặc menu "Lịch hẹn"
- Test bộ lọc:
  - Lọc theo thời gian: Hôm nay / Tuần này / Tháng này
  - Lọc theo trạng thái: Pending / Confirmed / Completed / Cancelled

#### ✅ Chi tiết lịch hẹn
- Click nút "Chi tiết" (icon mắt) ở bất kỳ lịch hẹn nào
- Xem đầy đủ thông tin:
  - Thông tin khách hàng
  - Thông tin thú cưng
  - Danh sách dịch vụ
  - Ghi chú

#### ✅ Xác nhận lịch hẹn
- Vào chi tiết lịch hẹn có status = **Pending**
- Click nút **"Xác nhận lịch hẹn"**
- Kiểm tra status chuyển sang **Confirmed**

#### ✅ Từ chối lịch hẹn
- Vào chi tiết lịch hẹn có status = **Pending**
- Click nút **"Từ chối lịch hẹn"**
- Nhập lý do từ chối trong popup
- Click **"Xác nhận từ chối"**
- Kiểm tra status chuyển sang **Cancelled**

#### ✅ Hoàn thành lịch hẹn
- Vào chi tiết lịch hẹn có status = **Confirmed**
- Click nút **"Hoàn thành lịch hẹn"**
- Điền form:
  - Tình trạng thú cưng (bắt buộc)
  - Ghi chú (tùy chọn)
  - Đường dẫn ảnh (tùy chọn)
- Click **"Hoàn thành lịch hẹn"**
- Kiểm tra status chuyển sang **Completed**

## Dữ liệu test trong database

### Staff
- **ID 2**: Trần Thị Hương - có lịch hẹn #1, #3
- **ID 3**: Lê Văn Cường - có lịch hẹn #2, #4

### Lịch hẹn mẫu
| ID | Ngày | Staff | Status | Ghi chú |
|----|------|-------|--------|---------|
| 1 | 15/11/2024 | Staff 2 | Completed | Đã hoàn thành |
| 2 | 18/11/2024 | Staff 3 | Completed | Đã hoàn thành |
| 3 | 20/12/2024 | Staff 2 | Confirmed | Có thể hoàn thành |
| 4 | 22/12/2024 | Staff 3 | Confirmed | Có thể hoàn thành |
| 5 | 23/12/2024 | NULL | Pending | Chưa có staff, có thể xác nhận/từ chối |
| 6 | 24/12/2024 | NULL | Pending | Chưa có staff, có thể xác nhận/từ chối |
| 7 | 20/11/2024 | NULL | Cancelled | Đã bị hủy |

## Quick Links để test

Sau khi đăng nhập giả lập, có thể truy cập trực tiếp:

- Dashboard: `/Staff/Index`
- Danh sách lịch hẹn: `/Staff/Appointments`
- Lịch hôm nay: `/Staff/Appointments?filter=today`
- Lịch chờ xử lý: `/Staff/Appointments?status=Pending`
- Chi tiết lịch #5: `/Staff/Details/5` (Pending - có thể xác nhận/từ chối)
- Chi tiết lịch #3: `/Staff/Details/3` (Confirmed - có thể hoàn thành)

## Lưu ý quan trọng

### ⚠️ Về TestController
- **TestController chỉ dùng để TEST**
- **PHẢI XÓA trước khi deploy production**
- File cần xóa:
  - `Controllers/TestController.cs`
  - `Views/Test/Index.cshtml`
  - Folder `Views/Test/`

### ⚠️ Về dữ liệu test
- Nếu muốn reset dữ liệu, chạy lại file `DVthucung.sql`
- Các thao tác xác nhận/từ chối/hoàn thành sẽ thay đổi database thật

## Troubleshooting

### Lỗi: "Không tìm thấy lịch hẹn"
- Kiểm tra xem Staff ID trong session có khớp với StaffId trong database không
- Lịch hẹn #5, #6 có StaffId = NULL nên sẽ không hiện trong danh sách của Staff

### Lỗi: "Chỉ có thể xác nhận lịch hẹn đang chờ xử lý"
- Kiểm tra status của lịch hẹn, chỉ có thể xác nhận khi status = "Pending"

### Lỗi: Connection String
- Kiểm tra file `Web.config`
- Đảm bảo connection string trỏ đúng database `PetCareDB`

## Khi nào xóa TestController?

Xóa TestController khi:
- ✅ Đã test xong tất cả chức năng
- ✅ Phần đăng nhập đã hoạt động bình thường
- ✅ Chuẩn bị deploy lên server production

Cách xóa:
1. Xóa file `Controllers/TestController.cs`
2. Xóa folder `Views/Test/`
3. Xóa các dòng liên quan trong file `.csproj`
4. Build lại project
