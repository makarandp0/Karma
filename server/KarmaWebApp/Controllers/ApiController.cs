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
            public bool error { get; private set; }
            public string errorcode { get; private set; }
            public JsonData()
            {
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

        public class JsonFriend
        {
            public string name;
            public string id;
            public int karmapoints;
            public JsonFriend(IKarmaFriend karmaFriend)
            {
                this.name = karmaFriend.Name;
                this.id = karmaFriend.FacebookId;
                this.karmapoints = karmaFriend.karmaPoints;
            }

        }

        public class JsonFriendsData : JsonData
        {
            public List<JsonFriend> friends = new List<JsonFriend>();
        }
        //
        // GET: /Api/getFriends
        public JsonResult getFriends(string client)
        {
            var data = new JsonFriendsData();

            var sessionContext = GetKarmaSessionContext();
            if (sessionContext.ActiveUser != null)
            {
                var dbFriends =  sessionContext.ActiveUser.Friends();
                foreach(var friend in dbFriends)
                {
                    data.friends.Add(new JsonFriend(friend));
                }
            }
            else
            {
                data.seterror("usersession inactive");
            }

            return Json(data, JsonRequestBehavior.AllowGet);
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
            var karmaRequest = sessionContext.ActiveUser.CreateRequest(Title, "no message", DateTime.MaxValue);
            data.id = karmaRequest.Id;
            return Json(data, JsonRequestBehavior.AllowGet);
        }
	}
}