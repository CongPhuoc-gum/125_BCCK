using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace _125_BCCK.Models.ViewModels
{
    // ViewModel cho danh sách lịch hẹn (Index page)
    public class AppointmentViewModel
    {
        public int AppointmentId { get; set; }

        [Display(Name = "Ngày hẹn")]
        public DateTime AppointmentDate { get; set; }

        [Display(Name = "Giờ hẹn")]
        public string TimeSlot { get; set; }

        [Display(Name = "Trạng thái")]
        public string Status { get; set; }

        [Display(Name = "Tổng tiền")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal TotalPrice { get; set; }

        [Display(Name = "Tiền cọc")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal DepositAmount { get; set; }

        [Display(Name = "Đã đặt cọc")]
        public bool DepositPaid { get; set; }

        [Display(Name = "Còn lại")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal RemainingAmount { get; set; }

        [Display(Name = "Đã thanh toán đủ")]
        public bool FullyPaid { get; set; }

        [Display(Name = "Phương thức thanh toán")]
        public string PaymentMethod { get; set; }

        [Display(Name = "Ghi chú của tôi")]
        public string CustomerNotes { get; set; }

        [Display(Name = "Ghi chú nhân viên")]
        public string StaffNotes { get; set; }

        // Thông tin thú cưng
        public int PetId { get; set; }

        [Display(Name = "Tên thú cưng")]
        public string PetName { get; set; }

        [Display(Name = "Loại")]
        public string Species { get; set; }

        // Thông tin nhân viên
        public int? StaffId { get; set; }

        [Display(Name = "Nhân viên phụ trách")]
        public string StaffName { get; set; }

        // Danh sách dịch vụ (dạng chuỗi từ stored procedure)
        [Display(Name = "Dịch vụ")]
        public string Services { get; set; }
    }

    // ViewModel cho chi tiết lịch hẹn (Details page)
    public class AppointmentDetailViewModel
    {
        public int AppointmentId { get; set; }

        [Display(Name = "Ngày hẹn")]
        public DateTime AppointmentDate { get; set; }

        [Display(Name = "Giờ hẹn")]
        public string TimeSlot { get; set; }

        [Display(Name = "Trạng thái")]
        public string Status { get; set; }

        [Display(Name = "Tổng tiền")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal TotalPrice { get; set; }

        [Display(Name = "Tiền cọc")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal DepositAmount { get; set; }

        [Display(Name = "Đã đặt cọc")]
        public bool DepositPaid { get; set; }

        [Display(Name = "Còn lại")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal RemainingAmount { get; set; }

        [Display(Name = "Đã thanh toán đủ")]
        public bool FullyPaid { get; set; }

        [Display(Name = "Phương thức thanh toán")]
        public string PaymentMethod { get; set; }

        [Display(Name = "Ghi chú của tôi")]
        public string CustomerNotes { get; set; }

        [Display(Name = "Ghi chú nhân viên")]
        public string StaffNotes { get; set; }

        [Display(Name = "Lý do hủy")]
        public string CancelReason { get; set; }

        // Thông tin khách hàng
        public int CustomerId { get; set; }

        [Display(Name = "Tên khách hàng")]
        public string CustomerName { get; set; }

        [Display(Name = "Email")]
        public string CustomerEmail { get; set; }

        [Display(Name = "Số điện thoại")]
        public string CustomerPhone { get; set; }

        // Thông tin thú cưng
        public int PetId { get; set; }

        [Display(Name = "Tên thú cưng")]
        public string PetName { get; set; }

        [Display(Name = "Loài")]
        public string Species { get; set; }

        [Display(Name = "Giống")]
        public string Breed { get; set; }

        // Thông tin nhân viên
        public int? StaffId { get; set; }

        [Display(Name = "Nhân viên phụ trách")]
        public string StaffName { get; set; }

        [Display(Name = "SĐT nhân viên")]
        public string StaffPhone { get; set; }

        // Danh sách dịch vụ chi tiết
        public List<AppointmentServiceDetailViewModel> Services { get; set; }

        // Lịch sử thanh toán
        public List<PaymentTransactionViewModel> PaymentHistory { get; set; }

        public AppointmentDetailViewModel()
        {
            Services = new List<AppointmentServiceDetailViewModel>();
            PaymentHistory = new List<PaymentTransactionViewModel>();
        }
    }

    // ViewModel cho dịch vụ trong lịch hẹn (tránh trùng tên với ServiceItemViewModel có sẵn)
    public class AppointmentServiceDetailViewModel
    {
        public int ServiceId { get; set; }

        [Display(Name = "Tên dịch vụ")]
        public string ServiceName { get; set; }

        [Display(Name = "Loại dịch vụ")]
        public string Category { get; set; }

        [Display(Name = "Giá")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal Price { get; set; }
    }

    // ViewModel cho lịch sử thanh toán
    public class PaymentTransactionViewModel
    {
        public int TransactionId { get; set; }

        [Display(Name = "Loại giao dịch")]
        public string TransactionType { get; set; }

        [Display(Name = "Số tiền")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal Amount { get; set; }

        [Display(Name = "Phương thức")]
        public string PaymentMethod { get; set; }

        [Display(Name = "Thời gian")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime PaymentDate { get; set; }

        [Display(Name = "Người xử lý")]
        public string ProcessedByName { get; set; }
    }
}