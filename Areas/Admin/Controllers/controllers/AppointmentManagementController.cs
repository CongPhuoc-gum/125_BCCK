using System;
using System.Collections.Generic; // ← THÊM DÒNG NÀY
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using _125_BCCK.Models;
using _125_BCCK.Models.ViewModels;

namespace _125_BCCK.Areas.Admin.Controllers
{
    [RouteArea("Admin", AreaPrefix = "admin")]
    [RoutePrefix("appointments")]
    public class AppointmentManagementController : Controller
    {
        private PetCareContext db = new PetCareContext();

        // GET: Admin/AppointmentManagement
        [Route("")]
        public ActionResult Index(string status = "Pending")
        {
            var appointments = db.Appointments
                .Include(a => a.Customer)
                .Include(a => a.Pet)
                .Include(a => a.Staff)
                .Where(a => status == "All" || a.Status == status)
                .OrderBy(a => a.AppointmentDate)
                .ThenBy(a => a.TimeSlot)
                .ToList();

            var viewModel = appointments.Select(a => new AppointmentListItemViewModel
            {
                AppointmentId = a.AppointmentId,
                AppointmentDate = a.AppointmentDate,
                TimeSlot = a.TimeSlot,
                Status = a.Status,
                CustomerName = a.Customer.FullName,
                PetName = a.Pet.PetName,
                Species = a.Pet.Species,
                TotalPrice = a.TotalPrice,
                DepositAmount = a.DepositAmount,
                DepositPaid = a.DepositPaid,
                RemainingAmount = a.RemainingAmount,
                FullyPaid = a.FullyPaid,
                CustomerNotes = a.CustomerNotes,
                Services = string.Join(", ", db.AppointmentServices
                    .Where(aps => aps.AppointmentId == a.AppointmentId)
                    .Select(aps => aps.Service.ServiceName))
            }).ToList();

            ViewBag.CurrentStatus = status;

            // ĐỔI PHẦN NÀY - Dùng Dictionary thay vì anonymous object
            ViewBag.StatusCounts = new Dictionary<string, int>
            {
                { "Pending", db.Appointments.Count(a => a.Status == "Pending") },
                { "Confirmed", db.Appointments.Count(a => a.Status == "Confirmed") },
                { "InProgress", db.Appointments.Count(a => a.Status == "InProgress") },
                { "Completed", db.Appointments.Count(a => a.Status == "Completed") },
                { "Cancelled", db.Appointments.Count(a => a.Status == "Cancelled") }
            };

            return View(viewModel);
        }

        // GET: Admin/AppointmentManagement/Details/5
        [Route("details/{id:int}")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var appointment = db.Appointments
                .Include(a => a.Customer)
                .Include(a => a.Pet)
                .Include(a => a.Staff)
                .FirstOrDefault(a => a.AppointmentId == id);

            if (appointment == null)
            {
                return HttpNotFound();
            }

            // Lấy danh sách dịch vụ
            var services = db.AppointmentServices
                .Where(aps => aps.AppointmentId == id)
                .Select(aps => new AppointmentServiceDetailViewModel
                {
                    ServiceId = aps.Service.ServiceId,
                    ServiceName = aps.Service.ServiceName,
                    Category = aps.Service.Category,
                    Duration = aps.Service.Duration,
                    Price = aps.ServicePrice
                }).ToList();

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
                    ProcessedByName = pt.ProcessedBy != null ? db.Users.FirstOrDefault(u => u.UserId == pt.ProcessedBy).FullName : "Tự động"
                }).ToList();

            var viewModel = new AppointmentDetailViewModel
            {
                AppointmentId = appointment.AppointmentId,
                AppointmentDate = appointment.AppointmentDate,
                TimeSlot = appointment.TimeSlot,
                Status = appointment.Status,
                CustomerName = appointment.Customer.FullName,
                CustomerEmail = appointment.Customer.Email,
                CustomerPhone = appointment.Customer.Phone,
                CustomerAddress = appointment.Customer.Address,
                CustomerId = appointment.CustomerId,
                PetId = appointment.PetId,
                PetName = appointment.Pet.PetName,
                Species = appointment.Pet.Species,
                Breed = appointment.Pet.Breed,
                Age = appointment.Pet.Age,
                Weight = appointment.Pet.Weight,
                Gender = appointment.Pet.Gender,
                Color = appointment.Pet.Color,
                SpecialNotes = appointment.Pet.SpecialNotes,
                StaffId = appointment.StaffId,
                StaffName = appointment.Staff?.FullName,
                StaffPhone = appointment.Staff?.Phone,
                Services = services,
                TotalPrice = appointment.TotalPrice,
                DepositAmount = (decimal)appointment.DepositAmount,
                DepositPaid = appointment.DepositPaid,
                RemainingAmount = appointment.RemainingAmount ?? 0,
                FullyPaid = appointment.FullyPaid,
                PaymentMethod = appointment.PaymentMethod,
                CustomerNotes = appointment.CustomerNotes,
                StaffNotes = appointment.StaffNotes,
                CancelReason = appointment.CancelReason,
                CreatedAt = appointment.CreatedAt,
                PaymentHistory = paymentHistory
            };

