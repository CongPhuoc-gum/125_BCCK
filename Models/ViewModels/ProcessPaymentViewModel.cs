using System.ComponentModel.DataAnnotations;

namespace _125_BCCK.Models.ViewModels
{
    // ViewModel cho xử lý thanh toán phần còn lại
    public class ProcessPaymentViewModel
    {
        public int AppointmentId { get; set; }

        [Display(Name = "Khách hàng")]
        public string CustomerName { get; set; }

        [Display(Name = "Thú cưng")]
        public string PetName { get; set; }

        [Display(Name = "Tổng tiền")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal TotalPrice { get; set; }

        [Display(Name = "Đã đặt cọc")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal DepositAmount { get; set; }

        [Display(Name = "Số tiền cần thu")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal RemainingAmount { get; set; }

        [Display(Name = "Đã đặt cọc trước")]
        public bool AlreadyPaid { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán")]
        [Display(Name = "Phương thức thanh toán")]
        public string PaymentMethod { get; set; }

        [Display(Name = "Ghi chú")]
        public string Notes { get; set; }
    }
}