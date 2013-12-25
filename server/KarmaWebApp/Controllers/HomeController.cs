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
	}
}