using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web;

namespace _125_BCCK.Models
{
    [Table("Pets")]
    public class Pet
    {
        [Key]
        public int PetId { get; set; }

        public int OwnerId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên thú cưng")]
        [StringLength(100)]
        [Display(Name = "Tên thú cưng")]
        public string PetName { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn loài")]
        [StringLength(50)]
        [Display(Name = "Loài")]
        public string Species { get; set; }

        [StringLength(100)]
        [Display(Name = "Giống")]
        public string Breed { get; set; }

        [Display(Name = "Tuổi")]
        public int? Age { get; set; }

        [Display(Name = "Cân nặng (kg)")]
        public decimal? Weight { get; set; }

        [StringLength(10)]
        [Display(Name = "Giới tính")]
        public string Gender { get; set; }

        [StringLength(50)]
        [Display(Name = "Màu lông")]
        public string Color { get; set; }

        [StringLength(255)]
        public string ImageUrl { get; set; }

        public string SpecialNotes { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        // Property để upload file (không map vào DB)
        [NotMapped]
        [Display(Name = "Chọn ảnh")]
        public HttpPostedFileBase ImageFile { get; set; }
    }
}