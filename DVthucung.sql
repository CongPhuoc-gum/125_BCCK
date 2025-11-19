IF DB_ID('PetCareDB') IS NOT NULL
BEGIN
    ALTER DATABASE PetCareDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE PetCareDB;
    PRINT N'Đã xóa database cũ thành công.';
END
GO

-- Tạo database mới
CREATE DATABASE PetCareDB;
GO

USE PetCareDB;
GO

CREATE TABLE Users (
    UserId INT PRIMARY KEY IDENTITY(1,1),
    FullName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    Phone NVARCHAR(20) NULL,
    Address NVARCHAR(255) NULL,
    Role NVARCHAR(20) NOT NULL DEFAULT 'Customer',
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NULL,
    
    CONSTRAINT CK_Users_Role CHECK (Role IN ('Admin', 'Staff', 'Customer'))
);
GO

CREATE TABLE Pets (
    PetId INT PRIMARY KEY IDENTITY(1,1),
    OwnerId INT NOT NULL,
    PetName NVARCHAR(100) NOT NULL,
    Species NVARCHAR(50) NOT NULL,              -- Loài: Chó, Mèo, Hamster...
    Breed NVARCHAR(100) NULL,                   -- Giống: Golden, Poodle...
    Age INT NULL,                               -- Tuổi (năm)
    Weight DECIMAL(5,2) NULL,                   -- Cân nặng (kg)
    Gender NVARCHAR(10) NULL,                   -- Giới tính
    Color NVARCHAR(50) NULL,                    -- Màu lông
    ImageUrl NVARCHAR(255) NULL,                -- Đường dẫn hình ảnh
    SpecialNotes NVARCHAR(MAX) NULL,            -- Ghi chú đặc biệt (dị ứng, tiền sử...)
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    
    CONSTRAINT FK_Pets_Users FOREIGN KEY (OwnerId) REFERENCES Users(UserId),
    CONSTRAINT CK_Pets_Gender CHECK (Gender IN (N'Đực', N'Cái', NULL))
);
GO

CREATE TABLE Services (
    ServiceId INT PRIMARY KEY IDENTITY(1,1),
    ServiceName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    Category NVARCHAR(50) NOT NULL,             -- Danh mục: Tắm rửa, Cắt tỉa, Y tế, Spa...
    Duration INT NOT NULL,                      -- Thời lượng (phút)
    Price DECIMAL(10,2) NOT NULL,               -- Giá tiền (VND)
    ImageUrl NVARCHAR(255) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    
    CONSTRAINT CK_Services_Category CHECK (Category IN (N'Tắm rửa', N'Cắt tỉa', N'Y tế', N'Spa', N'Khác'))
);
GO

CREATE TABLE Appointments (
    AppointmentId INT PRIMARY KEY IDENTITY(1,1),
    CustomerId INT NOT NULL,
    PetId INT NOT NULL,
    StaffId INT NULL,                           -- Nhân viên phụ trách (có thể NULL khi chưa phân công)
    AppointmentDate DATE NOT NULL,
    TimeSlot NVARCHAR(20) NOT NULL,             -- VD: "08:00-08:30"
    Status NVARCHAR(20) NOT NULL DEFAULT 'Pending',
    CustomerNotes NVARCHAR(MAX) NULL,           -- Ghi chú từ khách hàng
    StaffNotes NVARCHAR(MAX) NULL,              -- Ghi chú từ nhân viên (sau khi hoàn thành)
    CancelReason NVARCHAR(MAX) NULL,            -- Lý do hủy (nếu có)
    TotalPrice DECIMAL(10,2) NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NULL,
    
    CONSTRAINT FK_Appointments_Customers FOREIGN KEY (CustomerId) REFERENCES Users(UserId),
    CONSTRAINT FK_Appointments_Pets FOREIGN KEY (PetId) REFERENCES Pets(PetId),
    CONSTRAINT FK_Appointments_Staff FOREIGN KEY (StaffId) REFERENCES Users(UserId),
    CONSTRAINT CK_Appointments_Status CHECK (Status IN ('Pending', 'Confirmed', 'InProgress', 'Completed', 'Cancelled'))
);
GO

