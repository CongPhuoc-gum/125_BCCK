using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using _125_BCCK.Models;
using _125_BCCK.Models.ViewModels;

namespace _125_BCCK.Areas.Admin.Controllers
{
    [RouteArea("Admin", AreaPrefix = "admin")]
    [RoutePrefix("services")]
    public class ServiceManagementController : Controller
    {
        private PetCareContext db = new PetCareContext();

        // GET: Admin/ServiceManagement
        [Route("")]
        public ActionResult Index(string category = null, string search = null)
        {
            var services = db.Services.AsQueryable();

            if (!string.IsNullOrEmpty(category))
            {
                services = services.Where(s => s.Category == category);
            }

            if (!string.IsNullOrEmpty(search))
            {
                services = services.Where(s => s.ServiceName.Contains(search));
            }

            var viewModel = services
                .OrderBy(s => s.Category)
                .ThenBy(s => s.ServiceName)
                .Select(s => new ServiceItemViewModel
                {
                    ServiceId = s.ServiceId,
                    ServiceName = s.ServiceName,
                    Category = s.Category,
                    Price = s.Price,
                    Duration = s.Duration
                }).ToList();

            ViewBag.CurrentCategory = category;
            ViewBag.SearchTerm = search;
            ViewBag.Categories = db.Services
                .Select(s => s.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            return View(viewModel);
        }

        // GET: Admin/ServiceManagement/Create
        [Route("create")]
        public ActionResult Create()
        {
            var viewModel = new ServiceFormViewModel();
            ViewBag.Categories = GetCategorySelectList();
            return View(viewModel);
        }

        // POST: Admin/ServiceManagement/Create
        [HttpPost]
        [Route("create")]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ServiceFormViewModel model)
        {
            if (ModelState.IsValid)
            {
                // ✅ XỬ LÝ UPLOAD FILE
                string imageUrl = "/Content/Images/services/default.jpg"; // Ảnh mặc định

                if (model.ImageFile != null && model.ImageFile.ContentLength > 0)
                {
                    imageUrl = SaveImageFile(model.ImageFile);
                    if (imageUrl == null)
                    {
                        ModelState.AddModelError("ImageFile", "Không thể lưu file. Vui lòng thử lại.");
                        ViewBag.Categories = GetCategorySelectList();
                        return View(model);
                    }
                }

                var service = new Service
                {
                    ServiceName = model.ServiceName,
                    Description = model.Description,
                    Category = model.Category,
                    Duration = model.Duration,
                    Price = model.Price,
                    ImageUrl = imageUrl,
                    IsActive = model.IsActive,
                    CreatedAt = DateTime.Now
                };

                db.Services.Add(service);
                db.SaveChanges();

                TempData["SuccessMessage"] = "Thêm dịch vụ mới thành công!";
                return RedirectToAction("Index");
            }

            ViewBag.Categories = GetCategorySelectList();
            return View(model);
        }

        // GET: Admin/ServiceManagement/Edit/5
        [Route("edit/{id:int}")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var service = db.Services.Find(id);
            if (service == null)
            {
                return HttpNotFound();
            }

            var viewModel = new ServiceFormViewModel
            {
                ServiceId = service.ServiceId,
                ServiceName = service.ServiceName,
                Description = service.Description,
                Category = service.Category,
                Duration = service.Duration,
                Price = service.Price,
                ImageUrl = service.ImageUrl,
                IsActive = service.IsActive
            };

            ViewBag.Categories = GetCategorySelectList();
            return View(viewModel);
        }

