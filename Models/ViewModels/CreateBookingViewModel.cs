using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace _125_BCCK.Models.ViewModels
{
    public class CreateBookingViewModel
    {
        [Required(ErrorMessage = "Vui lòng chọn thú cưng")]
        public int PetId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ít nhất 1 dịch vụ")]
        public string ServiceIds { get; set; } // "1,2,3"

        [Required(ErrorMessage = "Vui lòng chọn ngày")]
        [DataType(DataType.Date)]
        public DateTime AppointmentDate { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn giờ")]
        public string TimeSlot { get; set; }

        public string CustomerNotes { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán")]
        public string PaymentOption { get; set; } // "deposit" hoặc "none"

        public string PaymentMethod { get; set; } // "Cash", "BankTransfer", "Momo", "ZaloPay"
    }
}