CREATE TABLE AppointmentServices (
    AppointmentServiceId INT PRIMARY KEY IDENTITY(1,1),
    AppointmentId INT NOT NULL,
    ServiceId INT NOT NULL,
    ServicePrice DECIMAL(10,2) NOT NULL,        -- Lưu giá tại thời điểm đặt (tránh thay đổi sau)
    
    CONSTRAINT FK_AppointmentServices_Appointments FOREIGN KEY (AppointmentId) REFERENCES Appointments(AppointmentId) ON DELETE CASCADE,
    CONSTRAINT FK_AppointmentServices_Services FOREIGN KEY (ServiceId) REFERENCES Services(ServiceId)
);
GO

CREATE TABLE VaccinationRecords (
    RecordId INT PRIMARY KEY IDENTITY(1,1),
    PetId INT NOT NULL,
    AppointmentId INT NOT NULL,
    VaccineName NVARCHAR(200) NOT NULL,         -- Tên vaccine (VD: Vaccine 5 bệnh)
    VaccinationDate DATE NOT NULL,
    NextDueDate DATE NULL,                      -- Ngày hẹn tiêm nhắc lại
    Notes NVARCHAR(MAX) NULL,                   -- Ghi chú (phản ứng, tình trạng...)
    StaffId INT NOT NULL,                       -- Nhân viên thực hiện
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    
    CONSTRAINT FK_VaccinationRecords_Pets FOREIGN KEY (PetId) REFERENCES Pets(PetId),
    CONSTRAINT FK_VaccinationRecords_Appointments FOREIGN KEY (AppointmentId) REFERENCES Appointments(AppointmentId),
    CONSTRAINT FK_VaccinationRecords_Staff FOREIGN KEY (StaffId) REFERENCES Users(UserId)
);
GO

CREATE TABLE WorkSchedules (
    ScheduleId INT PRIMARY KEY IDENTITY(1,1),
    DayOfWeek NVARCHAR(20) NOT NULL UNIQUE,
    OpenTime TIME NOT NULL,
    CloseTime TIME NOT NULL,
    IsClosed BIT NOT NULL DEFAULT 0,
    
    CONSTRAINT CK_WorkSchedules_DayOfWeek CHECK (DayOfWeek IN ('Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday', 'Sunday'))
);
GO

CREATE INDEX IX_Pets_OwnerId ON Pets(OwnerId);
CREATE INDEX IX_Appointments_CustomerId ON Appointments(CustomerId);
CREATE INDEX IX_Appointments_PetId ON Appointments(PetId);
CREATE INDEX IX_Appointments_StaffId ON Appointments(StaffId);
CREATE INDEX IX_Appointments_Date ON Appointments(AppointmentDate);
CREATE INDEX IX_Appointments_Status ON Appointments(Status);
CREATE INDEX IX_AppointmentServices_AppointmentId ON AppointmentServices(AppointmentId);
CREATE INDEX IX_VaccinationRecords_PetId ON VaccinationRecords(PetId);
GO

PRINT N'=== BẮT ĐẦU NHẬP DỮ LIỆU MẪU ===';
GO

-- 1. Users - 1 Admin, 2 Staff, 3 Customers
INSERT INTO Users (FullName, Email, PasswordHash, Phone, Address, Role, IsActive)
VALUES 
-- Admin (password: admin123)
(N'Nguyễn Văn Admin', 'admin@petcare.com', '8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92', '0901234567', N'123 Nguyễn Huệ, Q1, TP.HCM', 'Admin', 1),

-- Staff (password: staff123)
(N'Trần Thị Hương', 'huong.staff@petcare.com', 'ba3253876aed6bc22d4a6ff53d8406c6ad864195ed144ab5c87621b6c233b548', '0912345678', N'456 Lê Lợi, Q1, TP.HCM', 'Staff', 1),
(N'Lê Văn Cường', 'cuong.staff@petcare.com', 'ba3253876aed6bc22d4a6ff53d8406c6ad864195ed144ab5c87621b6c233b548', '0923456789', N'789 Điện Biên Phủ, Q3, TP.HCM', 'Staff', 1),

