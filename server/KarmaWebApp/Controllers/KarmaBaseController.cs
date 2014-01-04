using KarmaWebApp.Code.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KarmaWebApp
{
    /// <summary>
    /// base controller for all karma controllers.
    /// </summary>
    public class KarmaBaseController : Controller
    {
        public KarmaSessionContext GetKarmaSessionContext()
        {
            var karmaSessionContext = (KarmaSessionContext)Session["karmasessioncontext"];
            if (karmaSessionContext == null)
            {
                karmaSessionContext = new KarmaSessionContext();
                Session["karmasessioncontext"] = karmaSessionContext;
            }
            return karmaSessionContext;
        }

    }
}