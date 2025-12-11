using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using _125_BCCK.Models;
using _125_BCCK.Models.ViewModels;

namespace _125_BCCK.Controllers
{
    public class StaffController : Controller
    {
        private PetCareContext db = new PetCareContext();

        // 4.1. Dashboard Staff
        // 4.1. Dashboard Staff - CẢI TIẾN: Hiển thị lịch đã hoàn thành
        public ActionResult Index()
        {
            // Kiểm tra đăng nhập và role Staff
            if (Session["UserId"] == null || Session["Role"]?.ToString() != "Staff")
            {
                TempData["Error"] = "Vui lòng đăng nhập với tài khoản Staff";
                return RedirectToAction("Index", "Test");
            }

            try
            {
                var today = DateTime.Today;
                var startOfWeek = today.AddDays(-(int)today.DayOfWeek + 1);
                var endOfWeek = startOfWeek.AddDays(6);
                var staffId = (int)Session["UserId"];

                // ===== THỐNG KÊ =====
                // Số lịch hôm nay
                var todayCount = db.Appointments
                    .Count(a => a.StaffId == staffId &&
                               DbFunctions.TruncateTime(a.AppointmentDate) == today &&
                               a.Status != "Cancelled");

                // Số lịch tuần này
                var weekCount = db.Appointments
                    .Count(a => a.StaffId == staffId &&
                               a.AppointmentDate >= startOfWeek &&
                               a.AppointmentDate <= endOfWeek &&
                               a.Status != "Cancelled");

                // Tổng số lịch đã hoàn thành
                var totalCompletedCount = db.Appointments
                    .Count(a => a.StaffId == staffId && a.Status == "Completed");

                // Số lịch đã hoàn thành nhưng chưa thanh toán đủ
                var unpaidCompletedCount = db.Appointments
                    .Count(a => a.StaffId == staffId &&
                               a.Status == "Completed" &&
                               !a.FullyPaid);

                // ===== LỊCH HÔM NAY =====
                var todayAppointmentsList = db.Appointments
                    .Include(a => a.Customer)
                    .Include(a => a.Pet)
                    .Include(a => a.AppointmentServices)
                    .Where(a => a.StaffId == staffId &&
                               DbFunctions.TruncateTime(a.AppointmentDate) == today &&
                               a.Status != "Cancelled")
                    .OrderBy(a => a.TimeSlot)
                    .ToList();

                var todayAppointments = todayAppointmentsList
                    .Select(a => new AppointmentListItemViewModel
                    {
                        AppointmentId = a.AppointmentId,
                        AppointmentDate = a.AppointmentDate,
                        TimeSlot = a.TimeSlot,
                        Status = a.Status,
                        CustomerName = a.Customer?.FullName ?? "N/A",
                        PetName = a.Pet?.PetName ?? "N/A",
                        Species = a.Pet?.Species ?? "N/A",
                        Services = a.AppointmentServices != null && a.AppointmentServices.Any()
                            ? string.Join(", ", a.AppointmentServices.Select(aps =>
                                db.Services.FirstOrDefault(s => s.ServiceId == aps.ServiceId)?.ServiceName ?? "N/A"))
                            : "Chưa có dịch vụ",
                        TotalPrice = a.TotalPrice,
                        CustomerNotes = a.CustomerNotes,
                        DepositAmount = a.DepositAmount,
                        DepositPaid = a.DepositPaid,
                        RemainingAmount = a.RemainingAmount,
                        FullyPaid = a.FullyPaid
                    })
                    .ToList();

                // ===== LỊCH ĐÃ HOÀN THÀNH GẦN ĐÂY (30 ngày gần nhất) =====
                var thirtyDaysAgo = today.AddDays(-30);
                var completedAppointmentsList = db.Appointments
                    .Include(a => a.Customer)
                    .Include(a => a.Pet)
                    .Include(a => a.AppointmentServices)
                    .Where(a => a.StaffId == staffId &&
                               a.Status == "Completed" &&
                               a.AppointmentDate >= thirtyDaysAgo &&
                               a.AppointmentDate <= today)
                    .OrderByDescending(a => a.AppointmentDate)
                    .ThenByDescending(a => a.TimeSlot)
                    .Take(15) // Lấy 15 lịch gần nhất
                    .ToList();

                var completedAppointments = completedAppointmentsList
                    .Select(a => new AppointmentListItemViewModel
                    {
                        AppointmentId = a.AppointmentId,
                        AppointmentDate = a.AppointmentDate,
                        TimeSlot = a.TimeSlot,
                        Status = a.Status,
                        CustomerName = a.Customer?.FullName ?? "N/A",
                        PetName = a.Pet?.PetName ?? "N/A",
                        Species = a.Pet?.Species ?? "N/A",
                        Services = a.AppointmentServices != null && a.AppointmentServices.Any()
                            ? string.Join(", ", a.AppointmentServices.Select(aps =>
                                db.Services.FirstOrDefault(s => s.ServiceId == aps.ServiceId)?.ServiceName ?? "N/A"))
                            : "Chưa có dịch vụ",
                        TotalPrice = a.TotalPrice,
                        CustomerNotes = a.CustomerNotes,
                        DepositAmount = a.DepositAmount,
                        DepositPaid = a.DepositPaid,
                        RemainingAmount = a.RemainingAmount,
                        FullyPaid = a.FullyPaid
                    })
                    .ToList();

                var viewModel = new StaffDashboardViewModel
                {
                    TodayAppointmentsCount = todayCount,
                    WeekAppointmentsCount = weekCount,
                    TodayAppointments = todayAppointments ?? new List<AppointmentListItemViewModel>(),

                    // Thông tin lịch đã hoàn thành
                    TotalCompletedCount = totalCompletedCount,
                    CompletedAppointments = completedAppointments ?? new List<AppointmentListItemViewModel>(),
                    UnpaidCompletedCount = unpaidCompletedCount
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi: " + ex.Message;
                ViewBag.ErrorDetails = ex.ToString();
                return View(new StaffDashboardViewModel
                {
                    TodayAppointmentsCount = 0,
                    WeekAppointmentsCount = 0,
                    TodayAppointments = new List<AppointmentListItemViewModel>(),
                    TotalCompletedCount = 0,
                    CompletedAppointments = new List<AppointmentListItemViewModel>(),
                    UnpaidCompletedCount = 0
                });
            }
        }

        // 4.2. Lịch hẹn được giao
        public ActionResult Appointments(string filter = "today", string status = "", string paymentStatus = "")
        {
            if (Session["UserId"] == null || Session["Role"]?.ToString() != "Staff")
            {
                return RedirectToAction("Login", "Account");
            }

            var staffId = (int)Session["UserId"];
            var today = DateTime.Today;
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek + 1);
            var endOfWeek = startOfWeek.AddDays(6);
            var startOfMonth = new DateTime(today.Year, today.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            var query = db.Appointments
                .Include(a => a.Customer)
                .Include(a => a.Pet)
                .Include(a => a.AppointmentServices)
                .Where(a => a.StaffId == staffId);

            // Lọc theo thời gian
            switch (filter)
            {
                case "today":
                    query = query.Where(a => DbFunctions.TruncateTime(a.AppointmentDate) == today);
                    break;
                case "week":
                    query = query.Where(a => a.AppointmentDate >= startOfWeek && a.AppointmentDate <= endOfWeek);
                    break;
                case "month":
                    query = query.Where(a => a.AppointmentDate >= startOfMonth && a.AppointmentDate <= endOfMonth);
                    break;
                case "all":
                    // Không lọc gì, hiển thị tất cả
                    break;
            }

            // Lọc theo trạng thái lịch hẹn
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(a => a.Status == status);
            }

            // Lọc theo trạng thái thanh toán
            if (!string.IsNullOrEmpty(paymentStatus))
            {
                switch (paymentStatus)
                {
                    case "no_deposit":
                        query = query.Where(a => !a.DepositPaid);
                        break;
                    case "deposited":
                        query = query.Where(a => a.DepositPaid && !a.FullyPaid);
                        break;
                    case "fully_paid":
                        query = query.Where(a => a.FullyPaid);
                        break;
                }
            }

            var appointments = query
                .OrderByDescending(a => a.AppointmentDate)
                .ThenBy(a => a.TimeSlot)
                .ToList()
                .Select(a => new AppointmentListItemViewModel
                {
                    AppointmentId = a.AppointmentId,
                    AppointmentDate = a.AppointmentDate,
                    TimeSlot = a.TimeSlot,
                    Status = a.Status,
                    CustomerName = a.Customer.FullName,
                    PetName = a.Pet.PetName,
                    Species = a.Pet.Species,
                    Services = string.Join(", ", a.AppointmentServices.Select(aps =>
                        db.Services.FirstOrDefault(s => s.ServiceId == aps.ServiceId).ServiceName)),
                    TotalPrice = a.TotalPrice,
                    CustomerNotes = a.CustomerNotes
                })
                .ToList();

            ViewBag.Filter = filter;
            ViewBag.Status = status;
            ViewBag.PaymentStatus = paymentStatus;

            return View(appointments);
        }

