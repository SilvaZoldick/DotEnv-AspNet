using System;
using System.Web;
using System.Web.Optimization;
using System.Web.Routing;
using ConfigController.App_Start;

namespace ConfigController
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            // Code that runs on application startup
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            EnvControl.CriarEnvs("App_Data.Model");
        }
    }
}