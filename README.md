# PetCare - Hệ thống quản lý chăm sóc thú cưng

Dự án ASP.NET MVC quản lý dịch vụ chăm sóc thú cưng với các chức năng đặt lịch, quản lý khách hàng, nhân viên và dịch vụ.

## 🚀 Tính năng

### Khách hàng (Customer)
- Đăng ký/Đăng nhập tài khoản
- Quản lý thông tin thú cưng
- Đặt lịch hẹn dịch vụ
- Xem lịch sử và trạng thái lịch hẹn

### Nhân viên (Staff)
- ✅ Dashboard với thống kê lịch hẹn
- ✅ Xem danh sách lịch hẹn (có bộ lọc)
- ✅ Xem chi tiết lịch hẹn
- ✅ Xác nhận/Từ chối lịch hẹn
- ✅ Hoàn thành lịch hẹn (có upload ảnh)

### Quản trị viên (Admin)
- Quản lý người dùng
- Quản lý dịch vụ
- Xem báo cáo thống kê

## 📋 Yêu cầu hệ thống

- Visual Studio 2019 trở lên
- .NET Framework 4.8
- SQL Server 2016 trở lên
- Entity Framework 6

## 🔧 Cài đặt

### 1. Clone repository

```bash
git clone https://github.com/your-username/125_BCCK.git
cd 125_BCCK
```

### 2. Cấu hình Database

#### Bước 1: Tạo file Web.config
```bash
copy Web.config.template Web.config
```

#### Bước 2: Sửa connection string trong Web.config
Mở file `Web.config` và thay đổi:
```xml
<connectionStrings>
    <add name="PetCareDBEntities" 
         connectionString="data source=YOUR_SERVER_NAME;initial catalog=PetCareDB;user id=YOUR_USERNAME;password=YOUR_PASSWORD;trustservercertificate=True;MultipleActiveResultSets=True;App=EntityFramework" 
         providerName="System.Data.SqlClient"/>
</connectionStrings>
```

Thay thế:
- `YOUR_SERVER_NAME`: Tên SQL Server của bạn (VD: `localhost\SQLEXPRESS`)
- `YOUR_USERNAME`: Username SQL Server (VD: `sa`)
- `YOUR_PASSWORD`: Password SQL Server

#### Bước 3: Chạy script tạo database
Mở SQL Server Management Studio và chạy file `DVthucung.sql`

### 3. Restore NuGet Packages

Trong Visual Studio:
1. Right-click vào Solution
2. Chọn "Restore NuGet Packages"

### 4. Build và chạy

1. Nhấn `Ctrl + Shift + B` để build
2. Nhấn `F5` để chạy

## 🧪 Test

### Tài khoản test có sẵn:

**Admin:**
- Email: `admin@petcare.com`
- Password: `admin123`

**Staff:**
- Email: `huong.staff@petcare.com` / Password: `staff123`
- Email: `cuong.staff@petcare.com` / Password: `staff123`

**Customer:**
- Email: `tuan.customer@gmail.com` / Password: `customer123`

### Test nhanh (không cần đăng nhập thật):

Truy cập: `https://localhost:44336/Test`

Trang này cho phép đăng nhập giả lập để test các chức năng.

**LƯU Ý:** Nhớ xóa `TestController` và `DebugController` trước khi deploy production!

## 📁 Cấu trúc thư mục

```
125_BCCK/
├── Controllers/          # Controllers
│   ├── AccountController.cs
│   ├── HomeController.cs
│   ├── StaffController.cs      # ✅ Module Staff
│   ├── TestController.cs       # ⚠️ Chỉ dùng test
│   └── DebugController.cs      # ⚠️ Chỉ dùng debug
├── Models/              # Models & ViewModels
│   ├── Appointment.cs
│   ├── Pet.cs
│   ├── User.cs
│   ├── Service.cs
│   └── ViewModels/
├── Views/               # Razor Views
│   ├── Staff/          # ✅ Views cho Staff
│   ├── Test/           # ⚠️ Chỉ dùng test
│   └── Debug/          # ⚠️ Chỉ dùng debug
├── Content/            # CSS, Images
├── Scripts/            # JavaScript
├── images/
│   └── appointments/   # Ảnh upload từ Staff
├── DVthucung.sql       # Script tạo database
└── Web.config.template # Template config
```

## 🔒 Bảo mật

### Các file KHÔNG nên commit lên Git:
- ✅ `Web.config` - Chứa connection string nhạy cảm
- ✅ `bin/`, `obj/` - Build output
- ✅ `packages/` - NuGet packages
- ✅ `images/appointments/*` - Ảnh upload của user
- ✅ `.vs/` - Visual Studio cache

### Các file NÊN commit:
- ✅ `Web.config.template` - Template để người khác tham khảo
- ✅ `DVthucung.sql` - Script database
- ✅ Source code (.cs, .cshtml)
- ✅ `README.md`, `STAFF_MODULE_README.md`

## 📝 Module Staff - Chi tiết

Xem file `STAFF_MODULE_README.md` để biết chi tiết về module Staff.

## 🚨 Trước khi deploy Production

1. **Xóa các controller test:**
   - `Controllers/TestController.cs`
   - `Controllers/DebugController.cs`

2. **Xóa các view test:**
   - `Views/Test/`
   - `Views/Debug/`

3. **Kiểm tra Web.config:**
   - Đảm bảo connection string đúng
   - Set `debug="false"` trong compilation

4. **Xóa dữ liệu test trong database**

## 👥 Nhóm phát triển

- Thành viên 1: [Tên]
- Thành viên 2: [Tên]
- Thành viên 3: [Tên]

## 📄 License

[Chọn license phù hợp]

## 📞 Liên hệ

- Email: [email]
- GitHub: [link]