-- Customers (password: customer123)
(N'Phạm Minh Tuấn', 'tuan.customer@gmail.com', '0b14d501a594442a01c6859541bcb3e8164d183d32937b851835442f69d5c94e', '0934567890', N'111 Cách Mạng Tháng 8, Q10, TP.HCM', 'Customer', 1),
(N'Ngô Thị Lan', 'lan.customer@gmail.com', '0b14d501a594442a01c6859541bcb3e8164d183d32937b851835442f69d5c94e', '0945678901', N'222 Hùng Vương, Tân Bình, TP.HCM', 'Customer', 1),
(N'Hoàng Văn Nam', 'nam.customer@gmail.com', '0b14d501a594442a01c6859541bcb3e8164d183d32937b851835442f69d5c94e', '0956789012', N'333 Hoàng Diệu, Q4, TP.HCM', 'Customer', 1);
GO

PRINT N'✓ Đã thêm 6 Users (1 Admin, 2 Staff, 3 Customers)';
GO

-- 2. Services - Dịch vụ
INSERT INTO Services (ServiceName, Description, Category, Duration, Price, ImageUrl, IsActive)
VALUES 
(N'Tắm vệ sinh cơ bản', N'Dịch vụ tắm rửa vệ sinh cơ bản cho thú cưng: Dầu gội chuyên dụng, sấy khô, vệ sinh tai, cắt móng', N'Tắm rửa', 60, 150000, '/Content/images/services/tam-co-ban.jpg', 1),
(N'Tắm vệ sinh cao cấp', N'Tắm rửa với sản phẩm cao cấp, massage thư giãn, vệ sinh tai, cắt móng, dưỡng lông', N'Tắm rửa', 90, 250000, '/Content/images/services/tam-cao-cap.jpg', 1),

(N'Cắt tỉa lông cơ bản', N'Cắt tỉa lông gọn gàng, vệ sinh móng', N'Cắt tỉa', 60, 200000, '/Content/images/services/cat-tia-co-ban.jpg', 1),
(N'Cắt tỉa lông tạo kiểu', N'Cắt tỉa lông theo kiểu dáng chuyên nghiệp, tạo hình theo yêu cầu', N'Cắt tỉa', 90, 350000, '/Content/images/services/cat-tia-tao-kieu.jpg', 1),

(N'Khám sức khỏe tổng quát', N'Khám sức khỏe định kỳ, kiểm tra các chỉ số cơ bản, tư vấn dinh dưỡng', N'Y tế', 45, 300000, '/Content/images/services/kham-tong-quat.jpg', 1),
(N'Tiêm phòng 5 bệnh', N'Tiêm phòng vaccine 5 bệnh (Care, Parvo, Hepatitis, Leptospirosis, Parainfluenza)', N'Y tế', 30, 200000, '/Content/images/services/tiem-5-benh.jpg', 1),
(N'Tiêm phòng dại', N'Tiêm phòng bệnh dại (Rabies) - bắt buộc cho chó mèo', N'Y tế', 20, 150000, '/Content/images/services/tiem-dai.jpg', 1),
(N'Tẩy giun, ve rận', N'Tẩy giun sán định kỳ, trị ve rận cho thú cưng', N'Y tế', 15, 100000, '/Content/images/services/tay-giun.jpg', 1),

(N'Spa thú cưng', N'Dịch vụ spa thư giãn toàn thân, massage, xông hơi, dưỡng lông chuyên sâu', N'Spa', 120, 500000, '/Content/images/services/spa.jpg', 1),
(N'Nhuộm lông an toàn', N'Nhuộm lông với thuốc nhuộm an toàn, không gây kích ứng', N'Khác', 90, 350000, '/Content/images/services/nhuom-long.jpg', 1);
GO

PRINT N'✓ Đã thêm 10 Services';
GO

-- 3. Pets - Thú cưng mẫu
INSERT INTO Pets (OwnerId, PetName, Species, Breed, Age, Weight, Gender, Color, SpecialNotes, IsActive)
VALUES 
-- Thú cưng của User 4 (Phạm Minh Tuấn)
(4, N'Lucky', N'Chó', N'Golden Retriever', 3, 28.5, N'Đực', N'Vàng', N'Hiền lành, thích chơi với trẻ em. Đã tiêm phòng đầy đủ.', 1),
(4, N'Miu Miu', N'Mèo', N'Mèo Ba Tư', 2, 4.2, N'Cái', N'Trắng', N'Hơi ngại người lạ, cần xử lý nhẹ nhàng.', 1),