        // 4.3. Chi tiết lịch hẹn (CẬP NHẬT - Thêm thông tin thanh toán)
        public ActionResult Details(int id)
        {
            if (Session["UserId"] == null || Session["Role"]?.ToString() != "Staff")
            {
                return RedirectToAction("Login", "Account");
            }

            var appointment = db.Appointments
                .Include(a => a.Customer)
                .Include(a => a.Pet)
                .Include(a => a.AppointmentServices)
                .FirstOrDefault(a => a.AppointmentId == id);

            if (appointment == null)
            {
                return HttpNotFound();
            }

            // Lấy lịch sử thanh toán
            var paymentHistory = db.PaymentTransactions
                .Where(pt => pt.AppointmentId == id)
                .OrderBy(pt => pt.PaymentDate)
                .Select(pt => new PaymentTransactionViewModel
                {
                    TransactionId = pt.TransactionId,
                    TransactionType = pt.TransactionType,
                    Amount = pt.Amount,
                    PaymentMethod = pt.PaymentMethod,
                    PaymentDate = pt.PaymentDate,
                    ProcessedByName = pt.ProcessedBy.HasValue
                        ? db.Users.FirstOrDefault(u => u.UserId == pt.ProcessedBy.Value).FullName
                        : "Hệ thống"
                })
                .ToList();

            var viewModel = new AppointmentDetailViewModel
            {
                AppointmentId = appointment.AppointmentId,
                AppointmentDate = appointment.AppointmentDate,
                TimeSlot = appointment.TimeSlot,
                Status = appointment.Status,
                TotalPrice = appointment.TotalPrice,
                CustomerNotes = appointment.CustomerNotes,
                StaffNotes = appointment.StaffNotes,
                CancelReason = appointment.CancelReason,

                // Thông tin thanh toán - FIX DECIMAL NULLABLE
                DepositAmount = appointment.DepositAmount ?? 0,
                DepositPaid = appointment.DepositPaid,
                RemainingAmount = appointment.RemainingAmount ?? 0,
                FullyPaid = appointment.FullyPaid,
                PaymentMethod = appointment.PaymentMethod,
                PaymentHistory = paymentHistory,

                CustomerId = appointment.Customer.UserId,
                CustomerName = appointment.Customer.FullName,
                CustomerEmail = appointment.Customer.Email,
                CustomerPhone = appointment.Customer.Phone,

                PetId = appointment.Pet.PetId,
                PetName = appointment.Pet.PetName,
                Species = appointment.Pet.Species,
                Breed = appointment.Pet.Breed,

                Services = appointment.AppointmentServices.Select(aps =>
                {
                    var service = db.Services.FirstOrDefault(s => s.ServiceId == aps.ServiceId);
                    return new AppointmentServiceDetailViewModel
                    {
                        ServiceId = service.ServiceId,
                        ServiceName = service.ServiceName,
                        Category = service.Category,
                        Price = aps.ServicePrice
                    };
                }).ToList()
            };

            return View(viewModel);
        }

