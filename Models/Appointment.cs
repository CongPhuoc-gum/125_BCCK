using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _125_BCCK.Models
{
    [Table("Appointments")]
    public class Appointment
    {
        [Key]
        public int AppointmentId { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [Required]
        public int PetId { get; set; }

        public int? StaffId { get; set; }

        [Required]
        [Display(Name = "Ngày hẹn")]
        [DataType(DataType.Date)]
        public DateTime AppointmentDate { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Khung giờ")]
        public string TimeSlot { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Trạng thái")]
        public string Status { get; set; }

        [Display(Name = "Ghi chú từ khách hàng")]
        public string CustomerNotes { get; set; }

        [Display(Name = "Ghi chú từ nhân viên")]
        public string StaffNotes { get; set; }

        [Display(Name = "Lý do hủy")]
        public string CancelReason { get; set; }

        [Required]
        [Display(Name = "Tổng tiền")]
        public decimal TotalPrice { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        [ForeignKey("CustomerId")]
        public virtual User Customer { get; set; }

        [ForeignKey("PetId")]
        public virtual Pet Pet { get; set; }

        [ForeignKey("StaffId")]
        public virtual User Staff { get; set; }

        public virtual ICollection<AppointmentService> AppointmentServices { get; set; }
    }
}
