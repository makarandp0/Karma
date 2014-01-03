using KarmaBackEnd;
using KarmaWebApp.Code;
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
        [Serializable]
        public class JsonTestData
        {
            public string stringValueUninitialized;
            public string stringValueOne;
            public bool   boolValue;
            public List<string> ListOfStrings_Uninitialized;
            public List<string> ListOfStrings_Empty = new List<string>();
            public List<string> ListOfStrings_SomeEntries = new List<string>();
            public string[] arrayofStrings_UnInitialized;
            public string[] arrayofStrings_TwoEmptyElements;
            public string[] arrayofStrings_TwoRealElements;
            public int intValue = 5;

            public JsonTestData()
            {
                stringValueOne = "one";
                boolValue = false;
                ListOfStrings_SomeEntries.Add("one");
                ListOfStrings_SomeEntries.Add("two");

                arrayofStrings_TwoEmptyElements = new string[2];
                arrayofStrings_TwoRealElements = new string[2];
                arrayofStrings_TwoRealElements[0] = "one";
                arrayofStrings_TwoRealElements[0] = "two";
            }

        }

        public class JsonData
        {
            public string type { get; private set; }
            public bool error { get; private set; }
            public string errorcode { get; private set; }
            public JsonData()
            {
                this.type = this.GetType().ToString();
                this.error = false;
                this.errorcode = "";
            }
            public void seterror(string errcode)
            {
                this.errorcode = errcode;
                this.error = true;
            }
        }

        public class JsonCreateRequestResponse : JsonData
        {
            public string id;
        }

        public class JsonUser
        {
            public string id;
            public string name;
            public string pic;
            public bool   ismale;
        }
        public class JsonFriend2 : JsonUser
        {
            public bool blocked;
        }

        public class JsonrequestEntry
        {
            public string id;       // id should be in the format cretorid_datestamp.
            public string date;     // date 
            public string status;   // status - open/closed/...
            public string location; // request location
            public string title;    // request title
        }

        public class JsonInboxEntry : JsonrequestEntry
        {
            public string response; // response of the curernt user: noresponse, yes, no
            public string from;     // id of the friend who created this request.
        }

        public class JsonhelpOfferEntry
        {
            public string id; // requestid_responderid;
            public string response; // response from the user "noresponse" "yes", "no"
            public string from; // responderid.
        }
        public class JsonOutboxEntry : JsonrequestEntry
        {
            public List<JsonhelpOfferEntry> helpOffers;
        }

        public class JsonGetAllResponse : JsonData
        {
            public JsonUser me;
            public List<JsonFriend2> friends;
            public List<JsonInboxEntry> inbox;
            public JsonOutboxEntry outbox;
        }

        // creates a new request and returns its id.
        public JsonResult createRequest(string Title)
        {
            var data = new JsonCreateRequestResponse();
            if (string.IsNullOrWhiteSpace(Title))
            {
                data.seterror("bad title");
                return Json(data, JsonRequestBehavior.AllowGet);
            }

            var sessionContext = GetKarmaSessionContext();
            if (sessionContext.ActiveUser == null)
            {
                data.seterror("usersession inactive");
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            data.id = sessionContext.ActiveUser.CreateRequest(Title, "no message", DateTime.MaxValue);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        // GET: /Api/getAll/?accessToeken=<>
        public JsonResult getAll(string accessToken)
        {
            var data = new JsonGetAllResponse();
            return Json(data, JsonRequestBehavior.AllowGet);

        }
	}
}