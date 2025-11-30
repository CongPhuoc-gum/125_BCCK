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

                // Debug: Log thông tin
                ViewBag.DebugInfo = $"Staff ID: {staffId}, Today: {today:dd/MM/yyyy}";

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

                // Lịch hôm nay (list)
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
                        CustomerNotes = a.CustomerNotes
                    })
                    .ToList();

                var viewModel = new StaffDashboardViewModel
                {
                    TodayAppointmentsCount = todayCount,
                    WeekAppointmentsCount = weekCount,
                    TodayAppointments = todayAppointments ?? new List<AppointmentListItemViewModel>()
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
                    TodayAppointments = new List<AppointmentListItemViewModel>()
                });
            }
        }

        // 4.2. Lịch hẹn được giao
        public ActionResult Appointments(string filter = "today", string status = "")
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
                default:
                    // Mặc định hiển thị tất cả nếu không có filter
                    break;
            }

            // Lọc theo trạng thái
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(a => a.Status == status);
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

            return View(appointments);
        }

        // 4.3. Chi tiết lịch hẹn
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
                CreatedAt = appointment.CreatedAt,

                CustomerId = appointment.Customer.UserId,
                CustomerName = appointment.Customer.FullName,
                CustomerEmail = appointment.Customer.Email,
                CustomerPhone = appointment.Customer.Phone,
                CustomerAddress = appointment.Customer.Address,

                PetId = appointment.Pet.PetId,
                PetName = appointment.Pet.PetName,
                Species = appointment.Pet.Species,
                Breed = appointment.Pet.Breed,
                Age = appointment.Pet.Age,
                Weight = appointment.Pet.Weight,
                Gender = appointment.Pet.Gender,
                Color = appointment.Pet.Color,
                SpecialNotes = appointment.Pet.SpecialNotes,

                Services = appointment.AppointmentServices.Select(aps =>
                {
                    var service = db.Services.FirstOrDefault(s => s.ServiceId == aps.ServiceId);
                    return new ServiceItemViewModel
                    {
                        ServiceName = service.ServiceName,
                        Category = service.Category,
                        Duration = service.Duration,
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

        // 4.4. Từ chối lịch hẹn
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

        // 4.4. Hoàn thành lịch hẹn
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

            return View(new CompleteAppointmentViewModel { AppointmentId = id });
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
                    // Kiểm tra loại file
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var extension = System.IO.Path.GetExtension(model.ImageFile.FileName).ToLower();
                    
                    if (!allowedExtensions.Contains(extension))
                    {
                        ModelState.AddModelError("ImageFile", "Chỉ chấp nhận file ảnh (JPG, PNG, GIF)");
                        return View(model);
                    }

                    // Kiểm tra kích thước (5MB)
                    if (model.ImageFile.ContentLength > 5 * 1024 * 1024)
                    {
                        ModelState.AddModelError("ImageFile", "Kích thước ảnh không được vượt quá 5MB");
                        return View(model);
                    }

                    // Tạo tên file unique
                    var fileName = $"appointment_{model.AppointmentId}_{DateTime.Now:yyyyMMddHHmmss}{extension}";
                    
                    // Tạo thư mục nếu chưa có
                    var uploadPath = Server.MapPath("~/images/appointments");
                    if (!System.IO.Directory.Exists(uploadPath))
                    {
                        System.IO.Directory.CreateDirectory(uploadPath);
                    }

                    // Lưu file
                    var filePath = System.IO.Path.Combine(uploadPath, fileName);
                    model.ImageFile.SaveAs(filePath);

                    // Lưu đường dẫn relative
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

            appointment.Status = "Completed";
            appointment.StaffNotes = staffNotes;
            appointment.UpdatedAt = DateTime.Now;
            db.SaveChanges();

            TempData["SuccessMessage"] = "Đã hoàn thành lịch hẹn thành công";
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
