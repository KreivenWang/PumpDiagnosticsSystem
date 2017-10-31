using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(PumpDiagnosticsSystem.Web.Startup))]
namespace PumpDiagnosticsSystem.Web
{
    public partial class Startup
    {
        
        public void Configuration(IAppBuilder app)
        {
            //            iapplicationuser
            //            app.UseStaticFiles(new StaticFileOptions() {
            //                FileProvider = new PhysicalFileProvider(@"D:\Source\WebApplication1\src\WebApplication1\MyStaticFiles"),
            //                RequestPath = new PathString("/StaticFiles")
            //            });
        }
    }
}