-- Thú cưng của User 5 (Ngô Thị Lan)
(5, N'Buddy', N'Chó', N'Poodle', 1, 6.8, N'Đực', N'Nâu', N'Năng động, hay nhảy nhót. Cần kiểm tra móng thường xuyên.', 1),
(5, N'Luna', N'Mèo', N'Mèo Anh lông ngắn', 4, 5.5, N'Cái', N'Xám', N'Dị ứng với cá, chỉ ăn thức ăn hạt.', 1),

-- Thú cưng của User 6 (Hoàng Văn Nam)
(6, N'Max', N'Chó', N'Husky', 2, 22.0, N'Đực', N'Xám trắng', N'Rất năng động, cần tập luyện thường xuyên. Đang trong giai đoạn rụng lông.', 1),
(6, N'Bella', N'Chó', N'Corgi', 3, 11.5, N'Cái', N'Vàng nâu', N'Hiền lành, thích đùa. Đã triệt sản.', 1);
GO

PRINT N'✓ Đã thêm 6 Pets';
GO

-- 4. WorkSchedules - Giờ làm việc
INSERT INTO WorkSchedules (DayOfWeek, OpenTime, CloseTime, IsClosed)
VALUES 
('Monday', '08:00', '20:00', 0),
('Tuesday', '08:00', '20:00', 0),
('Wednesday', '08:00', '20:00', 0),
('Thursday', '08:00', '20:00', 0),
('Friday', '08:00', '20:00', 0),
('Saturday', '08:00', '20:00', 0),
('Sunday', '08:00', '17:00', 0);
GO

PRINT N'✓ Đã thêm WorkSchedules';
GO

-- 5. Appointments - Lịch hẹn mẫu (các trạng thái khác nhau)
INSERT INTO Appointments (CustomerId, PetId, StaffId, AppointmentDate, TimeSlot, Status, CustomerNotes, TotalPrice, CreatedAt)
VALUES 
-- Lịch hẹn đã hoàn thành (tháng trước)
(4, 1, 2, '2024-11-15', '09:00-10:00', 'Completed', N'Lucky đi chơi bẩn nhiều, cần tắm kỹ', 150000, '2024-11-10 14:30:00'),
(5, 3, 3, '2024-11-18', '14:00-15:30', 'Completed', N'Cắt tỉa + tắm', 550000, '2024-11-12 10:20:00'),

-- Lịch hẹn đã xác nhận (sắp tới)
(4, 2, 2, '2024-12-20', '10:00-11:30', 'Confirmed', N'Lần đầu đến, mong anh chị nhẹ nhàng với Miu', 250000, GETDATE()),
(6, 5, 3, '2024-12-22', '14:00-15:00', 'Confirmed', N'Max rụng lông nhiều, cần chải kỹ', 200000, GETDATE()),

-- Lịch hẹn chờ xử lý
(5, 4, NULL, '2024-12-23', '09:00-09:45', 'Pending', N'Luna cần khám định kỳ và tư vấn dinh dưỡng', 300000, GETDATE()),
(6, 6, NULL, '2024-12-24', '15:00-16:00', 'Pending', NULL, 200000, GETDATE()),

-- Lịch hẹn đã hủy
(4, 1, NULL, '2024-11-20', '08:00-09:00', 'Cancelled', N'Gia đình có việc đột xuất', 150000, '2024-11-15 09:00:00');
GO

PRINT N'✓ Đã thêm 7 Appointments';
GO

-- 6. AppointmentServices - Chi tiết dịch vụ trong từng lịch hẹn
INSERT INTO AppointmentServices (AppointmentId, ServiceId, ServicePrice)
VALUES 
-- Appointment 1: Tắm cơ bản (ID=1)
(1, 1, 150000),

-- Appointment 2: Cắt tỉa tạo kiểu (4) + Tắm cao cấp (2)
(2, 4, 350000),
(2, 2, 250000),

