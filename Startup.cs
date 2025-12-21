using Owin;
using Microsoft.Owin;
using Hangfire;

[assembly: OwinStartup(typeof(_125_BCCK.Startup))]

namespace _125_BCCK
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Chỉ cần khởi động Server và Dashboard
            // (Configuration đã làm trong Global.asax.cs rồi)
            app.UseHangfireServer();
            app.UseHangfireDashboard();
        }
    }
}