        // 4.4. Xác nhận lịch hẹn
        [HttpPost]
        public ActionResult Confirm(int id)
        {
            if (Session["UserId"] == null || Session["Role"]?.ToString() != "Staff")
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập" });
            }

            var appointment = db.Appointments.Find(id);
            if (appointment == null)
            {
                return Json(new { success = false, message = "Không tìm thấy lịch hẹn" });
            }

            if (appointment.Status != "Pending")
            {
                return Json(new { success = false, message = "Chỉ có thể xác nhận lịch hẹn đang chờ xử lý" });
            }

            appointment.Status = "Confirmed";
            appointment.UpdatedAt = DateTime.Now;
            db.SaveChanges();

            return Json(new { success = true, message = "Đã xác nhận lịch hẹn thành công" });
        }

        // 4.5. Từ chối lịch hẹn
        [HttpPost]
        public ActionResult Cancel(int id, string reason)
        {
            if (Session["UserId"] == null || Session["Role"]?.ToString() != "Staff")
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập" });
            }

            if (string.IsNullOrWhiteSpace(reason))
            {
                return Json(new { success = false, message = "Vui lòng nhập lý do từ chối" });
            }

            var appointment = db.Appointments.Find(id);
            if (appointment == null)
            {
                return Json(new { success = false, message = "Không tìm thấy lịch hẹn" });
            }

