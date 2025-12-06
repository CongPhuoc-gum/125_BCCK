using _125_BCCK.Helpers;
using _125_BCCK.Models;
using _125_BCCK.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;

namespace _125_BCCK.Controllers
{
    public class AppointmentsController : Controller
    {
        private PetCareContext db = new PetCareContext();

        // GET: Appointments - Danh sách lịch hẹn của khách hàng
        public ActionResult Index(string status = "")
        {
            // Kiểm tra đăng nhập
            if (!SessionHelper.IsLoggedIn())
            {
                TempData["Error"] = "Vui lòng đăng nhập để xem lịch hẹn";
                return RedirectToAction("Login", "Account");
            }

            // Chỉ cho phép khách hàng truy cập
            if (SessionHelper.GetRole() != "Customer")
            {
                TempData["Error"] = "Bạn không có quyền truy cập trang này";
                return RedirectToAction("Index", "Home");
            }

            int customerId = (int)SessionHelper.GetUserId();
            List<AppointmentViewModel> appointments = new List<AppointmentViewModel>();

            try
            {
                // Gọi stored procedure
                var customerIdParam = new SqlParameter("@CustomerId", customerId);
                var statusParam = new SqlParameter("@Status",
                    string.IsNullOrEmpty(status) ? (object)DBNull.Value : status);

                var result = db.Database.SqlQuery<AppointmentQueryResult>(
                    "EXEC sp_GetCustomerAppointments @CustomerId, @Status",
                    customerIdParam, statusParam
                ).ToList();

                // Map sang ViewModel
                appointments = result.Select(r => new AppointmentViewModel
                {
                    AppointmentId = r.AppointmentId,
                    AppointmentDate = r.AppointmentDate,
                    TimeSlot = r.TimeSlot,
                    Status = r.Status,
                    TotalPrice = r.TotalPrice,
                    DepositAmount = r.DepositAmount ?? 0,
                    DepositPaid = r.DepositPaid,
                    RemainingAmount = r.RemainingAmount ?? 0,
                    FullyPaid = r.FullyPaid,
                    PaymentMethod = r.PaymentMethod ?? "",
                    CustomerNotes = r.CustomerNotes ?? "",
                    StaffNotes = r.StaffNotes ?? "",
                    PetId = r.PetId,
                    PetName = r.PetName,
                    Species = r.Species,
                    StaffId = r.StaffId,
                    StaffName = r.StaffName ?? "Chưa phân công",
                    Services = r.Services ?? ""
                }).ToList();

                ViewBag.CurrentStatus = status;
                return View(appointments);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi khi tải danh sách lịch hẹn: " + ex.Message;
                return View(new List<AppointmentViewModel>());
            }
        }