            return View(viewModel);
        }

        // GET: Admin/AppointmentManagement/AssignStaff/5
        [Route("assign-staff/{id:int}")]
        public ActionResult AssignStaff(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var appointment = db.Appointments
                .Include(a => a.Customer)
                .Include(a => a.Pet)
                .Include(a => a.Staff)
                .FirstOrDefault(a => a.AppointmentId == id);

            if (appointment == null)
            {
                return HttpNotFound();
            }

            var viewModel = new AssignStaffViewModel
            {
                AppointmentId = appointment.AppointmentId,
                AppointmentDate = appointment.AppointmentDate,
                TimeSlot = appointment.TimeSlot,
                CustomerName = appointment.Customer.FullName,
                PetName = appointment.Pet.PetName,
                PetSpecies = appointment.Pet.Species,
                CurrentStaffId = appointment.StaffId,
                StaffNotes = appointment.StaffNotes
            };

            // Lấy danh sách nhân viên còn hoạt động
            ViewBag.StaffList = new SelectList(
                db.Users.Where(u => u.Role == "Staff" && u.IsActive)
                    .OrderBy(u => u.FullName)
                    .ToList(),
                "UserId",
                "FullName",
                appointment.StaffId
            );

            return View(viewModel);
        }

        // POST: Admin/AppointmentManagement/AssignStaff
        [HttpPost]
        [Route("assign-staff")]
        [ValidateAntiForgeryToken]
        public ActionResult AssignStaff(AssignStaffViewModel model)
        {
            if (ModelState.IsValid)
            {
                var appointment = db.Appointments.Find(model.AppointmentId);
                if (appointment == null)
                {
                    return HttpNotFound();
                }

                appointment.StaffId = model.SelectedStaffId;
                appointment.Status = "Confirmed"; // Tự động chuyển sang Confirmed khi phân công
                appointment.StaffNotes = model.StaffNotes;
                appointment.UpdatedAt = DateTime.Now;

                db.Entry(appointment).State = EntityState.Modified;
                db.SaveChanges();

                TempData["SuccessMessage"] = "Phân công nhân viên thành công!";
                return RedirectToAction("Index");
            }

            // Nếu validation fail, load lại staff list
            ViewBag.StaffList = new SelectList(
                db.Users.Where(u => u.Role == "Staff" && u.IsActive).ToList(),
                "UserId",
                "FullName"
            );

            return View(model);
        }

        // POST: Admin/AppointmentManagement/UpdateStatus
        [HttpPost]
        [Route("update-status")]
        public JsonResult UpdateStatus(int appointmentId, string status)
        {
            try
            {
                var appointment = db.Appointments.Find(appointmentId);
                if (appointment == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy lịch hẹn" });
                }

                appointment.Status = status;
                appointment.UpdatedAt = DateTime.Now;

                db.Entry(appointment).State = EntityState.Modified;
                db.SaveChanges();

                return Json(new { success = true, message = "Cập nhật trạng thái thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        // POST: Admin/AppointmentManagement/CancelAppointment
        [HttpPost]
        [Route("cancel")]
        [ValidateAntiForgeryToken]
        public ActionResult CancelAppointment(int appointmentId, string cancelReason)
        {
            try
            {
                var appointment = db.Appointments.Find(appointmentId);
                if (appointment == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy lịch hẹn";
                    return RedirectToAction("Index");
                }

                appointment.Status = "Cancelled";
                appointment.CancelReason = cancelReason;
                appointment.UpdatedAt = DateTime.Now;

                db.Entry(appointment).State = EntityState.Modified;
                db.SaveChanges();

                TempData["SuccessMessage"] = "Đã hủy lịch hẹn";
                return RedirectToAction("Details", new { id = appointmentId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra: " + ex.Message;
                return RedirectToAction("Index");
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
}