        // POST: Admin/ServiceManagement/Edit/5
        [HttpPost]
        [Route("edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ServiceFormViewModel model)
        {
            if (ModelState.IsValid)
            {
                var service = db.Services.Find(model.ServiceId);
                if (service == null)
                {
                    return HttpNotFound();
                }

                // ✅ XỬ LÝ UPLOAD FILE MỚI
                if (model.ImageFile != null && model.ImageFile.ContentLength > 0)
                {
                    // Xóa ảnh cũ nếu không phải ảnh mặc định
                    if (!string.IsNullOrEmpty(service.ImageUrl) &&
                        service.ImageUrl != "/Content/Images/services/default.jpg")
                    {
                        DeleteImageFile(service.ImageUrl);
                    }

                    // Lưu ảnh mới
                    string newImageUrl = SaveImageFile(model.ImageFile);
                    if (newImageUrl != null)
                    {
                        service.ImageUrl = newImageUrl;
                    }
                    else
                    {
                        ModelState.AddModelError("ImageFile", "Không thể lưu file. Vui lòng thử lại.");
                        ViewBag.Categories = GetCategorySelectList();
                        return View(model);
                    }
                }
                // Nếu không upload file mới, giữ nguyên ImageUrl cũ

                service.ServiceName = model.ServiceName;
                service.Description = model.Description;
                service.Category = model.Category;
                service.Duration = model.Duration;
                service.Price = model.Price;
                service.IsActive = model.IsActive;

                db.Entry(service).State = EntityState.Modified;
                db.SaveChanges();

                TempData["SuccessMessage"] = "Cập nhật dịch vụ thành công!";
                return RedirectToAction("Index");
            }

            ViewBag.Categories = GetCategorySelectList();
            return View(model);
        }

        // GET: Admin/ServiceManagement/Delete/5
        [Route("delete/{id:int}")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var service = db.Services.Find(id);
            if (service == null)
            {
                return HttpNotFound();
            }

            var viewModel = new ServiceItemViewModel
            {
                ServiceId = service.ServiceId,
                ServiceName = service.ServiceName,
                Category = service.Category,
                Price = service.Price,
                Duration = service.Duration
            };

            return View(viewModel);
        }

        // POST: Admin/ServiceManagement/Delete/5
        [HttpPost, ActionName("Delete")]
        [Route("delete/{id:int}")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var service = db.Services.Find(id);
            if (service == null)
            {
                return HttpNotFound();
            }

            var hasAppointments = db.AppointmentServices.Any(a => a.ServiceId == id);

            if (hasAppointments)
            {
                TempData["ErrorMessage"] = "Không thể xóa dịch vụ này vì đã có lịch hẹn sử dụng!";
                return RedirectToAction("Index");
            }

            // Xóa ảnh nếu không phải ảnh mặc định
            if (!string.IsNullOrEmpty(service.ImageUrl) &&
                service.ImageUrl != "/Content/Images/services/default.jpg")
            {
                DeleteImageFile(service.ImageUrl);
            }

            db.Services.Remove(service);
            db.SaveChanges();

            TempData["SuccessMessage"] = "Xóa dịch vụ thành công!";
            return RedirectToAction("Index");
        }

        // POST: Admin/ServiceManagement/ToggleActive/5
        [HttpPost]
        [Route("toggle-active/{id:int}")]
        public JsonResult ToggleActive(int id)
        {
            try
            {
                var service = db.Services.Find(id);
                if (service == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy dịch vụ" });
                }

                service.IsActive = !service.IsActive;
                db.Entry(service).State = EntityState.Modified;
                db.SaveChanges();

                return Json(new
                {
                    success = true,
                    isActive = service.IsActive,
                    message = service.IsActive ? "Đã kích hoạt dịch vụ" : "Đã vô hiệu hóa dịch vụ"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        // ✅ HELPER: Lưu file ảnh
        private string SaveImageFile(HttpPostedFileBase file)
        {
            try
            {
                // Kiểm tra file hợp lệ
                if (file == null || file.ContentLength == 0)
                    return null;

                // Kiểm tra định dạng
                string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
                string fileExtension = Path.GetExtension(file.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                    return null;

                // Kiểm tra kích thước (max 5MB)
                if (file.ContentLength > 5 * 1024 * 1024)
                    return null;

                // Tạo tên file unique
                string fileName = Guid.NewGuid().ToString() + fileExtension;

                // Đường dẫn thư mục lưu
                string uploadPath = Server.MapPath("~/Content/Images/services");

                // Tạo thư mục nếu chưa có
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                // Đường dẫn đầy đủ
                string filePath = Path.Combine(uploadPath, fileName);

                // Lưu file
                file.SaveAs(filePath);

                // Trả về URL tương đối
                return "/Content/Images/services/" + fileName;
            }
            catch (Exception ex)
            {
                // Log error nếu cần
                System.Diagnostics.Debug.WriteLine("Error saving image: " + ex.Message);
                return null;
            }
        }

        // ✅ HELPER: Xóa file ảnh
        private void DeleteImageFile(string imageUrl)
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
                // Log error nếu cần
                System.Diagnostics.Debug.WriteLine("Error deleting image: " + ex.Message);
            }
        }

        // Helper method
        private SelectList GetCategorySelectList()
        {
            var categories = new[]
            {
                new { Value = "Tắm rửa", Text = "🛁 Tắm rửa" },
                new { Value = "Cắt tỉa", Text = "✂️ Cắt tỉa" },
                new { Value = "Y tế", Text = "💊 Y tế" },
                new { Value = "Spa", Text = "💆 Spa" },
                new { Value = "Khác", Text = "📋 Khác" }
            };

            return new SelectList(categories, "Value", "Text");
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