-- Appointment 3: Tắm cao cấp (2)
(3, 2, 250000),

-- Appointment 4: Cắt tỉa cơ bản (3)
(4, 3, 200000),

-- Appointment 5: Khám sức khỏe (5)
(5, 5, 300000),

-- Appointment 6: Cắt tỉa cơ bản (3)
(6, 3, 200000),

-- Appointment 7: Tắm cơ bản (1) - Đã hủy
(7, 1, 150000);
GO

PRINT N'✓ Đã thêm AppointmentServices';
GO

-- 7. VaccinationRecords - Hồ sơ tiêm phòng (cho lịch hẹn đã hoàn thành)
INSERT INTO VaccinationRecords (PetId, AppointmentId, VaccineName, VaccinationDate, NextDueDate, Notes, StaffId)
VALUES 
(1, 1, N'Vaccine 5 bệnh', '2024-11-15', '2025-11-15', N'Tiêm mũi nhắc lại hàng năm. Lucky phản ứng tốt, không có tác dụng phụ.', 2);
GO

PRINT N'✓ Đã thêm VaccinationRecords';
GO

-- Cập nhật StaffNotes cho lịch hẹn đã hoàn thành
UPDATE Appointments 
SET StaffNotes = N'Đã tắm xong, thú cưng rất ngoan. Lông sạch sẽ, không có ve rận.'
WHERE AppointmentId = 1;

UPDATE Appointments 
SET StaffNotes = N'Đã cắt tỉa và tắm xong. Khách hàng rất hài lòng với kiểu tóc mới của Buddy.'
WHERE AppointmentId = 2;
GO

-- Procedure: Lấy danh sách lịch hẹn của khách hàng
CREATE PROCEDURE sp_GetCustomerAppointments
    @CustomerId INT,
    @Status NVARCHAR(20) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        a.AppointmentId,
        a.AppointmentDate,
        a.TimeSlot,
        a.Status,
        a.TotalPrice,
        a.CustomerNotes,
        a.StaffNotes,
        p.PetName,
        p.Species,
        u.FullName AS StaffName,
        STRING_AGG(s.ServiceName, ', ') AS Services
    FROM Appointments a
    INNER JOIN Pets p ON a.PetId = p.PetId
    LEFT JOIN Users u ON a.StaffId = u.UserId
    LEFT JOIN AppointmentServices aps ON a.AppointmentId = aps.AppointmentId
    LEFT JOIN Services s ON aps.ServiceId = s.ServiceId
    WHERE a.CustomerId = @CustomerId
        AND (@Status IS NULL OR a.Status = @Status)
    GROUP BY 
        a.AppointmentId, a.AppointmentDate, a.TimeSlot, a.Status,
        a.TotalPrice, a.CustomerNotes, a.StaffNotes,
        p.PetName, p.Species, u.FullName
    ORDER BY a.AppointmentDate DESC, a.TimeSlot DESC;
END;
GO

-- Procedure: Lấy khung giờ đã đặt trong ngày
CREATE PROCEDURE sp_GetBookedTimeSlots
    @Date DATE
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT TimeSlot, COUNT(*) AS BookingCount
    FROM Appointments
    WHERE AppointmentDate = @Date
        AND Status NOT IN ('Cancelled')
    GROUP BY TimeSlot;
END;
GO

-- Procedure: Thống kê doanh thu theo khoảng thời gian
CREATE PROCEDURE sp_GetRevenueReport
    @FromDate DATE,
    @ToDate DATE
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        CAST(a.AppointmentDate AS DATE) AS ReportDate,
        COUNT(*) AS TotalAppointments,
        SUM(a.TotalPrice) AS TotalRevenue,
        AVG(a.TotalPrice) AS AvgRevenue
    FROM Appointments a
    WHERE a.Status = 'Completed'
        AND a.AppointmentDate BETWEEN @FromDate AND @ToDate
    GROUP BY CAST(a.AppointmentDate AS DATE)
    ORDER BY ReportDate;
END;
GO

