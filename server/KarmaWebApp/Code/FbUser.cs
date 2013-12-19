using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

/*
namespace KarmaWebApp.Code
{
    public class FbUserBasicInfo
    {
        public string FacebookId { get; private set; }
        public string Name {get;private set;}

        public virtual bool IsKarmaUser() {return false;}

        public FbUserBasicInfo(string fbId, string name)
        {
            Debug.Assert(!String.IsNullOrEmpty(fbId));
            Debug.Assert(!String.IsNullOrEmpty(name));

            this.FacebookId = fbId;
            this.Name = name;
        }
    }

    public class FBUserExtendedInfo : FbUserBasicInfo
    {
        public string FirstName { get; set; }

        public string PictureUrl { get; set; }

        public string Location { get; set; }

        public FBUserExtendedInfo(string fbId, string name)
            : base(fbId, name)
        {
        }

    }

    public class KarmaUser : FBUserExtendedInfo
    {
        public int KarmaPoints { get; set; }

        public override bool IsKarmaUser() {return true;}

        public KarmaUser(string fbId, string name) : base(fbId, name)
        {
        }
    }
}
*/