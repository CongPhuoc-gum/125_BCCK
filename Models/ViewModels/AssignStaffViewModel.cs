using System;
using System.ComponentModel.DataAnnotations;

namespace _125_BCCK.Models.ViewModels
{
    /// <summary>
    /// ViewModel cho phân công nhân viên - BỔ SUNG THÊM
    /// </summary>
    public class AssignStaffViewModel
    {
        public int AppointmentId { get; set; }

        [Display(Name = "Ngày hẹn")]
        public DateTime AppointmentDate { get; set; }

        [Display(Name = "Giờ hẹn")]
        public string TimeSlot { get; set; }

        [Display(Name = "Khách hàng")]
        public string CustomerName { get; set; }

        [Display(Name = "Thú cưng")]
        public string PetName { get; set; }

        [Display(Name = "Loài")]
        public string PetSpecies { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn nhân viên")]
        [Display(Name = "Chọn nhân viên")]
        public int? SelectedStaffId { get; set; }

        public int? CurrentStaffId { get; set; }

        [Display(Name = "Ghi chú của nhân viên")]
        [DataType(DataType.MultilineText)]
        public string StaffNotes { get; set; }
    }
}