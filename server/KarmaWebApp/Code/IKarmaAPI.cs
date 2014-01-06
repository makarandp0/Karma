using KarmaGraph.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KarmaWebApp.Code.API
{
    public class KarmaSessionContext
    {
        public KarmaSessionContext()
        {
            ActiveUser = null;
        }

        public IActiveUser ActiveUser {get;set;}
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

        public JsonData(string error)
        {
            this.type = this.GetType().ToString();
            this.error = true;
            this.errorcode = error;
        }
        public void seterror(string errcode)
        {
            this.errorcode = errcode;
            this.error = true;
        }
    }

    public class JsonUser
    {
        public string id { get; set; }
        public string name { get; set; }
        public string pic { get; set; }
        public bool ismale { get; set; }
    }
    public class JsonFriend : JsonUser
    {
        public bool blocked { get; set; }
    }

    public class JsonrequestEntry
    {
        public string id { get; set; }
        public string date { get; set; }
        public string status { get; set; }
        public string location { get; set; }
        public string title { get; set; }
    }

    public class JsonInboxEntry : JsonrequestEntry
    {
        public string response { get; set; } // response of the curernt user: none, yes, no
        public string from { get; set; }     // id of the friend who created this request.
    }

    public class JsonhelpOfferEntry
    {
        public string id { get; set; } // requestid_responderid;
        public string response { get; set; } // response from the user "none" "yes", "no"
        public string from { get; set; } // responderid.
    }
    public class JsonOutboxEntry : JsonrequestEntry
    {
        public List<JsonhelpOfferEntry> helpOffers = new List<JsonhelpOfferEntry>();
    }

    public class JsonGetAllResponse : JsonData
    {
        public JsonUser me { get; set; }
        public List<JsonFriend> friends { get; set; }
        public List<JsonInboxEntry> inbox { get; set; }
        public List<JsonOutboxEntry> outbox { get; set; }
    }

    public class JsonCreateRequestResponse : JsonData
    {
        public JsonOutboxEntry request { get; set; }
    }


    /// <summary>
    /// this class encapsulates the services offered by Web Backend.
    /// This can be used by all clients (mobile,web) of this service. 
    /// </summary>
    public interface IKarmaWebBackEnd
    {
        /// <summary>
        ///  to access services specific to a user clients must create a KarmaWebUser Object
        ///  this object is created by providing fb accessToken.
        /// </summary>
        /// <param name="accessToken">Token obtained from facebook</param>
        /// <param name="clientid">Unique string identifying client</param>
        /// <returns></returns>
        IActiveUser LogonUserUsingFB(string accessToken);
    }

    /// <summary>
    ///  reprensets a karma user. A specialized version KaramActiveUser is derived from this.
    /// </summary>

    /// <summary>
    /// represents You - a logged on user.
    /// </summary>
    public interface IActiveUser
    {
        JsonGetAllResponse GetAll();

        JsonCreateRequestResponse CreateRequest(string title, KarmaDate dateTime, Location loc);

        JsonData OfferHelp(string requestId, bool offered);

        JsonData AcceptHelp(string requestId, bool accpeted);
    }

}