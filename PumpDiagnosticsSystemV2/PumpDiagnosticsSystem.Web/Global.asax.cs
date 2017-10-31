using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using PumpDiagnosticsSystem.Business;

namespace PumpDiagnosticsSystem.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            Debug.WriteLine("Application_Started.");
        }
    }

    public static class MsgRepo
    {
        public static List<string> GraphMsgs { get; } = new List<string>();
        public static string GraphMsg { get; set; }
    }
}
