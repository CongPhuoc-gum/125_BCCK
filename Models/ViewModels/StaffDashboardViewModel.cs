using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace _125_BCCK.Models.ViewModels
{
    public class StaffDashboardViewModel
    {
        [Display(Name = "Số lịch hôm nay")]
        public int TodayAppointmentsCount { get; set; }

        [Display(Name = "Số lịch tuần này")]
        public int WeekAppointmentsCount { get; set; }

        [Display(Name = "Danh sách lịch hôm nay")]
        public List<AppointmentListItemViewModel> TodayAppointments { get; set; }

        // ===== MỚI: Thêm lịch đã hoàn thành =====
        [Display(Name = "Tổng số lịch đã hoàn thành")]
        public int TotalCompletedCount { get; set; }

        [Display(Name = "Danh sách lịch đã hoàn thành gần đây")]
        public List<AppointmentListItemViewModel> CompletedAppointments { get; set; }

        [Display(Name = "Số lịch chưa thanh toán đủ")]
        public int UnpaidCompletedCount { get; set; }

        public StaffDashboardViewModel()
        {
            TodayAppointments = new List<AppointmentListItemViewModel>();
            CompletedAppointments = new List<AppointmentListItemViewModel>();
        }
    }
}