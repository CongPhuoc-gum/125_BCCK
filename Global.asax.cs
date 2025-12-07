using _125_BCCK.Helpers;
using _125_BCCK.Models;
using _125_BCCK.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers; // Thư viện quan trọng cho đồng hồ
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace _125_BCCK
{
    public class MvcApplication : System.Web.HttpApplication
    {
        // 1. Khai báo đồng hồ (Timer)
        private static Timer _timer;

        protected void Application_Start()
        {
            // Các cấu hình mặc định của MVC
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // 2. CÀI ĐẶT ĐỒNG HỒ BÁO THỨC
            // 60000 ms = 1 phút. Nghĩa là cứ 1 phút hệ thống sẽ tỉnh dậy làm việc 1 lần.
            // (Khi test xong có thể tăng lên 600000 = 10 phút cho đỡ nặng máy)
            _timer = new Timer(60000);

            // Chỉ định công việc phải làm: Chạy hàm OnTimedEvent
            _timer.Elapsed += OnTimedEvent;

            // Bắt đầu chạy đồng hồ
            _timer.AutoReset = true;
            _timer.Enabled = true;
        }

        // 3. Hàm sự kiện: Được gọi mỗi khi đồng hồ điểm (mỗi 1 phút)
        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            try
            {
                // Gọi hàm kiểm tra và gửi mail
                CheckAndSendReminders();
            }
            catch (Exception)
            {
                // Nếu có lỗi xảy ra trong lúc chạy ngầm, ta bỏ qua để không làm sập web
                // (Trong thực tế nên ghi log lỗi vào file text)
            }
        }

        // 4. Logic chính: Quét Database và Gửi Mail
        private void CheckAndSendReminders()
        {
            // Mở kết nối Database
            using (var db = new PetCareContext())
            {
                DateTime now = DateTime.Now;

                // A. Lấy danh sách các lịch hẹn THỎA MÃN 3 ĐIỀU KIỆN:
                // 1. Lịch hẹn là hôm nay (AppointmentDate = Hôm nay)
                // 2. Trạng thái đã xác nhận (Status = 'Confirmed') hoặc 'Pending' tùy bạn chọn
                // 3. Chưa gửi mail (EmailSent = 0 hoặc false)
                var todayAppointments = db.Database.SqlQuery<AppointmentReminderViewModel>(
                    @"SELECT 
                        a.AppointmentId, 
                        a.TimeSlot, 
                        u.Email, 
                        u.FullName, 
                        p.PetName 
                      FROM Appointments a
                      JOIN Users u ON a.CustomerId = u.UserId
                      JOIN Pets p ON a.PetId = p.PetId
                      WHERE CAST(a.AppointmentDate AS DATE) = CAST(GETDATE() AS DATE) 
                      AND a.Status IN ('Confirmed', 'Pending')
                      AND a.EmailSent = 0"
                ).ToList();

                // B. Duyệt qua từng lịch hẹn để kiểm tra giờ
                foreach (var item in todayAppointments)
                {
                    try
                    {
                        // item.TimeSlot có dạng "09:00-10:00". Ta lấy phần đầu "09:00"
                        string startTimeStr = item.TimeSlot.Split('-')[0];
                        TimeSpan startTime = TimeSpan.Parse(startTimeStr);

                        // Tạo ra thời gian hẹn cụ thể (Ngày hôm nay + Giờ hẹn)
                        DateTime appointmentTime = DateTime.Today.Add(startTime);

                        // Tính khoảng cách: Giờ hẹn - Giờ hiện tại (ra số phút)
                        double minutesUntilAppointment = (appointmentTime - now).TotalMinutes;

                        // C. KIỂM TRA: Nếu còn dưới 120 phút (2 tiếng) và chưa quá giờ hẹn
                        // (Bạn có thể sửa số 120 thành 60 nếu muốn nhắc trước 1 tiếng)
                        if (minutesUntilAppointment > 0 && minutesUntilAppointment <= 120)
                        {
                            // --- Soạn nội dung Email ---
                            string subject = $"[PetCare] Nhắc nhở: Lịch hẹn sắp tới cho bé {item.PetName}";
                            string body = $@"
                                <h3>Xin chào {item.FullName},</h3>
                                <p>Hệ thống PetCare xin nhắc bạn có lịch hẹn chăm sóc thú cưng sắp diễn ra:</p>
                                <ul>
                                    <li><b>Thú cưng:</b> {item.PetName}</li>
                                    <li><b>Thời gian:</b> {item.TimeSlot} hôm nay</li>
                                </ul>
                                <p>Vui lòng đến đúng giờ để được phục vụ tốt nhất.</p>
                                <p>Trân trọng,<br/>Đội ngũ PetCare.</p>";

                            // --- Gửi Email bằng Helper ---
                            bool success = EmailHelper.SendMail(item.Email, subject, body);

                            // --- Nếu gửi thành công -> Đánh dấu vào Database ---
                            if (success)
                            {
                                // Cập nhật EmailSent = 1 để không gửi lại lần nữa
                                db.Database.ExecuteSqlCommand(
                                    "UPDATE Appointments SET EmailSent = 1, EmailSentDate = GETDATE() WHERE AppointmentId = @p0",
                                    item.AppointmentId
                                );
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // Nếu lỗi ở một lịch hẹn cụ thể, bỏ qua và chạy tiếp lịch hẹn sau
                        continue;
                    }
                }
            }
        }
    }
}