        // GET: Appointments/Details/5
        public ActionResult Details(int id)
        {
            if (!SessionHelper.IsLoggedIn())
            {
                TempData["Error"] = "Vui lòng đăng nhập";
                return RedirectToAction("Login", "Account");
            }

            int userId = (int)SessionHelper.GetUserId();
            AppointmentDetailViewModel model = null;

            try
            {
                var appointmentIdParam = new SqlParameter("@AppointmentId", id);

                using (var command = db.Database.Connection.CreateCommand())
                {
                    command.CommandText = "sp_GetAppointmentDetails";
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(appointmentIdParam);

                    db.Database.Connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        // Result set 1: Thông tin chính
                        if (reader.Read())
                        {
                            int customerId = reader.GetInt32(reader.GetOrdinal("CustomerId"));

                            // Kiểm tra quyền
                            if (SessionHelper.GetRole() == "Customer" && customerId != userId)
                            {
                                TempData["Error"] = "Bạn không có quyền xem lịch hẹn này";
                                return RedirectToAction("Index");
                            }

                            model = new AppointmentDetailViewModel
                            {
                                AppointmentId = reader.GetInt32(reader.GetOrdinal("AppointmentId")),
                                AppointmentDate = reader.GetDateTime(reader.GetOrdinal("AppointmentDate")),
                                TimeSlot = reader.GetString(reader.GetOrdinal("TimeSlot")),
                                Status = reader.GetString(reader.GetOrdinal("Status")),
                                TotalPrice = reader.GetDecimal(reader.GetOrdinal("TotalPrice")),
                                DepositAmount = reader.IsDBNull(reader.GetOrdinal("DepositAmount")) ? 0 : reader.GetDecimal(reader.GetOrdinal("DepositAmount")),
                                DepositPaid = reader.GetBoolean(reader.GetOrdinal("DepositPaid")),
                                RemainingAmount = reader.IsDBNull(reader.GetOrdinal("RemainingAmount")) ? 0 : reader.GetDecimal(reader.GetOrdinal("RemainingAmount")),
                                FullyPaid = reader.GetBoolean(reader.GetOrdinal("FullyPaid")),
                                PaymentMethod = reader.IsDBNull(reader.GetOrdinal("PaymentMethod")) ? "" : reader.GetString(reader.GetOrdinal("PaymentMethod")),
                                CustomerNotes = reader.IsDBNull(reader.GetOrdinal("CustomerNotes")) ? "" : reader.GetString(reader.GetOrdinal("CustomerNotes")),
                                StaffNotes = reader.IsDBNull(reader.GetOrdinal("StaffNotes")) ? "" : reader.GetString(reader.GetOrdinal("StaffNotes")),
                                CustomerId = customerId,
                                CustomerName = reader.GetString(reader.GetOrdinal("CustomerName")),
                                CustomerEmail = reader.GetString(reader.GetOrdinal("CustomerEmail")),
                                CustomerPhone = reader.IsDBNull(reader.GetOrdinal("CustomerPhone")) ? "" : reader.GetString(reader.GetOrdinal("CustomerPhone")),
                                PetId = reader.GetInt32(reader.GetOrdinal("PetId")),
                                PetName = reader.GetString(reader.GetOrdinal("PetName")),
                                Species = reader.GetString(reader.GetOrdinal("Species")),
                                Breed = reader.IsDBNull(reader.GetOrdinal("Breed")) ? "" : reader.GetString(reader.GetOrdinal("Breed")),
                                StaffId = reader.IsDBNull(reader.GetOrdinal("StaffId")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("StaffId")),
                                StaffName = reader.IsDBNull(reader.GetOrdinal("StaffName")) ? "Chưa phân công" : reader.GetString(reader.GetOrdinal("StaffName")),
                                StaffPhone = reader.IsDBNull(reader.GetOrdinal("StaffPhone")) ? "" : reader.GetString(reader.GetOrdinal("StaffPhone")),
                                Services = new List<AppointmentServiceDetailViewModel>(),
                                PaymentHistory = new List<PaymentTransactionViewModel>()
                            };
                        }
                        else
                        {
                            TempData["Error"] = "Không tìm thấy lịch hẹn";
                            return RedirectToAction("Index");
                        }

                        // Result set 2: Danh sách dịch vụ
                        if (reader.NextResult())
                        {
                            while (reader.Read())
                            {
                                model.Services.Add(new AppointmentServiceDetailViewModel
                                {
                                    ServiceId = reader.GetInt32(reader.GetOrdinal("ServiceId")),
                                    ServiceName = reader.GetString(reader.GetOrdinal("ServiceName")),
                                    Category = reader.GetString(reader.GetOrdinal("Category")),
                                    Price = reader.GetDecimal(reader.GetOrdinal("ServicePrice"))
                                });
                            }
                        }

                        // Result set 3: Lịch sử thanh toán
                        if (reader.NextResult())
                        {
                            while (reader.Read())
                            {
                                model.PaymentHistory.Add(new PaymentTransactionViewModel
                                {
                                    TransactionId = reader.GetInt32(reader.GetOrdinal("TransactionId")),
                                    TransactionType = reader.GetString(reader.GetOrdinal("TransactionType")),
                                    Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
                                    PaymentMethod = reader.IsDBNull(reader.GetOrdinal("PaymentMethod")) ? "" : reader.GetString(reader.GetOrdinal("PaymentMethod")),
                                    PaymentDate = reader.GetDateTime(reader.GetOrdinal("PaymentDate")),
                                    ProcessedByName = reader.IsDBNull(reader.GetOrdinal("ProcessedByName")) ? "" : reader.GetString(reader.GetOrdinal("ProcessedByName"))
                                });
                            }
                        }
                    }
                }

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi khi tải chi tiết lịch hẹn: " + ex.Message;
                return RedirectToAction("Index");
            }
            finally
            {
                if (db.Database.Connection.State == ConnectionState.Open)
                {
                    db.Database.Connection.Close();
                }
            }
        }

        // POST: Appointments/Cancel/5 - Hủy lịch hẹn
        [HttpPost]
        public ActionResult Cancel(int id, string cancelReason)
        {
            if (!SessionHelper.IsLoggedIn())
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập" });
            }

            try
            {
                var appointment = db.Appointments.Find(id);

                if (appointment == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy lịch hẹn" });
                }

                // Kiểm tra quyền
                if (SessionHelper.GetRole() == "Customer" && appointment.CustomerId != SessionHelper.GetUserId())
                {
                    return Json(new { success = false, message = "Bạn không có quyền hủy lịch hẹn này" });
                }

                // Kiểm tra trạng thái
                if (appointment.Status == "Completed" || appointment.Status == "Cancelled")
                {
                    return Json(new { success = false, message = "Không thể hủy lịch hẹn này" });
                }

                // Cập nhật trạng thái
                appointment.Status = "Cancelled";
                appointment.CancelReason = cancelReason;
                appointment.UpdatedAt = DateTime.Now;

                db.SaveChanges();

                return Json(new { success = true, message = "Đã hủy lịch hẹn thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    // Helper class để map kết quả từ stored procedure
    public class AppointmentQueryResult
    {
        public int AppointmentId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string TimeSlot { get; set; }
        public string Status { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal? DepositAmount { get; set; }
        public bool DepositPaid { get; set; }
        public decimal? RemainingAmount { get; set; }
        public bool FullyPaid { get; set; }
        public string PaymentMethod { get; set; }
        public string CustomerNotes { get; set; }
        public string StaffNotes { get; set; }
        public int PetId { get; set; }
        public string PetName { get; set; }
        public string Species { get; set; }
        public int? StaffId { get; set; }
        public string StaffName { get; set; }
        public string Services { get; set; }
    }
}