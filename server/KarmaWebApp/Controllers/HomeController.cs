using Facebook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using KarmaWebApp.Models;
namespace KarmaWebApp.Controllers
{
    public class HomeController : KarmaBaseController
    {
        //
        // GET: /Home/
        public ActionResult Index()
        {
            return View("landing", new MainModel());
        }

        // GET: /Home/Mock
        public ActionResult Mock()
        {
            return File(Server.MapPath("~/Views/Home/") + "mock.html", "text/html");
        }

        // Home/me
        public ActionResult me(string accessToken)
        {
            // return fake results if no access token. 
            var model = new MobileSessionModel();

            try
            {
                model.useMock = String.IsNullOrEmpty(accessToken);
                return View("user", model);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                // if something goes wrong with the access token
                // go back to the main facebook logon page.
                return RedirectToAction("Index", "Home");
            }
        }
	}
}