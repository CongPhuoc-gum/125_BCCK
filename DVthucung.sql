USE master;
GO

IF DB_ID('PetCareDB') IS NOT NULL
BEGIN
    ALTER DATABASE PetCareDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE PetCareDB;
    PRINT N'✓ Đã xóa database cũ thành công.';
END
ELSE
BEGIN
    PRINT N'✓ Database chưa tồn tại, sẽ tạo mới.';
END
GO

-- Tạo database mới
CREATE DATABASE PetCareDB;
GO

USE PetCareDB;
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
    Species NVARCHAR(50) NOT NULL,
    Breed NVARCHAR(100) NULL,
    Age INT NULL,
    Weight DECIMAL(5,2) NULL,
    Gender NVARCHAR(10) NULL,
    Color NVARCHAR(50) NULL,
    ImageUrl NVARCHAR(255) NULL,
    SpecialNotes NVARCHAR(MAX) NULL,
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
    Category NVARCHAR(50) NOT NULL,
    Duration INT NOT NULL,
    Price DECIMAL(10,2) NOT NULL,
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
    StaffId INT NULL,
    AppointmentDate DATE NOT NULL,
    TimeSlot NVARCHAR(20) NOT NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Pending',
    CustomerNotes NVARCHAR(MAX) NULL,
    StaffNotes NVARCHAR(MAX) NULL,
    CancelReason NVARCHAR(MAX) NULL,
    
    -- THANH TOÁN
    TotalPrice DECIMAL(10,2) NOT NULL,
    DepositAmount DECIMAL(10,2) NULL DEFAULT 0,
    DepositPaid BIT NOT NULL DEFAULT 0,
    RemainingAmount DECIMAL(10,2) NULL,
    FullyPaid BIT NOT NULL DEFAULT 0,
    PaymentMethod NVARCHAR(50) NULL, -- 'Cash', 'BankTransfer', 'Card', 'Momo', 'ZaloPay'
    
    -- EMAIL
    EmailSent BIT NOT NULL DEFAULT 0,
    EmailSentDate DATETIME NULL,
    
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
    ServicePrice DECIMAL(10,2) NOT NULL,
    
    CONSTRAINT FK_AppointmentServices_Appointments FOREIGN KEY (AppointmentId) REFERENCES Appointments(AppointmentId) ON DELETE CASCADE,
    CONSTRAINT FK_AppointmentServices_Services FOREIGN KEY (ServiceId) REFERENCES Services(ServiceId)
);
GO

CREATE TABLE PaymentTransactions (
    TransactionId INT PRIMARY KEY IDENTITY(1,1),
    AppointmentId INT NOT NULL,
    TransactionType NVARCHAR(20) NOT NULL, -- 'Deposit' hoặc 'Final'
    Amount DECIMAL(10,2) NOT NULL,
    PaymentMethod NVARCHAR(50) NULL,
    PaymentDate DATETIME NOT NULL DEFAULT GETDATE(),
    ProcessedBy INT NULL, -- StaffId xử lý thanh toán
    Notes NVARCHAR(MAX) NULL,
    
    CONSTRAINT FK_PaymentTransactions_Appointments FOREIGN KEY (AppointmentId) REFERENCES Appointments(AppointmentId),
    CONSTRAINT FK_PaymentTransactions_ProcessedBy FOREIGN KEY (ProcessedBy) REFERENCES Users(UserId),
    CONSTRAINT CK_PaymentTransactions_Type CHECK (TransactionType IN ('Deposit', 'Final'))
);
GO

CREATE TABLE EmailLogs (
    EmailLogId INT PRIMARY KEY IDENTITY(1,1),
    AppointmentId INT NOT NULL,
    RecipientEmail NVARCHAR(100) NOT NULL,
    EmailType NVARCHAR(50) NOT NULL, -- 'BookingConfirmation', 'StatusUpdate', 'PaymentReminder'
    Subject NVARCHAR(255) NOT NULL,
    Body NVARCHAR(MAX) NOT NULL,
    SentDate DATETIME NOT NULL DEFAULT GETDATE(),
    IsSuccess BIT NOT NULL DEFAULT 1,
    ErrorMessage NVARCHAR(MAX) NULL,
    
    CONSTRAINT FK_EmailLogs_Appointments FOREIGN KEY (AppointmentId) REFERENCES Appointments(AppointmentId),
    CONSTRAINT CK_EmailLogs_Type CHECK (EmailType IN ('BookingConfirmation', 'StatusUpdate', 'PaymentReminder', 'Cancelled'))
);
GO