-- Procedure: Thống kê dịch vụ phổ biến
CREATE PROCEDURE sp_GetTopServices
    @TopN INT = 5
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT TOP (@TopN)
        s.ServiceName,
        s.Category,
        COUNT(aps.ServiceId) AS BookingCount,
        SUM(aps.ServicePrice) AS TotalRevenue
    FROM AppointmentServices aps
    INNER JOIN Services s ON aps.ServiceId = s.ServiceId
    INNER JOIN Appointments a ON aps.AppointmentId = a.AppointmentId
    WHERE a.Status IN ('Completed', 'Confirmed')
    GROUP BY s.ServiceName, s.Category
    ORDER BY BookingCount DESC;
END;
GO

-- View: Chi tiết lịch hẹn đầy đủ
CREATE VIEW vw_AppointmentDetails AS
SELECT 
    a.AppointmentId,
    a.AppointmentDate,
    a.TimeSlot,
    a.Status,
    a.TotalPrice,
    a.CustomerNotes,
    a.StaffNotes,
    a.CancelReason,
    a.CreatedAt,
    c.UserId AS CustomerId,
    c.FullName AS CustomerName,
    c.Email AS CustomerEmail,
    c.Phone AS CustomerPhone,
    p.PetId,
    p.PetName,
    p.Species,
    p.Breed,
    p.Age,
    p.Weight,
    s.UserId AS StaffId,
    s.FullName AS StaffName,
    s.Phone AS StaffPhone
FROM Appointments a
INNER JOIN Users c ON a.CustomerId = c.UserId
INNER JOIN Pets p ON a.PetId = p.PetId
LEFT JOIN Users s ON a.StaffId = s.UserId;
GO

-- View: Thống kê thú cưng theo chủ
CREATE VIEW vw_CustomerPetsSummary AS
SELECT 
    u.UserId,
    u.FullName,
    u.Email,
    u.Phone,
    COUNT(DISTINCT p.PetId) AS TotalPets,
    COUNT(DISTINCT a.AppointmentId) AS TotalAppointments,
    SUM(CASE WHEN a.Status = 'Completed' THEN a.TotalPrice ELSE 0 END) AS TotalSpent
FROM Users u
LEFT JOIN Pets p ON u.UserId = p.OwnerId AND p.IsActive = 1
LEFT JOIN Appointments a ON u.UserId = a.CustomerId
WHERE u.Role = 'Customer'
GROUP BY u.UserId, u.FullName, u.Email, u.Phone;
GO

PRINT N'✓ Đã tạo Views';
GO

-- Trigger: Tự động cập nhật UpdatedAt khi sửa Appointments
CREATE TRIGGER trg_Appointments_UpdateTimestamp
ON Appointments
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE Appointments
    SET UpdatedAt = GETDATE()
    WHERE AppointmentId IN (SELECT AppointmentId FROM inserted);
END;
GO

-- Trigger: Tự động cập nhật UpdatedAt khi sửa Users
CREATE TRIGGER trg_Users_UpdateTimestamp
ON Users
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE Users
    SET UpdatedAt = GETDATE()
    WHERE UserId IN (SELECT UserId FROM inserted);
END;
GO

PRINT N'=== MỘT SỐ QUERY MẪU ===';
PRINT N'';
PRINT N'1. Xem danh sách khách hàng:';
PRINT N'   SELECT * FROM Users WHERE Role = ''Customer'';';
PRINT N'';
PRINT N'2. Xem danh sách dịch vụ:';
PRINT N'   SELECT * FROM Services WHERE IsActive = 1;';
PRINT N'';
PRINT N'3. Xem lịch hẹn của khách hàng (UserId = 4):';
PRINT N'   EXEC sp_GetCustomerAppointments @CustomerId = 4;';
PRINT N'';
PRINT N'4. Xem các khung giờ đã đặt ngày 2024-12-20:';
PRINT N'   EXEC sp_GetBookedTimeSlots @Date = ''2024-12-20'';';
PRINT N'';
PRINT N'5. Báo cáo doanh thu tháng 11/2024:';
PRINT N'   EXEC sp_GetRevenueReport @FromDate = ''2024-11-01'', @ToDate = ''2024-11-30'';';
PRINT N'';
PRINT N'6. Xem top 5 dịch vụ phổ biến:';
PRINT N'   EXEC sp_GetTopServices @TopN = 5;';
GO

