using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace RedAlert.API
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            string configOverridePath = Server.MapPath("~/debugconfig.json");
            if (File.Exists(configOverridePath))
            {
                var fileContents = File.ReadAllText(configOverridePath);
                var overrideConfig = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(fileContents);
                foreach (var pair in overrideConfig)

                {
                    
                    if (ConfigurationManager.ConnectionStrings[pair.Key] != null)
                    {
                        //some extra magic is required here:
                        var setting = ConfigurationManager.ConnectionStrings[pair.Key];
                        var fi = typeof(ConfigurationElement).GetField("_bReadOnly", BindingFlags.Instance | BindingFlags.NonPublic);

                        fi.SetValue(setting, false);

                        setting.ConnectionString = pair.Value;
                    }

                    if (ConfigurationManager.AppSettings[pair.Key] != null)
                    {
                        ConfigurationManager.AppSettings[pair.Key] = pair.Value;
                    }
                }
            }


            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}