CREATE TABLE VaccinationRecords (
    RecordId INT PRIMARY KEY IDENTITY(1,1),
    PetId INT NOT NULL,
    AppointmentId INT NOT NULL,
    VaccineName NVARCHAR(200) NOT NULL,
    VaccinationDate DATE NOT NULL,
    NextDueDate DATE NULL,
    Notes NVARCHAR(MAX) NULL,
    StaffId INT NOT NULL,
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
CREATE INDEX IX_PaymentTransactions_AppointmentId ON PaymentTransactions(AppointmentId);
CREATE INDEX IX_EmailLogs_AppointmentId ON EmailLogs(AppointmentId);
GO

PRINT N'✓ Đã tạo xong cấu trúc bảng và index';
GO

-- 1. USERS (với hash ĐÚNG từ SessionHelper.HashPassword)
INSERT INTO Users (FullName, Email, PasswordHash, Phone, Address, Role, IsActive)
VALUES 
-- Admin (password: admin123)
-- Hash: 240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9
(N'Nguyễn Văn Admin', 'admin@petcare.com', '240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9', '0901234567', N'123 Nguyễn Huệ, Q1, TP.HCM', 'Admin', 1),

-- Staff (password: staff123)  
-- Hash: 10176e7b7b24d317acfcf8d2064cfd2f24e154f7b5a96603077d5ef813d6a6b6
(N'Trần Thị Hương', 'huong.staff@petcare.com', '10176e7b7b24d317acfcf8d2064cfd2f24e154f7b5a96603077d3ef813d6a6b6', '0912345678', N'456 Lê Lợi, Q1, TP.HCM', 'Staff', 1),
(N'Lê Văn Cường', 'cuong.staff@petcare.com', '10176e7b7b24d317acfcf8d2064cfd2f24e154f7b5a96603077d3ef813d6a6b6', '0923456789', N'789 Điện Biên Phủ, Q3, TP.HCM', 'Staff', 1),

-- Customers (password: customer123)
-- Hash: b041c0aeb35bb0fa4aa668ca5a920b590196fdaf9a00eb852c9b7f4d123cc6d6
(N'Phạm Minh Tuấn', 'tuan.customer@gmail.com', '5906ac361a137e2d286465cd6588ebb5ac3f5ae955001100bc41577c3d751764', '0934567890', N'111 Cách Mạng Tháng 8, Q10, TP.HCM', 'Customer', 1),
(N'Ngô Thị Lan', 'lan.customer@gmail.com', '5906ac361a137e2d286465cd6588ebb5ac3f5ae955001100bc41577c3d751764', '0945678901', N'222 Hùng Vương, Tân Bình, TP.HCM', 'Customer', 1),
(N'Hoàng Văn Nam', 'nam.customer@gmail.com', '5906ac361a137e2d286465cd6588ebb5ac3f5ae955001100bc41577c3d751764', '0956789012', N'333 Hoàng Diệu, Q4, TP.HCM', 'Customer', 1);
GO

-- 2. SERVICES
INSERT INTO Services (ServiceName, Description, Category, Duration, Price, ImageUrl, IsActive)
VALUES 
-- TẮM RỬA
(N'Tắm vệ sinh cơ bản', N'Dịch vụ tắm rửa vệ sinh cơ bản cho thú cưng: Dầu gội chuyên dụng, sấy khô, vệ sinh tai, cắt móng', N'Tắm rửa', 60, 150000, '/Content/Images/services/tam-co-ban.jpg', 1),
(N'Tắm vệ sinh cao cấp', N'Tắm rửa với sản phẩm cao cấp, massage thư giãn, vệ sinh tai, cắt móng, dưỡng lông', N'Tắm rửa', 90, 250000, '/Content/Images/services/tam-cao-cap.jpg', 1),
(N'Tắm trị liệu da nhạy cảm', N'Tắm với dầu gội trị liệu đặc biệt cho thú cưng bị ngứa, viêm da, dị ứng', N'Tắm rửa', 75, 300000, '/Content/Images/services/tam-tri-lieu.jpg', 1),

-- CẮT TỈA
(N'Cắt tỉa lông cơ bản', N'Cắt tỉa lông gọn gàng, vệ sinh móng', N'Cắt tỉa', 60, 200000, '/Content/Images/services/cat-tia-co-ban.jpg', 1),
(N'Cắt tỉa lông tạo kiểu', N'Cắt tỉa lông theo kiểu dáng chuyên nghiệp, tạo hình theo yêu cầu', N'Cắt tỉa', 90, 350000, '/Content/Images/services/cat-tia-tao-kieu.jpg', 1),
(N'Cắt tỉa theo tiêu chuẩn show', N'Cắt tỉa theo tiêu chuẩn triển lãm quốc tế', N'Cắt tỉa', 120, 500000, '/Content/Images/services/cat-show.jpg', 1),

-- Y TẾ
(N'Khám sức khỏe tổng quát', N'Khám sức khỏe định kỳ, kiểm tra các chỉ số cơ bản, tư vấn dinh dưỡng', N'Y tế', 45, 300000, '/Content/Images/services/kham-tong-quat.jpg', 1),
(N'Tiêm phòng 5 bệnh', N'Tiêm phòng vaccine 5 bệnh (Care, Parvo, Hepatitis, Leptospirosis, Parainfluenza)', N'Y tế', 30, 200000, '/Content/Images/services/tiem-5-benh.jpg', 1),
(N'Tiêm phòng 7 bệnh', N'Tiêm phòng vaccine 7 bệnh - bảo vệ toàn diện hơn', N'Y tế', 30, 250000, '/Content/Images/services/tiem-7-benh.jpg', 1),
(N'Tiêm phòng dại', N'Tiêm phòng bệnh dại (Rabies) - bắt buộc cho chó mèo', N'Y tế', 20, 150000, '/Content/Images/services/tiem-dai.jpg', 1),
(N'Tẩy giun sán định kỳ', N'Tẩy giun sán đường ruột 3-6 tháng/lần', N'Y tế', 15, 100000, '/Content/Images/services/tay-giun.jpg', 1),
(N'Trị ve, bọ chét, rận', N'Xịt thuốc diệt ve rận bọ chét an toàn', N'Y tế', 20, 120000, '/Content/Images/services/tri-ve-ran.jpg', 1),
(N'Vệ sinh răng miệng', N'Đánh bóng răng, loại bỏ cao răng, khử mùi hôi miệng', N'Y tế', 40, 250000, '/Content/Images/services/ve-sinh-rang.jpg', 1),

-- SPA
(N'Spa thú cưng VIP', N'Dịch vụ spa thư giãn toàn thân: massage, xông hơi thảo mộc, dưỡng lông phục hồi', N'Spa', 120, 500000, '/Content/Images/services/spa-vip.jpg', 1),
(N'Massage thư giãn', N'Massage giúp thú cưng thư giãn, giảm stress, cải thiện tuần hoàn máu', N'Spa', 60, 300000, '/Content/Images/services/massage.jpg', 1),

-- KHÁC
(N'Nhuộm lông an toàn', N'Nhuộm lông với thuốc nhuộm an toàn, không gây kích ứng', N'Khác', 90, 350000, '/Content/Images/services/nhuom-long.jpg', 1),
(N'Cắt móng chuyên nghiệp', N'Cắt, dũa móng an toàn, không chảy máu', N'Khác', 20, 80000, '/Content/Images/services/cat-mong.jpg', 1);
GO

-- 3. PETS
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

-- 4. WORKSCHEDULES
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

-- 5. APPOINTMENTS (Với thanh toán)
INSERT INTO Appointments (CustomerId, PetId, StaffId, AppointmentDate, TimeSlot, Status, CustomerNotes, TotalPrice, DepositAmount, DepositPaid, RemainingAmount, FullyPaid, PaymentMethod, EmailSent, CreatedAt)
VALUES 
-- Lịch hẹn đã hoàn thành + Đã thanh toán đầy đủ
(4, 1, 2, '2024-11-15', '09:00-10:00', 'Completed', N'Lucky đi chơi bẩn nhiều, cần tắm kỹ', 150000, 45000, 1, 105000, 1, 'Cash', 1, '2024-11-10 14:30:00'),
(5, 3, 3, '2024-11-18', '14:00-15:30', 'Completed', N'Cắt tỉa + tắm', 600000, 180000, 1, 420000, 1, 'BankTransfer', 1, '2024-11-12 10:20:00'),

-- Lịch hẹn đã xác nhận + Đã đặt cọc (chưa thanh toán hết)
(4, 2, 2, '2024-12-20', '10:00-11:30', 'Confirmed', N'Lần đầu đến, mong anh chị nhẹ nhàng với Miu', 250000, 75000, 1, 175000, 0, 'Momo', 1, GETDATE()),
(6, 5, 3, '2024-12-22', '14:00-15:00', 'Confirmed', N'Max rụng lông nhiều, cần chải kỹ', 200000, 60000, 1, 140000, 0, 'ZaloPay', 1, GETDATE()),

-- Lịch hẹn chờ xử lý (KHÔNG đặt cọc)
(5, 4, NULL, '2024-12-23', '09:00-09:45', 'Pending', N'Luna cần khám định kỳ và tư vấn dinh dưỡng', 300000, 0, 0, 300000, 0, NULL, 1, GETDATE()),
(6, 6, NULL, '2024-12-24', '15:00-16:00', 'Pending', NULL, 200000, 0, 0, 200000, 0, NULL, 1, GETDATE()),

-- Lịch hẹn đã hủy
(4, 1, NULL, '2024-11-20', '08:00-09:00', 'Cancelled', N'Gia đình có việc đột xuất', 150000, 0, 0, 150000, 0, NULL, 1, '2024-11-15 09:00:00');
GO

-- 6. APPOINTMENTSERVICES
INSERT INTO AppointmentServices (AppointmentId, ServiceId, ServicePrice)
VALUES 
-- Appointment 1: Tắm cơ bản
(1, 1, 150000),

-- Appointment 2: Cắt tỉa tạo kiểu + Tắm cao cấp
(2, 5, 350000),
(2, 2, 250000),

-- Appointment 3: Tắm cao cấp
(3, 2, 250000),

-- Appointment 4: Cắt tỉa cơ bản
(4, 4, 200000),

-- Appointment 5: Khám sức khỏe
(5, 7, 300000),

-- Appointment 6: Cắt tỉa cơ bản
(6, 4, 200000),

-- Appointment 7: Tắm cơ bản
(7, 1, 150000);
GO

-- 7. PAYMENTTRANSACTIONS
INSERT INTO PaymentTransactions (AppointmentId, TransactionType, Amount, PaymentMethod, ProcessedBy, PaymentDate)
VALUES 
-- Appointment 1: Đã thanh toán đầy đủ
(1, 'Deposit', 45000, 'Cash', NULL, '2024-11-10 14:30:00'),
(1, 'Final', 105000, 'Cash', 2, '2024-11-15 10:30:00'),

-- Appointment 2: Đã thanh toán đầy đủ
(2, 'Deposit', 180000, 'BankTransfer', NULL, '2024-11-12 10:20:00'),
(2, 'Final', 420000, 'BankTransfer', 3, '2024-11-18 16:00:00'),

-- Appointment 3: Chỉ đặt cọc
(3, 'Deposit', 75000, 'Momo', NULL, GETDATE()),

-- Appointment 4: Chỉ đặt cọc
(4, 'Deposit', 60000, 'ZaloPay', NULL, GETDATE());
GO

-- 8. EMAILLOGS
INSERT INTO EmailLogs (AppointmentId, RecipientEmail, EmailType, Subject, Body, SentDate, IsSuccess)
VALUES 
(1, 'tuan.customer@gmail.com', 'BookingConfirmation', 
 N'[PetCare] Xác nhận đặt lịch #1', 
 N'Kính chào Phạm Minh Tuấn,

Lịch hẹn của bạn đã được xác nhận:
━━━━━━━━━━━━━━━━━━━━━━━━━━━
📅 Ngày: 15/11/2024
🕐 Giờ: 09:00-10:00
🐕 Thú cưng: Lucky (Golden Retriever)
💼 Dịch vụ: Tắm vệ sinh cơ bản
💰 Tổng tiền: 150,000 VNĐ
✅ Đã đặt cọc: 45,000 VNĐ (30%)
💵 Còn lại: 105,000 VNĐ

Vui lòng đến đúng giờ. Cảm ơn bạn đã tin tùng PetCare!

Hotline: 1900-xxxx', 
 '2024-11-10 14:35:00', 1),

(3, 'tuan.customer@gmail.com', 'BookingConfirmation',
 N'[PetCare] Xác nhận đặt lịch #3',
 N'Kính chào Phạm Minh Tuấn,

Lịch hẹn của bạn đã được xác nhận:
━━━━━━━━━━━━━━━━━━━━━━━━━━━
📅 Ngày: 20/12/2024
🕐 Giờ: 10:00-11:30
🐱 Thú cưng: Miu Miu (Mèo Ba Tư)
💼 Dịch vụ: Tắm vệ sinh cao cấp
💰 Tổng tiền: 250,000 VNĐ
✅ Đã đặt cọc: 75,000 VNĐ (30%)
💵 Còn lại: 175,000 VNĐ

Vui lòng đến đúng giờ. Cảm ơn bạn đã tin tùng PetCare!

Hotline: 1900-xxxx',
 GETDATE(), 1),

(5, 'lan.customer@gmail.com', 'BookingConfirmation',
 N'[PetCare] Xác nhận đặt lịch #5',
 N'Kính chào Ngô Thị Lan,

Lịch hẹn của bạn đã được xác nhận:
━━━━━━━━━━━━━━━━━━━━━━━━━━━
📅 Ngày: 23/12/2024
🕐 Giờ: 09:00-09:45
🐱 Thú cưng: Luna (Mèo Anh lông ngắn)
💼 Dịch vụ: Khám sức khỏe tổng quát
💰 Tổng tiền: 300,000 VNĐ
⚠️ Chưa đặt cọc - Vui lòng thanh toán tại quầy

Vui lòng đến đúng giờ. Cảm ơn bạn đã tin tùng PetCare!

Hotline: 1900-xxxx',
 GETDATE(), 1);
GO

PRINT N'✓ Đã thêm EmailLogs';
GO

-- 9. VACCINATIONRECORDS
INSERT INTO VaccinationRecords (PetId, AppointmentId, VaccineName, VaccinationDate, NextDueDate, Notes, StaffId)
VALUES 
(1, 1, N'Vaccine 5 bệnh', '2024-11-15', '2025-11-15', N'Tiêm mũi nhắc lại hàng năm. Lucky phản ứng tốt, không có tác dụng phụ.', 2);
GO

PRINT N'✓ Đã thêm VaccinationRecords';
GO

-- Cập nhật StaffNotes cho các lịch hẹn đã hoàn thành
UPDATE Appointments 
SET StaffNotes = N'Đã tắm xong, thú cưng rất ngoan. Lông sạch sẽ, không có ve rận. Khách đã thanh toán đầy đủ.'
WHERE AppointmentId = 1;

UPDATE Appointments 
SET StaffNotes = N'Đã cắt tỉa và tắm xong. Khách hàng rất hài lòng với kiểu tóc mới của Buddy. Đã thu đủ tiền.'
WHERE AppointmentId = 2;
GO

-- Procedure 1: Đặt lịch hẹn (Khách hàng)
CREATE PROCEDURE sp_CreateAppointment
    @CustomerId INT,
    @PetId INT,
    @AppointmentDate DATE,
    @TimeSlot NVARCHAR(20),
    @ServiceIds NVARCHAR(MAX), -- Danh sách ServiceId cách nhau bởi dấu phẩy: "1,2,3"
    @CustomerNotes NVARCHAR(MAX) = NULL,
    @IsDepositPaid BIT = 0, -- Có đặt cọc hay không
    @PaymentMethod NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        
        DECLARE @TotalPrice DECIMAL(10,2) = 0;
        DECLARE @DepositAmount DECIMAL(10,2) = 0;
        DECLARE @RemainingAmount DECIMAL(10,2) = 0;
        DECLARE @NewAppointmentId INT;
        
        -- Tính tổng tiền từ các dịch vụ
        SELECT @TotalPrice = SUM(Price)
        FROM Services
        WHERE ServiceId IN (SELECT value FROM STRING_SPLIT(@ServiceIds, ','));
        
        -- Nếu có đặt cọc thì tính 30%
        IF @IsDepositPaid = 1
        BEGIN
            SET @DepositAmount = @TotalPrice * 0.3;
            SET @RemainingAmount = @TotalPrice - @DepositAmount;
        END
        ELSE
        BEGIN
            SET @DepositAmount = 0;
            SET @RemainingAmount = @TotalPrice;
        END
        
        -- Tạo lịch hẹn
        INSERT INTO Appointments (CustomerId, PetId, AppointmentDate, TimeSlot, Status, CustomerNotes, 
                                  TotalPrice, DepositAmount, DepositPaid, RemainingAmount, FullyPaid, PaymentMethod)
        VALUES (@CustomerId, @PetId, @AppointmentDate, @TimeSlot, 'Pending', @CustomerNotes,
                @TotalPrice, @DepositAmount, @IsDepositPaid, @RemainingAmount, 0, @PaymentMethod);
        
        SET @NewAppointmentId = SCOPE_IDENTITY();
        
        -- Thêm chi tiết dịch vụ
        INSERT INTO AppointmentServices (AppointmentId, ServiceId, ServicePrice)
        SELECT @NewAppointmentId, ServiceId, Price
        FROM Services
        WHERE ServiceId IN (SELECT value FROM STRING_SPLIT(@ServiceIds, ','));
        
        -- Nếu có đặt cọc thì tạo giao dịch
        IF @IsDepositPaid = 1
        BEGIN
            INSERT INTO PaymentTransactions (AppointmentId, TransactionType, Amount, PaymentMethod)
            VALUES (@NewAppointmentId, 'Deposit', @DepositAmount, @PaymentMethod);
        END
        
        COMMIT TRANSACTION;
        
        -- Trả về thông tin lịch hẹn
        SELECT 
            @NewAppointmentId AS AppointmentId,
            @TotalPrice AS TotalPrice,
            @DepositAmount AS DepositAmount,
            @RemainingAmount AS RemainingAmount,
            @IsDepositPaid AS DepositPaid,
            'Success' AS Status;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR(@ErrorMessage, 16, 1);
    END CATCH
END;
GO

-- Procedure 2: Thanh toán phần còn lại (Staff xử lý)
CREATE PROCEDURE sp_ProcessFinalPayment
    @AppointmentId INT,
    @PaymentMethod NVARCHAR(50),
    @StaffId INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        
        DECLARE @RemainingAmount DECIMAL(10,2);
        DECLARE @CurrentStatus NVARCHAR(20);
        
        -- Lấy thông tin thanh toán
        SELECT 
            @RemainingAmount = RemainingAmount,
            @CurrentStatus = Status
        FROM Appointments 
        WHERE AppointmentId = @AppointmentId;
        
        -- Kiểm tra
        IF @RemainingAmount IS NULL
        BEGIN
            RAISERROR(N'Không tìm thấy lịch hẹn', 16, 1);
            RETURN;
        END
        
        IF @RemainingAmount <= 0
        BEGIN
            RAISERROR(N'Lịch hẹn này đã thanh toán đầy đủ', 16, 1);
            RETURN;
        END
        
        -- Cập nhật trạng thái thanh toán
        UPDATE Appointments
        SET 
            FullyPaid = 1,
            RemainingAmount = 0,
            Status = CASE WHEN Status = 'InProgress' THEN 'Completed' ELSE Status END,
            UpdatedAt = GETDATE()
        WHERE AppointmentId = @AppointmentId;
        
        -- Thêm giao dịch thanh toán
        INSERT INTO PaymentTransactions (AppointmentId, TransactionType, Amount, PaymentMethod, ProcessedBy)
        VALUES (@AppointmentId, 'Final', @RemainingAmount, @PaymentMethod, @StaffId);
        
        COMMIT TRANSACTION;
        
        SELECT 
            @AppointmentId AS AppointmentId,
            @RemainingAmount AS PaidAmount,
            'Success' AS Status;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR(@ErrorMessage, 16, 1);
    END CATCH
END;
GO

-- Procedure 3: Lấy danh sách lịch hẹn của khách hàng
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
        a.DepositAmount,
        a.DepositPaid,
        a.RemainingAmount,
        a.FullyPaid,
        a.PaymentMethod,
        a.CustomerNotes,
        a.StaffNotes,
        p.PetId,
        p.PetName,
        p.Species,
        u.UserId AS StaffId,
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
        a.TotalPrice, a.DepositAmount, a.DepositPaid, a.RemainingAmount, a.FullyPaid,
        a.PaymentMethod, a.CustomerNotes, a.StaffNotes,
        p.PetId, p.PetName, p.Species, u.UserId, u.FullName
    ORDER BY a.AppointmentDate DESC, a.TimeSlot DESC;
END;
GO

-- Procedure 4: Lấy chi tiết lịch hẹn (để gửi email)
CREATE PROCEDURE sp_GetAppointmentDetails
    @AppointmentId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Thông tin chính
    SELECT 
        a.AppointmentId,
        a.AppointmentDate,
        a.TimeSlot,
        a.Status,
        a.TotalPrice,
        a.DepositAmount,
        a.DepositPaid,
        a.RemainingAmount,
        a.FullyPaid,
        a.PaymentMethod,
        a.CustomerNotes,
        a.StaffNotes,
        c.UserId AS CustomerId,
        c.FullName AS CustomerName,
        c.Email AS CustomerEmail,
        c.Phone AS CustomerPhone,
        p.PetId,
        p.PetName,
        p.Species,
        p.Breed,
        s.UserId AS StaffId,
        s.FullName AS StaffName,
        s.Phone AS StaffPhone
    FROM Appointments a
    INNER JOIN Users c ON a.CustomerId = c.UserId
    INNER JOIN Pets p ON a.PetId = p.PetId
    LEFT JOIN Users s ON a.StaffId = s.UserId
    WHERE a.AppointmentId = @AppointmentId;
    
    -- Danh sách dịch vụ
    SELECT 
        s.ServiceId,
        s.ServiceName,
        s.Category,
        aps.ServicePrice
    FROM AppointmentServices aps
    INNER JOIN Services s ON aps.ServiceId = s.ServiceId
    WHERE aps.AppointmentId = @AppointmentId;
    
    -- Lịch sử thanh toán
    SELECT 
        pt.TransactionId,
        pt.TransactionType,
        pt.Amount,
        pt.PaymentMethod,
        pt.PaymentDate,
        u.FullName AS ProcessedByName
    FROM PaymentTransactions pt
    LEFT JOIN Users u ON pt.ProcessedBy = u.UserId
    WHERE pt.AppointmentId = @AppointmentId
    ORDER BY pt.PaymentDate;
END;
GO

-- Procedure 5: Lấy khung giờ đã đặt trong ngày
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

-- Procedure 6: Cập nhật trạng thái lịch hẹn (Staff)
CREATE PROCEDURE sp_UpdateAppointmentStatus
    @AppointmentId INT,
    @Status NVARCHAR(20),
    @StaffId INT = NULL,
    @StaffNotes NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE Appointments
    SET 
        Status = @Status,
        StaffId = COALESCE(@StaffId, StaffId),
        StaffNotes = COALESCE(@StaffNotes, StaffNotes),
        UpdatedAt = GETDATE()
    WHERE AppointmentId = @AppointmentId;
    
    SELECT 'Success' AS Status;
END;
GO

-- Procedure 7: Ghi log email
CREATE PROCEDURE sp_LogEmail
    @AppointmentId INT,
    @RecipientEmail NVARCHAR(100),
    @EmailType NVARCHAR(50),
    @Subject NVARCHAR(255),
    @Body NVARCHAR(MAX),
    @IsSuccess BIT = 1,
    @ErrorMessage NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO EmailLogs (AppointmentId, RecipientEmail, EmailType, Subject, Body, IsSuccess, ErrorMessage)
    VALUES (@AppointmentId, @RecipientEmail, @EmailType, @Subject, @Body, @IsSuccess, @ErrorMessage);
    
    -- Cập nhật trạng thái email trong Appointments
    IF @IsSuccess = 1
    BEGIN
        UPDATE Appointments
        SET EmailSent = 1, EmailSentDate = GETDATE()
        WHERE AppointmentId = @AppointmentId;
    END
    
    SELECT SCOPE_IDENTITY() AS EmailLogId;
END;
GO

-- Procedure 8: Thống kê doanh thu
CREATE PROCEDURE sp_GetRevenueReport
    @FromDate DATE,
    @ToDate DATE
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Tổng quan
    SELECT 
        COUNT(DISTINCT a.AppointmentId) AS TotalAppointments,
        SUM(a.TotalPrice) AS TotalRevenue,
        SUM(CASE WHEN a.DepositPaid = 1 THEN a.DepositAmount ELSE 0 END) AS TotalDeposits,
        SUM(CASE WHEN a.FullyPaid = 1 THEN a.TotalPrice ELSE 0 END) AS FullyPaidRevenue,
        COUNT(CASE WHEN a.FullyPaid = 0 THEN 1 END) AS UnpaidCount,
        COUNT(CASE WHEN a.FullyPaid = 1 THEN 1 END) AS PaidCount
    FROM Appointments a
    WHERE a.AppointmentDate BETWEEN @FromDate AND @ToDate
        AND a.Status != 'Cancelled';
    
    -- Theo phương thức thanh toán
    SELECT 
        pt.PaymentMethod,
        COUNT(*) AS TransactionCount,
        SUM(pt.Amount) AS TotalAmount
    FROM PaymentTransactions pt
    INNER JOIN Appointments a ON pt.AppointmentId = a.AppointmentId
    WHERE a.AppointmentDate BETWEEN @FromDate AND @ToDate
    GROUP BY pt.PaymentMethod
    ORDER BY TotalAmount DESC;
END;
GO

-- Procedure 9: Top dịch vụ phổ biến
CREATE PROCEDURE sp_GetTopServices
    @TopN INT = 5
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT TOP (@TopN)
        s.ServiceId,
        s.ServiceName,
        s.Category,
        s.Price,
        COUNT(aps.ServiceId) AS BookingCount,
        SUM(aps.ServicePrice) AS TotalRevenue
    FROM AppointmentServices aps
    INNER JOIN Services s ON aps.ServiceId = s.ServiceId
    INNER JOIN Appointments a ON aps.AppointmentId = a.AppointmentId
    WHERE a.Status IN ('Completed', 'Confirmed')
    GROUP BY s.ServiceId, s.ServiceName, s.Category, s.Price
    ORDER BY BookingCount DESC;
END;
GO

PRINT N'=== TẠO VIEWS ===';
GO

-- View 1: Chi tiết lịch hẹn đầy đủ
CREATE VIEW vw_AppointmentFullDetails AS
SELECT 
    a.AppointmentId,
    a.AppointmentDate,
    a.TimeSlot,
    a.Status,
    a.TotalPrice,
    a.DepositAmount,
    a.DepositPaid,
    a.RemainingAmount,
    a.FullyPaid,
    a.PaymentMethod,
    a.CustomerNotes,
    a.StaffNotes,
    a.EmailSent,
    a.CreatedAt,
    c.UserId AS CustomerId,
    c.FullName AS CustomerName,
    c.Email AS CustomerEmail,
    c.Phone AS CustomerPhone,
    p.PetId,
    p.PetName,
    p.Species,
    p.Breed,
    s.UserId AS StaffId,
    s.FullName AS StaffName,
    s.Phone AS StaffPhone
FROM Appointments a
INNER JOIN Users c ON a.CustomerId = c.UserId
INNER JOIN Pets p ON a.PetId = p.PetId
LEFT JOIN Users s ON a.StaffId = s.UserId;
GO

-- View 2: Thống kê khách hàng
CREATE VIEW vw_CustomerSummary AS
SELECT 
    u.UserId,
    u.FullName,
    u.Email,
    u.Phone,
    COUNT(DISTINCT p.PetId) AS TotalPets,
    COUNT(DISTINCT a.AppointmentId) AS TotalAppointments,
    SUM(CASE WHEN a.FullyPaid = 1 THEN a.TotalPrice ELSE 0 END) AS TotalSpent,
    MAX(a.AppointmentDate) AS LastVisit
FROM Users u
LEFT JOIN Pets p ON u.UserId = p.OwnerId AND p.IsActive = 1
LEFT JOIN Appointments a ON u.UserId = a.CustomerId
WHERE u.Role = 'Customer'
GROUP BY u.UserId, u.FullName, u.Email, u.Phone;
GO

UPDATE Users 
SET PasswordHash = 'b041c0aeb35bb0fa4aa668ca5a920b590196fdaf9a00eb852c9b7f4d123cc6d6'
WHERE Email IN ('huong.staff@petcare.com', 'cuong.staff@petcare.com');

UPDATE Users 
SET PasswordHash = 'b041c0aeb35bb0fa4aa668ca5a920b590196fdaf9a00eb852c9b7f4d123cc6d6'
WHERE Email IN ('tuan.customer@gmail.com');