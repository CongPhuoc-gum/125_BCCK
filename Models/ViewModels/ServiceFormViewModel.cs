using System.ComponentModel.DataAnnotations;

namespace _125_BCCK.Models.ViewModels
{
    /// <summary>
    /// ViewModel cho form tạo/sửa dịch vụ
    /// </summary>
    public class ServiceFormViewModel
    {
        public int ServiceId { get; set; }

        [Required(ErrorMessage = "Tên dịch vụ không được để trống")]
        [StringLength(100, ErrorMessage = "Tên dịch vụ không quá 100 ký tự")]
        [Display(Name = "Tên dịch vụ")]
        public string ServiceName { get; set; }

        [Display(Name = "Mô tả")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn danh mục")]
        [Display(Name = "Danh mục")]
        public string Category { get; set; }

        [Required(ErrorMessage = "Thời gian không được để trống")]
        [Range(5, 300, ErrorMessage = "Thời gian từ 5-300 phút")]
        [Display(Name = "Thời gian (phút)")]
        public int Duration { get; set; }

        [Required(ErrorMessage = "Giá không được để trống")]
        [Range(10000, 10000000, ErrorMessage = "Giá từ 10,000 - 10,000,000 VNĐ")]
        [Display(Name = "Giá (VNĐ)")]
        public decimal Price { get; set; }

        [StringLength(255)]
        [Display(Name = "URL hình ảnh")]
        public string ImageUrl { get; set; }

        [Display(Name = "Đang hoạt động")]
        public bool IsActive { get; set; } = true;
    }
}