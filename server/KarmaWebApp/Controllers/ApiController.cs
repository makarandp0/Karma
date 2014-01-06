using KarmaGraph.Types;
using KarmaWebApp.Code;
using KarmaWebApp.Code.API;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KarmaWebApp.Controllers
{
    public class ApiController : KarmaBaseController
    {
        // creates a new request and returns its id.
        public JsonResult createRequest(string title, string date, string location, double? lat, double? lan)
        {
            try
            {
                var sessionContext = GetKarmaSessionContext();
                if (sessionContext.ActiveUser == null)
                {
                    return JsonError("usersession inactive");
                }

                // validate parameters
                if (string.IsNullOrWhiteSpace(title))
                {
                    return JsonError("bad title");
                }

                var dateTime = KarmaDate.FromJsonDate(date);
                if (dateTime == null)
                {
                    return JsonError("invlid date");
                }

                Location loc = new Location();
                if (!string.IsNullOrEmpty(location))
                {
                    loc.nameIsValid = true;
                    loc.name = location;
                }
                if (lat.HasValue && lan.HasValue)
                {
                    loc.latlanIsValid = true;
                    loc.lat = lat.Value;
                    loc.lan = lan.Value;
                }
                if (!loc.latlanIsValid && !loc.nameIsValid)
                {
                    return JsonError("must specify location or  lat, lan ");
                }

                var createRequestResponse = sessionContext.ActiveUser.CreateRequest(title, dateTime, loc);
                return Json(createRequestResponse, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return JsonError("Exception in createRequest:" + ex);
            }
        }

        // GET: /Api/getAll/?accessToeken=<>
        public JsonResult getAll(string accessToken)
        {
            try
            {
                if (string.IsNullOrEmpty(accessToken))
                {
                    return JsonError("bad access token");
                }

                var activeUser = KarmaBackEnd.LogonUserUsingFB(accessToken);
                if (activeUser == null)
                {
                    return JsonError("failed to logon user using accesstoken");
                }

                var sessionContext = GetKarmaSessionContext();
                sessionContext.ActiveUser = activeUser;

                var getAllResponse = activeUser.GetAll();
                if (getAllResponse == null)
                {
                    return JsonError("activeUser.GetAll failed");
                }

                return Json(getAllResponse, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return JsonError("Exception in getAll:" + ex);
            }
        }

        // get /Api/offerHelp/?requestid=xyz&offer=yes[no]
        public JsonResult offerhelp(string requestId, string offer)
        {
            try
            {
                bool offered = false; // is offer yes= true, no = false;
                var sessionContext = GetKarmaSessionContext();
                if (sessionContext.ActiveUser == null)
                {
                    return JsonError("usersession inactive");
                }

                // validate parameters
                if (string.IsNullOrWhiteSpace(requestId))
                {
                    return JsonError("bad requestId");
                }

                if (string.Compare(offer, "yes", true) == 0)
                {
                    offered = true;
                }
                else if (string.Compare(offer, "no", true) == 0)
                {
                    offered = false;
                }
                else
                {
                    return JsonError("bad offer:" + offer);
                }

                var result = sessionContext.ActiveUser.OfferHelp(requestId, offered);
                return Json(result, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return JsonError("Exception in offerhelp:" + ex);
            }

        }

        // get /Api/accepthelp/?offerid=xyz&accept=yes[no]
        public JsonResult accepthelp(string offerId, string accept)
        {
            try
            {
                bool accpeted = false; // is yes=true, no=false;
                var sessionContext = GetKarmaSessionContext();
                if (sessionContext.ActiveUser == null)
                {
                    return JsonError("usersession inactive");
                }

                // validate parameters
                if (string.IsNullOrWhiteSpace(offerId))
                {
                    return JsonError("bad requestId");
                }

                if (string.Compare(accept, "yes", true) == 0)
                {
                    accpeted = true;
                }
                else if (string.Compare(accept, "no", true) == 0)
                {
                    accpeted = false;
                }
                else
                {
                    return JsonError("bad accept:" + accept);
                }

                var result = sessionContext.ActiveUser.AcceptHelp(offerId, accpeted);
                return Json(result, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return JsonError("Exception in accepthelp:" + ex);
            }

        }

        private JsonResult JsonError(string error)
        {
            return Json(new JsonData(error), JsonRequestBehavior.AllowGet);
        }
	}
}