            appointment.Status = "Cancelled";
            appointment.CancelReason = reason;
            appointment.UpdatedAt = DateTime.Now;
            db.SaveChanges();

            return Json(new { success = true, message = "Đã từ chối lịch hẹn" });
        }

        // 4.6. Hoàn thành lịch hẹn (CẬP NHẬT - Có thể thu tiền luôn)
        [HttpGet]
        public ActionResult Complete(int id)
        {
            if (Session["UserId"] == null || Session["Role"]?.ToString() != "Staff")
            {
                return RedirectToAction("Login", "Account");
            }

            var appointment = db.Appointments.Find(id);
            if (appointment == null)
            {
                return HttpNotFound();
            }

            return View(new CompleteAppointmentViewModel
            {
                AppointmentId = id,
                RemainingAmount = appointment.RemainingAmount ?? 0,
                NeedPayment = !appointment.FullyPaid
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Complete(CompleteAppointmentViewModel model)
        {
            if (Session["UserId"] == null || Session["Role"]?.ToString() != "Staff")
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var appointment = db.Appointments.Find(model.AppointmentId);
            if (appointment == null)
            {
                return HttpNotFound();
            }

            // Xử lý upload ảnh
            string imageUrl = null;
            if (model.ImageFile != null && model.ImageFile.ContentLength > 0)
            {
                try
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var extension = System.IO.Path.GetExtension(model.ImageFile.FileName).ToLower();

                    if (!allowedExtensions.Contains(extension))
                    {
                        ModelState.AddModelError("ImageFile", "Chỉ chấp nhận file ảnh (JPG, PNG, GIF)");
                        return View(model);
                    }

                    if (model.ImageFile.ContentLength > 5 * 1024 * 1024)
                    {
                        ModelState.AddModelError("ImageFile", "Kích thước ảnh không được vượt quá 5MB");
                        return View(model);
                    }

                    var fileName = $"appointment_{model.AppointmentId}_{DateTime.Now:yyyyMMddHHmmss}{extension}";
                    var uploadPath = Server.MapPath("~/images/appointments");

                    if (!System.IO.Directory.Exists(uploadPath))
                    {
                        System.IO.Directory.CreateDirectory(uploadPath);
                    }

                    var filePath = System.IO.Path.Combine(uploadPath, fileName);
                    model.ImageFile.SaveAs(filePath);
                    imageUrl = $"/images/appointments/{fileName}";
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("ImageFile", "Lỗi khi tải ảnh lên: " + ex.Message);
                    return View(model);
                }
            }

            // Tạo staff notes
            var staffNotes = $"Tình trạng: {model.PetCondition}";
            if (!string.IsNullOrWhiteSpace(model.Notes))
            {
                staffNotes += $"\nGhi chú: {model.Notes}";
            }
            if (!string.IsNullOrWhiteSpace(imageUrl))
            {
                staffNotes += $"\nHình ảnh: {imageUrl}";
            }

            // Cập nhật trạng thái hoàn thành
            appointment.Status = "Completed";
            appointment.StaffNotes = staffNotes;
            appointment.UpdatedAt = DateTime.Now;

            // Nếu khách thanh toán ngay phần còn lại
            if (model.PayNow && !string.IsNullOrEmpty(model.PaymentMethod) && !appointment.FullyPaid)
            {
                var staffId = (int)Session["UserId"];
                var remainingAmount = appointment.RemainingAmount ?? 0;

                var transaction = new PaymentTransaction
                {
                    AppointmentId = model.AppointmentId,
                    TransactionType = "Final",
                    Amount = remainingAmount,
                    PaymentMethod = model.PaymentMethod,
                    ProcessedBy = staffId,
                    PaymentDate = DateTime.Now,
                    Notes = "Thu tiền khi hoàn thành dịch vụ"
                };
                db.PaymentTransactions.Add(transaction);

                appointment.FullyPaid = true;
                appointment.RemainingAmount = 0;

                TempData["SuccessMessage"] = $"Đã hoàn thành và thu {remainingAmount:N0} VNĐ thành công";
            }
            else
            {
                TempData["SuccessMessage"] = "Đã hoàn thành lịch hẹn";
                if (!appointment.FullyPaid && appointment.RemainingAmount > 0)
                {
                    TempData["WarningMessage"] = $"Còn {appointment.RemainingAmount:N0} VNĐ chưa thanh toán";
                }
            }

            db.SaveChanges();
            return RedirectToAction("Details", new { id = model.AppointmentId });
        }

        // 4.7. MỚI - Xử lý thanh toán riêng
        [HttpGet]
        public ActionResult ProcessPayment(int id)
        {
            if (Session["UserId"] == null || Session["Role"]?.ToString() != "Staff")
            {
                return RedirectToAction("Login", "Account");
            }

            var appointment = db.Appointments
                .Include(a => a.Customer)
                .Include(a => a.Pet)
                .FirstOrDefault(a => a.AppointmentId == id);

            if (appointment == null)
            {
                return HttpNotFound();
            }

            if (appointment.FullyPaid)
            {
                TempData["Error"] = "Lịch hẹn này đã thanh toán đầy đủ";
                return RedirectToAction("Details", new { id });
            }

            var viewModel = new ProcessPaymentViewModel
            {
                AppointmentId = id,
                CustomerName = appointment.Customer.FullName,
                PetName = appointment.Pet.PetName,
                TotalPrice = appointment.TotalPrice,
                DepositAmount = appointment.DepositAmount ?? 0,
                RemainingAmount = appointment.RemainingAmount ?? 0,
                AlreadyPaid = appointment.DepositPaid
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ProcessPayment(ProcessPaymentViewModel model)
        {
            if (Session["UserId"] == null || Session["Role"]?.ToString() != "Staff")
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var appointment = db.Appointments.Find(model.AppointmentId);
            if (appointment == null)
            {
                return HttpNotFound();
            }

            if (appointment.FullyPaid)
            {
                TempData["Error"] = "Lịch hẹn này đã thanh toán đầy đủ";
                return RedirectToAction("Details", new { id = model.AppointmentId });
            }

            var staffId = (int)Session["UserId"];
            var remainingAmount = appointment.RemainingAmount ?? 0;

            // Tạo giao dịch thanh toán
            var transaction = new PaymentTransaction
            {
                AppointmentId = model.AppointmentId,
                TransactionType = "Final",
                Amount = remainingAmount,
                PaymentMethod = model.PaymentMethod,
                ProcessedBy = staffId,
                PaymentDate = DateTime.Now,
                Notes = model.Notes
            };
            db.PaymentTransactions.Add(transaction);

            // Cập nhật trạng thái thanh toán
            appointment.FullyPaid = true;
            appointment.RemainingAmount = 0;
            appointment.UpdatedAt = DateTime.Now;

            db.SaveChanges();

            TempData["SuccessMessage"] = $"Đã thu {remainingAmount:N0} VNĐ thành công";
            return RedirectToAction("Details", new { id = model.AppointmentId });
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
}