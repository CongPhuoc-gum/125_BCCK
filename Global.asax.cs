using System;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Threading; 
using _125_BCCK.Helpers; 

namespace _125_BCCK
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private static Timer _emailTimer;

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // ===== THÊM ĐOẠN NÀY: Khởi động job gửi email =====
            StartEmailReminderJob();
        }

        private void StartEmailReminderJob()
        {
            // Chạy ngay 1 lần khi start
            EmailReminderJob.SendAppointmentReminders();

            // Sau đó chạy mỗi 30 phút
            _emailTimer = new Timer(
                callback: (state) => EmailReminderJob.SendAppointmentReminders(),
                state: null,
                dueTime: TimeSpan.FromMinutes(30), // Lần đầu sau 30 phút
                period: TimeSpan.FromMinutes(30)   // Lặp lại mỗi 30 phút
            );

            System.Diagnostics.Debug.WriteLine("✓ Email Reminder Job đã được khởi động");
        }

        protected void Application_End()
        {
            _emailTimer?.Dispose();
        }
    }
}