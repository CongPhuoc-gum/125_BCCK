using _125_BCCK.Helpers;
using _125_BCCK.Models;
using _125_BCCK.Models.ViewModels;
using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace _125_BCCK.Controllers
{
    public class AccountController : Controller
    {
        private PetCareContext db = new PetCareContext();

        // GET: Account/Login
        public ActionResult Login()
        {
            if (SessionHelper.IsLoggedIn())
            {
                return RedirectToHome();
            }
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var userByEmail = db.Users.FirstOrDefault(u => u.Email == model.Email);

                if (userByEmail == null)
                {
                    ModelState.AddModelError("", "❌ Email không tồn tại trong hệ thống");
                    return View(model);
                }

                if (!userByEmail.IsActive)
                {
                    ModelState.AddModelError("", "❌ Tài khoản đã bị khóa");
                    return View(model);
                }

                string hashedPassword = SessionHelper.HashPassword(model.Password).Trim();
                string storedHash = (userByEmail.PasswordHash ?? "").Trim();

                System.Diagnostics.Debug.WriteLine("=== DEBUG LOGIN ===");
                System.Diagnostics.Debug.WriteLine($"Email: {model.Email}");
                System.Diagnostics.Debug.WriteLine($"Hash generated: {hashedPassword}");
                System.Diagnostics.Debug.WriteLine($"Hash stored:    {storedHash}");
                System.Diagnostics.Debug.WriteLine($"Match: {string.Equals(hashedPassword, storedHash, StringComparison.Ordinal)}");

                if (!string.Equals(hashedPassword, storedHash, StringComparison.Ordinal))
                {
                    ModelState.AddModelError("", "❌ Mật khẩu không đúng");
                    return View(model);
                }

                SessionHelper.SetUserSession(userByEmail.UserId, userByEmail.FullName, userByEmail.Email, userByEmail.Role);

                TempData["Success"] = "✅ Đăng nhập thành công!";
                return RedirectToHome();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "❌ LỖI: " + ex.Message);
                System.Diagnostics.Debug.WriteLine($"Exception: {ex.Message}");
                return View(model);
            }
        }

        // GET: Account/Register
        public ActionResult Register()
        {
            if (SessionHelper.IsLoggedIn())
            {
                return RedirectToHome();
            }
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                if (db.Users.Any(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "Email này đã được đăng ký");
                    return View(model);
                }

                var user = new User
                {
                    FullName = model.FullName,
                    Email = model.Email,
                    PasswordHash = SessionHelper.HashPassword(model.Password),
                    Phone = model.Phone,
                    Address = model.Address,
                    Role = "Customer",
                    IsActive = true,
                    AvatarUrl = "/Content/Images/default-avatar.png", // Avatar mặc định
                    CreatedAt = DateTime.Now
                };

                db.Users.Add(user);
                db.SaveChanges();

                SessionHelper.SetUserSession(user.UserId, user.FullName, user.Email, user.Role);

                TempData["Success"] = "Đăng ký thành công! Chào mừng bạn đến với PetCare.";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Đã xảy ra lỗi: " + ex.Message);
                return View(model);
            }
        }

        // GET: Account/Logout
        public ActionResult Logout()
        {
            SessionHelper.ClearSession();
            return RedirectToAction("Index", "Home");
        }

        // ==================== THÊM MỚI: PROFILE ====================

        // GET: Account/Profile
        [HttpGet]
        public ActionResult Profile()
        {
            if (!SessionHelper.IsLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            int userId = (int)SessionHelper.GetUserId();

            try
            {
                var user = db.Users.Find(userId);

                if (user == null)
                {
                    TempData["Error"] = "Không tìm thấy thông tin người dùng";
                    return RedirectToAction("Index", "Home");
                }

                var model = new ProfileViewModel
                {
                    UserId = user.UserId,
                    FullName = user.FullName,
                    Email = user.Email,
                    Phone = user.Phone,
                    Address = user.Address,
                    Role = user.Role,
                    CreatedAt = user.CreatedAt,
                    AvatarUrl = string.IsNullOrEmpty(user.AvatarUrl) ? "/Content/Images/default-avatar.png" : user.AvatarUrl
                };

                // Nếu là Customer thì lấy thêm thống kê
                if (user.Role == "Customer")
                {
                    model.TotalPets = db.Pets.Count(p => p.OwnerId == userId && p.IsActive);
                    model.TotalAppointments = db.Appointments.Count(a => a.CustomerId == userId);
                    model.TotalSpent = db.Appointments
                        .Where(a => a.CustomerId == userId && a.FullyPaid)
                        .Sum(a => (decimal?)a.TotalPrice) ?? 0;
                    model.LastVisit = db.Appointments
                        .Where(a => a.CustomerId == userId && a.Status == "Completed")
                        .OrderByDescending(a => a.AppointmentDate)
                        .Select(a => (DateTime?)a.AppointmentDate)
                        .FirstOrDefault();
                }

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra: " + ex.Message;
                return RedirectToAction("Index", "Home");
            }
        }

        // POST: Account/Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Profile(ProfileViewModel model)
        {
            if (!SessionHelper.IsLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            int userId = (int)SessionHelper.GetUserId();

            // Bỏ qua validation cho các trường không submit
            ModelState.Remove("TotalPets");
            ModelState.Remove("TotalAppointments");
            ModelState.Remove("TotalSpent");
            ModelState.Remove("LastVisit");
            ModelState.Remove("AvatarUrl");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var user = db.Users.Find(userId);

                if (user == null)
                {
                    TempData["Error"] = "Không tìm thấy thông tin người dùng";
                    return View(model);
                }

                // Kiểm tra email trùng
                if (user.Email != model.Email)
                {
                    bool emailExists = db.Users.Any(u => u.Email == model.Email && u.UserId != userId);
                    if (emailExists)
                    {
                        ModelState.AddModelError("Email", "Email này đã được sử dụng");
                        return View(model);
                    }
                }

                // Xử lý upload avatar
                if (model.AvatarFile != null && model.AvatarFile.ContentLength > 0)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var extension = Path.GetExtension(model.AvatarFile.FileName).ToLower();

                    if (!allowedExtensions.Contains(extension))
                    {
                        ModelState.AddModelError("AvatarFile", "Chỉ chấp nhận file ảnh (.jpg, .jpeg, .png, .gif)");
                        return View(model);
                    }

                    if (model.AvatarFile.ContentLength > 5 * 1024 * 1024) // 5MB
                    {
                        ModelState.AddModelError("AvatarFile", "Kích thước file không được vượt quá 5MB");
                        return View(model);
                    }

                    // Tạo tên file duy nhất
                    string fileName = $"avatar_{userId}_{DateTime.Now.Ticks}{extension}";
                    string uploadPath = Server.MapPath("~/Content/Images/Avatars");

                    // Tạo thư mục nếu chưa có
                    if (!Directory.Exists(uploadPath))
                    {
                        Directory.CreateDirectory(uploadPath);
                    }

                    string filePath = Path.Combine(uploadPath, fileName);

                    // Xóa avatar cũ nếu có
                    if (!string.IsNullOrEmpty(user.AvatarUrl) && user.AvatarUrl != "/Content/Images/default-avatar.png")
                    {
                        string oldFilePath = Server.MapPath(user.AvatarUrl);
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    // Lưu file mới
                    model.AvatarFile.SaveAs(filePath);
                    user.AvatarUrl = $"/Content/Images/Avatars/{fileName}";
                }

                // Cập nhật thông tin
                user.FullName = model.FullName;
                user.Email = model.Email;
                user.Phone = model.Phone;
                user.Address = model.Address;
                user.UpdatedAt = DateTime.Now;

                db.SaveChanges();

                // Cập nhật session
                SessionHelper.SetUserSession(user.UserId, user.FullName, user.Email, user.Role);

                TempData["Success"] = "Cập nhật thông tin thành công!";
                return RedirectToAction("Profile");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra: " + ex.Message;
                return View(model);
            }
        }

        // GET: Account/ChangePassword
        [HttpGet]
        public ActionResult ChangePassword()
        {
            if (!SessionHelper.IsLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }
            return View();
        }

        // POST: Account/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ChangePasswordViewModel model)
        {
            if (!SessionHelper.IsLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            int userId = (int)SessionHelper.GetUserId();

            try
            {
                var user = db.Users.Find(userId);

                if (user == null)
                {
                    TempData["Error"] = "Không tìm thấy thông tin người dùng";
                    return View(model);
                }

                // Kiểm tra mật khẩu hiện tại
                string currentPasswordHash = SessionHelper.HashPassword(model.CurrentPassword);
                if (user.PasswordHash != currentPasswordHash)
                {
                    ModelState.AddModelError("CurrentPassword", "Mật khẩu hiện tại không đúng");
                    return View(model);
                }

                // Cập nhật mật khẩu mới
                user.PasswordHash = SessionHelper.HashPassword(model.NewPassword);
                user.UpdatedAt = DateTime.Now;

                db.SaveChanges();

                TempData["Success"] = "Đổi mật khẩu thành công!";
                return RedirectToAction("Profile");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra: " + ex.Message;
                return View(model);
            }
        }

        // ============================================================

        // Helper: Redirect về trang chủ theo Role
        private ActionResult RedirectToHome()
        {
            string role = SessionHelper.GetRole();

            if (role == "Admin")
                return RedirectToAction("Index", "homeadmin", new { area = "Admin" });
            else if (role == "Staff")
                return RedirectToAction("Index", "Staff", new { area = "" });
            else
                return RedirectToAction("Index", "Home");
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