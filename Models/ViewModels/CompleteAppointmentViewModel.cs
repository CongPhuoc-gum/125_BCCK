using System.ComponentModel.DataAnnotations;
using System.Web;

namespace _125_BCCK.Models.ViewModels
{
    public class CompleteAppointmentViewModel
    {
        public int AppointmentId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tình trạng thú cưng")]
        [Display(Name = "Tình trạng thú cưng")]
        public string PetCondition { get; set; }

        [Display(Name = "Ghi chú")]
        public string Notes { get; set; }

        [Display(Name = "Hình ảnh (nếu có)")]
        public HttpPostedFileBase ImageFile { get; set; }

        // Để lưu đường dẫn ảnh sau khi upload
        public string ImageUrl { get; set; }

        // THÊM MỚI - Thông tin thanh toán
        [Display(Name = "Số tiền còn lại")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal RemainingAmount { get; set; }

        [Display(Name = "Cần thanh toán?")]
        public bool NeedPayment { get; set; }

        [Display(Name = "Khách thanh toán ngay?")]
        public bool PayNow { get; set; }

        [Display(Name = "Phương thức thanh toán")]
        public string PaymentMethod { get; set; }
    }
}