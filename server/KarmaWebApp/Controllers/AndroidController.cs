using Facebook;
using KarmaBackEnd;
using KarmaWebApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KarmaWebApp.Controllers
{
    public class AndroidController : KarmaBaseController
    {
        //
        // GET: /Android/
        public ActionResult Index(string accessToken)
        {
            // return fake results if no access token. 
            if (String.IsNullOrEmpty(accessToken))
            {
                var model = new MobileSessionModel();

                model.name = "Fake";
                model.pictureUrl = "https://fbcdn-profile-a.akamaihd.net/hprofile-ak-ash2/274191_628825055_1428018580_q.jpg";
                model.facebookId = "628825055";
                model.location = "Redmond, WA";
                return View(model);
            }

            try 
            {
                var backEnd = new KarmaBackend();
                var logonUser = backEnd.LogonUserUsingFB(accessToken);

                // save user context for subsequent calls.
                // this will be used by all api calls.
                var karmaSessionContext = GetKarmaSessionContext();
                karmaSessionContext.ActiveUser = logonUser;

                var model = new MobileSessionModel();
                model.name = logonUser.FirstName;
                model.pictureUrl = logonUser.PictureUrl;
                model.facebookId = logonUser.FacebookId;
                model.location = logonUser.Location;

                return View(model);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                // if something goes wrong with the access token
                // go back to the main facebook logon page.
                return RedirectToAction("Index", "Home");
            }
        }

	}
}