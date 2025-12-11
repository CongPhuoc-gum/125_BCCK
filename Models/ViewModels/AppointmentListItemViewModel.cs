using System;
using System.ComponentModel.DataAnnotations;

namespace _125_BCCK.Models.ViewModels
{
    // ViewModel cho item trong danh sách lịch hẹn (dùng trong Dashboard và Appointments list)
    public class AppointmentListItemViewModel
    {
        public int AppointmentId { get; set; }

        [Display(Name = "Ngày hẹn")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime AppointmentDate { get; set; }

        [Display(Name = "Giờ hẹn")]
        public string TimeSlot { get; set; }

        [Display(Name = "Trạng thái")]
        public string Status { get; set; }

        [Display(Name = "Khách hàng")]
        public string CustomerName { get; set; }

        [Display(Name = "Thú cưng")]
        public string PetName { get; set; }

        [Display(Name = "Loài")]
        public string Species { get; set; }

        [Display(Name = "Dịch vụ")]
        public string Services { get; set; }

        [Display(Name = "Tổng tiền")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal TotalPrice { get; set; }

        [Display(Name = "Ghi chú")]
        public string CustomerNotes { get; set; }

        // Thêm thông tin thanh toán (optional - dùng khi cần)
        public decimal? DepositAmount { get; set; }
        public bool DepositPaid { get; set; }
        public decimal? RemainingAmount { get; set; }
        public bool FullyPaid { get; set; }
    }
}