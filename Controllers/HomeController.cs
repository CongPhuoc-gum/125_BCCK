using _125_BCCK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace _125_BCCK.Controllers
{
    public class HomeController : Controller
    {
        private PetCareContext db = new PetCareContext();
        // GET: Home/Index
        public ActionResult Index()
        {
            // Lấy 6 dịch vụ nổi bật
            var featuredServices = db.Services
                .Where(s => s.IsActive)
                .OrderByDescending(s => s.CreatedAt)
                .Take(6)
                .ToList();

            return View(featuredServices);
        }

        // GET: Home/About
        public ActionResult About()
        {
            return View();
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