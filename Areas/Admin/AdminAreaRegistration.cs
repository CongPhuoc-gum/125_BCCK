using System.Web.Mvc;

namespace _125_BCCK.Areas.Admin
{
    public class AdminAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Admin";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            // Route cho ServiceManagement
            context.MapRoute(
                name: "Admin_ServiceManagement",
                url: "Admin/services/{action}/{id}",
                defaults: new { controller = "ServiceManagement", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "_125_BCCK.Areas.Admin.Controllers" }
            );

            // Route cho AppointmentManagement
            context.MapRoute(
                name: "Admin_AppointmentManagement",
                url: "Admin/appointments/{action}/{id}",
                defaults: new { controller = "AppointmentManagement", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "_125_BCCK.Areas.Admin.Controllers" }
            );

            // Route mặc định cho Admin area
            context.MapRoute(
                name: "Admin_default",
                url: "Admin/{controller}/{action}/{id}",
                defaults: new { action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "_125_BCCK.Areas.Admin.Controllers" }
            );
        }
    }
}