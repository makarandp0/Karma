using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;
using System.Web.Http;
using System.Web.Optimization;
using KarmaWebApp.Models;
using System.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace KarmaWebApp
{

    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            // Code that runs on application startup
            AreaRegistration.RegisterAllAreas();
            //GlobalConfiguration.Configure(WebApiConfig.Register);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            ReadAppConfiguration();
            KarmaBackEnd.LoadDatabase();
        }

        public void Session_OnStart()
        {
            Application.Lock();
            // Application["UsersOnline"] = (int)Application["UsersOnline"] + 1;
            // Session["karmasessioncontext"] = new KarmaSessionContext();
            Application.UnLock();
        }

        public void Session_OnEnd()
        {
            Application.Lock();
            // Application["UsersOnline"] = (int)Application["UsersOnline"] - 1;
            // Session["karmasessioncontext"] = null;
            Application.UnLock();
        }

        void ReadAppConfiguration()
        {
            var configToUse = ConfigurationManager.AppSettings["UseConfig"];
            
            var appKeyName = "FacebookAppKey-" + configToUse;
            var appkeyValue = ConfigurationManager.AppSettings[appKeyName];
            if (string.IsNullOrEmpty(appkeyValue))
            {
                Console.WriteLine("Key:{0} not found" + appKeyName);
            }
            else
            {
                MainModel.SetAppKey(appkeyValue);
                Console.WriteLine("customsetting1 application string = \"{0}\"",
                    appkeyValue);
            }
        }
    }
}