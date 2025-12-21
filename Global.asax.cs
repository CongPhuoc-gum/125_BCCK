using System;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Hangfire;
using Hangfire.SqlServer;
using _125_BCCK.Helpers;

namespace _125_BCCK
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // ===== CẤU HÌNH HANGFIRE TRƯỚC =====
            GlobalConfiguration.Configuration
                .UseSqlServerStorage(
                    "Data Source=ASUS\\SQLEXPRESS;Initial Catalog=PetCareDB;Integrated Security=True;TrustServerCertificate=True;",
                    new SqlServerStorageOptions
                    {
                        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                        QueuePollInterval = TimeSpan.Zero,
                        UseRecommendedIsolationLevel = true,
                        DisableGlobalLocks = true
                    });

            // ===== SAU ĐÓ MỚI DÙNG HANGFIRE =====
            // Chạy ngay 1 lần khi start
            BackgroundJob.Enqueue(() => EmailReminderJob.SendAppointmentReminders());

            // Sau đó chạy tự động mỗi 30 phút
            RecurringJob.AddOrUpdate(
                "send-appointment-reminders",
                () => EmailReminderJob.SendAppointmentReminders(),
                "*/30 * * * *" // Cron expression: mỗi 30 phút
            );

            System.Diagnostics.Debug.WriteLine("✓ Email Reminder Job đã được khởi động với Hangfire");
        }
    }
}