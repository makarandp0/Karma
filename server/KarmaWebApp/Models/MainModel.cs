using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KarmaWebApp.Models
{
    public class MainModel
    {
        public static string rootname {get; private set;}
        public static string AppKey { get; private set; }

        public static void SetAppKey(string appKey)
        {
            AppKey = appKey;
        }
    }

    public class MobileSessionModel : MainModel
    {
        public bool IsNeweUser {get;set;}
        public string pictureUrl { get; set; }
        public string facebookId { get; set; }
        public string name { get; set; }
        public string location { get; set; }
    }

}