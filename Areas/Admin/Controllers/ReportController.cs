using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using _125_BCCK.Models;
using _125_BCCK.Models.ViewModels;

namespace _125_BCCK.Areas.Admin.Controllers
{
    public class ReportController : Controller
    {
        private PetCareContext db = new PetCareContext();

        // GET: Admin/Report
        public ActionResult Index()
        {
            var model = new StatisticalViewModel();

            // --- 1. LẤY SỐ LIỆU TỔNG QUAN (CARDS) ---
            // Gọi sp_GetDashboardStatistics
            // Lưu ý: Vì Procedure này trả về 1 dòng nhiều cột, ta dùng SqlQuery để map
            var stats = db.Database.SqlQuery<DashboardStatDTO>("exec sp_GetDashboardStatistics").FirstOrDefault();

            // Tính tổng doanh thu toàn bộ (hoặc em có thể dùng sp_GetRevenueReport)
            var revenue = db.Appointments
                            .Where(a => a.Status == "Completed" || a.Status == "Confirmed")
                            .Sum(a => (decimal?)a.TotalPrice) ?? 0;

            if (stats != null)
            {
                model.PendingCount = stats.PendingCount;
                model.CompletedCount = stats.CompletedCount;
                model.TotalCustomers = stats.TotalCustomers;
                model.TotalRevenue = revenue;
            }

            // --- 2. LẤY DỮ LIỆU BIỂU ĐỒ THÁNG (Năm nay) ---
            int currentYear = DateTime.Now.Year;
            var monthlyData = db.Database.SqlQuery<MonthlyStatDTO>(
                $"exec sp_GetMonthlyRevenueStats @Year={currentYear}"
            ).ToList();

            model.MonthlyLabels = monthlyData.Select(x => x.Month).ToList();
            model.MonthlyRevenue = monthlyData.Select(x => x.Revenue).ToList();

            // --- 3. LẤY DỮ LIỆU BIỂU ĐỒ NĂM ---
            var yearlyData = db.Database.SqlQuery<YearlyStatDTO>("exec sp_GetYearlyRevenueStats").ToList();

            model.YearlyLabels = yearlyData.Select(x => x.Year).ToList();
            model.YearlyRevenue = yearlyData.Select(x => x.TotalRevenue).ToList();

            // --- 4. TOP DỊCH VỤ ---
            var topServices = db.Database.SqlQuery<ServiceStatDTO>("exec sp_GetTopServicesStats").ToList();
            model.ServiceLabels = topServices.Select(x => x.ServiceName).ToList();
            model.ServiceRevenue = topServices.Select(x => x.RevenueGenerated).ToList();

            // --- 5. TOP KHÁCH HÀNG (Từ View vw_CustomerSummary) ---
            // Lấy 5 khách chi tiêu cao nhất
            var topCustomers = db.Database.SqlQuery<CustomerStatDTO>(
                "SELECT TOP 5 FullName, TotalAppointments as BookingCount, TotalSpent FROM vw_CustomerSummary ORDER BY TotalSpent DESC"
            ).ToList();
            model.TopCustomers = topCustomers;

            return View(model);
        }

        // Các class DTO nhỏ để hứng dữ liệu từ SQL (Mapping)
        private class DashboardStatDTO
        {
            public int PendingCount { get; set; }
            public int CompletedCount { get; set; }
            public int TotalCustomers { get; set; }
        }
        private class MonthlyStatDTO { public int Month { get; set; } public decimal Revenue { get; set; } }
        private class YearlyStatDTO { public int Year { get; set; } public decimal TotalRevenue { get; set; } }
        private class ServiceStatDTO { public string ServiceName { get; set; } public decimal RevenueGenerated { get; set; } }
    }
}