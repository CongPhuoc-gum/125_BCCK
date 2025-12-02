using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using _125_BCCK.Helpers;
using _125_BCCK.Models;
using _125_BCCK.Models.ViewModels;

namespace _125_BCCK.Controllers
{
    public class PetsController : Controller
    {
        private PetCareContext db = new PetCareContext();

        // ==========================================
        // GET: Pets/Index
        // ==========================================
        public ActionResult Index()
        {
            if (!SessionHelper.IsLoggedIn())
            {
                TempData["Error"] = "Vui lòng đăng nhập để xem thú cưng của bạn";
                return RedirectToAction("Login", "Account");
            }

            int? userId = SessionHelper.GetUserId();
            if (!userId.HasValue)
            {
                TempData["Error"] = "Phiên đăng nhập không hợp lệ";
                SessionHelper.ClearSession();
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var pets = db.Pets
                            .Where(p => p.OwnerId == userId.Value && p.IsActive == true)
                            .OrderByDescending(p => p.CreatedAt)
                            .ToList();

                return View(pets);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi khi tải danh sách thú cưng: " + ex.Message;
                return View(new List<Pet>());
            }
        }

        // ==========================================
        // GET: Pets/Details/5 - CHI TIẾT ĐẦY ĐỦ
        // ==========================================
        public ActionResult Details(int? id)
        {
            if (!SessionHelper.IsLoggedIn())
            {
                TempData["Error"] = "Vui lòng đăng nhập";
                return RedirectToAction("Login", "Account");
            }

            if (id == null || id == 0)
            {
                TempData["Error"] = "Không tìm thấy thú cưng";
                return RedirectToAction("Index");
            }

            int? userId = SessionHelper.GetUserId();
            if (!userId.HasValue)
            {
                TempData["Error"] = "Phiên đăng nhập không hợp lệ";
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var pet = db.Pets.FirstOrDefault(p =>
                    p.PetId == id &&
                    p.OwnerId == userId.Value &&
                    p.IsActive == true);

                if (pet == null)
                {
                    TempData["Error"] = "Không tìm thấy thú cưng";
                    return RedirectToAction("Index");
                }

                // Tạo ViewModel với đầy đủ thông tin
                var viewModel = new PetDetailsViewModel
                {
                    Pet = pet,
                    VaccinationHistory = GetVaccinationHistory(id.Value),
                    ServiceHistory = GetServiceHistory(id.Value),
                    Statistics = GetPetStatistics(id.Value)
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // ==========================================
        // GET: Pets/Create
        // ==========================================
        public ActionResult Create()
        {
            if (!SessionHelper.IsLoggedIn())
            {
                TempData["Error"] = "Vui lòng đăng nhập";
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        // ==========================================
        // POST: Pets/Create
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Pet pet, HttpPostedFileBase ImageFile)
        {
            if (!SessionHelper.IsLoggedIn())
            {
                TempData["Error"] = "Vui lòng đăng nhập";
                return RedirectToAction("Login", "Account");
            }

            ModelState.Remove("ImageFile");
            ModelState.Remove("Owner");

            if (ModelState.IsValid)
            {
                try
                {
                    int? userId = SessionHelper.GetUserId();
                    if (!userId.HasValue)
                    {
                        TempData["Error"] = "Phiên đăng nhập không hợp lệ";
                        return RedirectToAction("Login", "Account");
                    }

                    if (ImageFile != null && ImageFile.ContentLength > 0)
                    {
                        string imageUrl = SavePetImage(ImageFile);
                        if (imageUrl != null)
                        {
                            pet.ImageUrl = imageUrl;
                        }
                        else
                        {
                            return View(pet);
                        }
                    }

                    pet.OwnerId = userId.Value;
                    pet.IsActive = true;
                    pet.CreatedAt = DateTime.Now;

                    db.Pets.Add(pet);
                    db.SaveChanges();

                    TempData["Success"] = $"Thêm thú cưng '{pet.PetName}' thành công!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Có lỗi xảy ra: " + ex.Message;
                }
            }

            return View(pet);
        }

        // ==========================================
        // GET: Pets/Edit/5
        // ==========================================
        public ActionResult Edit(int? id)
        {
            if (!SessionHelper.IsLoggedIn())
            {
                TempData["Error"] = "Vui lòng đăng nhập";
                return RedirectToAction("Login", "Account");
            }

            if (id == null || id == 0)
            {
                TempData["Error"] = "Không tìm thấy thú cưng";
                return RedirectToAction("Index");
            }

            int? userId = SessionHelper.GetUserId();
            if (!userId.HasValue)
            {
                TempData["Error"] = "Phiên đăng nhập không hợp lệ";
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var pet = db.Pets.FirstOrDefault(p =>
                    p.PetId == id &&
                    p.OwnerId == userId.Value &&
                    p.IsActive == true);

                if (pet == null)
                {
                    TempData["Error"] = "Không tìm thấy thú cưng hoặc bạn không có quyền chỉnh sửa";
                    return RedirectToAction("Index");
                }

                return View(pet);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // ==========================================
        // POST: Pets/Edit/5
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Pet pet, HttpPostedFileBase ImageFile)
        {
            if (!SessionHelper.IsLoggedIn())
            {
                TempData["Error"] = "Vui lòng đăng nhập";
                return RedirectToAction("Login", "Account");
            }

            ModelState.Remove("ImageFile");
            ModelState.Remove("Owner");

            if (ModelState.IsValid)
            {
                try
                {
                    int? userId = SessionHelper.GetUserId();
                    if (!userId.HasValue)
                    {
                        TempData["Error"] = "Phiên đăng nhập không hợp lệ";
                        return RedirectToAction("Login", "Account");
                    }

                    var existingPet = db.Pets.FirstOrDefault(p =>
                        p.PetId == pet.PetId &&
                        p.OwnerId == userId.Value &&
                        p.IsActive == true);

                    if (existingPet == null)
                    {
                        TempData["Error"] = "Không tìm thấy thú cưng hoặc bạn không có quyền chỉnh sửa";
                        return RedirectToAction("Index");
                    }

                    if (ImageFile != null && ImageFile.ContentLength > 0)
                    {
                        if (!string.IsNullOrEmpty(existingPet.ImageUrl))
                        {
                            DeletePetImage(existingPet.ImageUrl);
                        }

                        string imageUrl = SavePetImage(ImageFile);
                        if (imageUrl != null)
                        {
                            existingPet.ImageUrl = imageUrl;
                        }
                        else
                        {
                            return View(pet);
                        }
                    }

                    existingPet.PetName = pet.PetName;
                    existingPet.Species = pet.Species;
                    existingPet.Breed = pet.Breed;
                    existingPet.Age = pet.Age;
                    existingPet.Weight = pet.Weight;
                    existingPet.Gender = pet.Gender;
                    existingPet.Color = pet.Color;
                    existingPet.SpecialNotes = pet.SpecialNotes;

                    db.SaveChanges();

                    TempData["Success"] = $"Cập nhật thông tin '{pet.PetName}' thành công!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Có lỗi xảy ra: " + ex.Message;
                }
            }

            return View(pet);
        }

        // ==========================================
        // POST: Pets/Delete/5
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            if (!SessionHelper.IsLoggedIn())
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập" });
            }

            try
            {
                int? userId = SessionHelper.GetUserId();
                if (!userId.HasValue)
                {
                    return Json(new { success = false, message = "Phiên đăng nhập không hợp lệ" });
                }

                var pet = db.Pets.FirstOrDefault(p =>
                    p.PetId == id &&
                    p.OwnerId == userId.Value &&
                    p.IsActive == true);

                if (pet == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy thú cưng" });
                }

                if (!string.IsNullOrEmpty(pet.ImageUrl))
                {
                    DeletePetImage(pet.ImageUrl);
                }

                pet.IsActive = false;
                db.SaveChanges();

                return Json(new
                {
                    success = true,
                    message = $"Xóa thú cưng '{pet.PetName}' thành công"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Có lỗi xảy ra: " + ex.Message
                });
            }
        }

        #region Helper Methods - Lấy dữ liệu lịch sử

        /// <summary>
        /// Lấy lịch sử tiêm phòng
        /// </summary>
        private List<VaccinationHistoryItem> GetVaccinationHistory(int petId)
        {
            try
            {
                var records = (from vr in db.VaccinationRecords
                               join staff in db.Users on vr.StaffId equals staff.UserId
                               where vr.PetId == petId
                               orderby vr.VaccinationDate descending
                               select new VaccinationHistoryItem
                               {
                                   VaccineName = vr.VaccineName,
                                   VaccinationDate = vr.VaccinationDate,
                                   NextDueDate = vr.NextDueDate,
                                   Notes = vr.Notes,
                                   VaccinatedBy = staff.FullName
                               }).ToList();

                return records;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                return new List<VaccinationHistoryItem>();
            }
        }

        /// <summary>
        /// Lấy lịch sử sử dụng dịch vụ
        /// </summary>
        private List<ServiceHistoryItem> GetServiceHistory(int petId)
        {
            try
            {
                var appointments = db.Appointments
                    .Where(a => a.PetId == petId && a.Status != "Cancelled")
                    .OrderByDescending(a => a.AppointmentDate)
                    .ToList();

                var history = new List<ServiceHistoryItem>();

                foreach (var appointment in appointments)
                {
                    var services = (from aps in db.AppointmentServices
                                    join s in db.Services on aps.ServiceId equals s.ServiceId
                                    where aps.AppointmentId == appointment.AppointmentId
                                    select s.ServiceName).ToList();

                    string staffName = null;
                    if (appointment.StaffId.HasValue)
                    {
                        var staff = db.Users.FirstOrDefault(u => u.UserId == appointment.StaffId.Value);
                        staffName = staff?.FullName;
                    }

                    history.Add(new ServiceHistoryItem
                    {
                        AppointmentId = appointment.AppointmentId,
                        AppointmentDate = appointment.AppointmentDate,
                        TimeSlot = appointment.TimeSlot,
                        Status = appointment.Status,
                        Services = string.Join(", ", services),
                        TotalPrice = appointment.TotalPrice,
                        FullyPaid = appointment.FullyPaid,
                        StaffName = staffName
                    });
                }

                return history;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                return new List<ServiceHistoryItem>();
            }
        }

        /// <summary>
        /// Lấy thống kê tổng quan
        /// </summary>
        private PetStatistics GetPetStatistics(int petId)
        {
            try
            {
                var stats = new PetStatistics();

                stats.TotalVaccinations = db.VaccinationRecords
                    .Count(vr => vr.PetId == petId);

                stats.TotalAppointments = db.Appointments
                    .Count(a => a.PetId == petId && a.Status != "Cancelled");

                stats.TotalSpent = db.Appointments
                    .Where(a => a.PetId == petId && a.FullyPaid && a.Status != "Cancelled")
                    .Sum(a => (decimal?)a.TotalPrice) ?? 0;

                var lastAppointment = db.Appointments
                    .Where(a => a.PetId == petId && a.Status == "Completed")
                    .OrderByDescending(a => a.AppointmentDate)
                    .FirstOrDefault();

                stats.LastVisit = lastAppointment?.AppointmentDate;

                var thirtyDaysLater = DateTime.Now.AddDays(30);
                stats.UpcomingVaccinations = db.VaccinationRecords
                    .Count(vr => vr.PetId == petId &&
                           vr.NextDueDate.HasValue &&
                           vr.NextDueDate.Value >= DateTime.Now &&
                           vr.NextDueDate.Value <= thirtyDaysLater);

                return stats;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                return new PetStatistics();
            }
        }

        #endregion

        #region Helper Methods - Upload/Delete Image

        private string SavePetImage(HttpPostedFileBase file)
        {
            try
            {
                if (file == null || file.ContentLength == 0)
                    return null;

                string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
                string fileExtension = Path.GetExtension(file.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    TempData["Error"] = "Chỉ chấp nhận file ảnh: JPG, JPEG, PNG, GIF, BMP";
                    return null;
                }

                if (file.ContentLength > 5 * 1024 * 1024)
                {
                    TempData["Error"] = "Kích thước file không được vượt quá 5MB";
                    return null;
                }

                string fileName = Guid.NewGuid().ToString() + fileExtension;
                string uploadFolder = Server.MapPath("~/Content/Images/pets");

                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                string filePath = Path.Combine(uploadFolder, fileName);
                file.SaveAs(filePath);

                return "~/Content/Images/pets/" + fileName;
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi khi upload ảnh: " + ex.Message;
                return null;
            }
        }

        private void DeletePetImage(string imageUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(imageUrl))
                    return;

                string filePath = Server.MapPath(imageUrl);

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting image: {ex.Message}");
            }
        }

        #endregion

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