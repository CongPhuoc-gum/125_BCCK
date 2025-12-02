using _125_BCCK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace _125_BCCK.Controllers
{
    public class ServicesController : Controller
    {
        private PetCareContext db = new PetCareContext();

        // GET: Services/Index
        public ActionResult Index(string category = null)
        {
            try
            {
                var query = db.Services.Where(s => s.IsActive);

                if (!string.IsNullOrEmpty(category))
                {
                    query = query.Where(s => s.Category == category);
                }

                var services = query
                    .OrderBy(s => s.Category)
                    .ThenBy(s => s.ServiceName)
                    .ToList();

                ViewBag.Categories = db.Services
                    .Where(s => s.IsActive)
                    .Select(s => s.Category)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToList();

                ViewBag.SelectedCategory = category;

                return View(services);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi: " + ex.Message;
                return View(new List<Service>());
            }
        }

        // GET: Services/Details/5
        public ActionResult Details(int? id)
        {
            // Kiểm tra id
            if (!id.HasValue)
            {
                TempData["Error"] = "Không tìm thấy dịch vụ";
                return RedirectToAction("Index");
            }

            try
            {
                var service = db.Services
                    .FirstOrDefault(s => s.ServiceId == id.Value && s.IsActive);

                if (service == null)
                {
                    TempData["Error"] = "Không tìm thấy dịch vụ";
                    return RedirectToAction("Index");
                }

                // Lấy 3 dịch vụ CÙNG DANH MỤC (không bao gồm dịch vụ hiện tại)
                ViewBag.RelatedServices = db.Services
                    .Where(s => s.IsActive
                             && s.Category == service.Category
                             && s.ServiceId != service.ServiceId)
                    .OrderBy(s => s.Price)
                    .Take(3)
                    .ToList();

                // Lấy 3 dịch vụ từ DANH MỤC KHÁC (gợi ý đa dạng)
                // Lấy tất cả trước, sau đó random trong memory để tránh lỗi với Entity Framework
                var otherServicesList = db.Services
                    .Where(s => s.IsActive && s.Category != service.Category)
                    .ToList();

                ViewBag.OtherServices = otherServicesList
                    .OrderBy(s => Guid.NewGuid())
                    .Take(3)
                    .ToList();

                return View(service);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi: " + ex.Message;
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