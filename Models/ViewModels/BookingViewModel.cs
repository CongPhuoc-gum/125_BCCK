using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace _125_BCCK.Models.ViewModels
{
    public class BookingViewModel
    {
        // Thông tin người đặt
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }

        // Thông tin dịch vụ
        [Required(ErrorMessage = "Vui lòng chọn thú cưng")]
        public int PetId { get; set; }
        
        [Required(ErrorMessage = "Vui lòng chọn ngày")]
        public DateTime AppointmentDate { get; set; }
        
        [Required(ErrorMessage = "Vui lòng chọn khung giờ")]
        public string TimeSlot { get; set; } // Ví dụ: "09:00-10:00"

        // Danh sách ID dịch vụ (vd: "1,3,5")
        public string ServiceIds { get; set; } 
        
        // Tổng tiền (dùng để hiển thị)
        public decimal TotalAmount { get; set; }
        
        // Tiền cọc (30%)
        public decimal DepositAmount { get; set; }

        public string Notes { get; set; }
    }
}