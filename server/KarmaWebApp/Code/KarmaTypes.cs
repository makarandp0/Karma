using KarmaDb.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KarmaGraph.Types
{
    /// <summary>
    /// contains in memeory representation of Karma object.
    /// </summary>

    public enum EGender { Male, Female, Unknown }

    public class GenderUtil
    {
        public static EGender FromDbGender(string dbGender)
        {
            if (dbGender == DbConstants.DBGENDER_MALE)
                return EGender.Male;

            if (dbGender == DbConstants.DBGENER_FEMALE)
                return EGender.Female;

            return EGender.Unknown;
        }

        public static string ToDbGender(EGender gender)
        {
            if (gender == EGender.Male)
                return DbConstants.DBGENDER_MALE;
            else if (gender == EGender.Female)
                return DbConstants.DBGENER_FEMALE;

            return DbConstants.DBGENDER_UNKNOWN;
        }
    }

    public class LocationUtil
    {
        internal static Location FromDbLocation(double lat, double lan, string location, EDBLocationFlags flags)
        {
            var loc = new Location();
            loc.lat = lat;
            loc.lan = lan;
            loc.name = location;
            loc.nameIsValid = ((flags & EDBLocationFlags.Location_NameIsValid) != 0);
            loc.latlanIsValid = ((flags & EDBLocationFlags.Location_LatLanIsValid) != 0);
            return loc;
        }

        internal static void ToDbLocation(Location location, DbUserBasic userBasic)
        {
            if (location.nameIsValid)
            {
               userBasic.location = location.name;
               userBasic.locFlags |= EDBLocationFlags.Location_NameIsValid;
            }
            else
            {
                userBasic.location = "";
                userBasic.locFlags &= ~EDBLocationFlags.Location_NameIsValid;
            }
            
            if (location.latlanIsValid)
            {
                userBasic.lat = location.lat;
                userBasic.lang = location.lan;
                userBasic.locFlags |= EDBLocationFlags.Location_LatLanIsValid;
            }
            else
            {
                userBasic.locFlags &= ~EDBLocationFlags.Location_LatLanIsValid;
            }
        }
    }


    public class KarmaPoints
    {
        public int Requested;
        public int Offered;
        public int Helped;

        static public KarmaPoints FromDbPoints(string dbPoints)
        {
            // TODO: IMPLEMENT.
            return new KarmaPoints();
        }
    }

    public class Location
    {
        public Location()
        {
            this.nameIsValid = false;
            this.latlanIsValid = false;
        }

        public bool nameIsValid;
        public bool latlanIsValid;
        public string name;
        public double lat;
        public double lan;
    }

    public enum RequestState
    {
        created,
        opening,
        opened,
        intransitPatial,    // has been send to some friends, some friends remains
        intransitFull,       // has been sent to all friends. no friends remaining.
        offered,
        accepted,
        failed,
        closed
    }

    public class KarmaRequest
    {
        public string requestId;
        public string title;
        public Location location;
        public RequestState state;

        public KarmaUser from;
        public List<KarmaUser> delieverTo;
        public List<KarmaUser> delieveredTo;
        public List<KarmaUser> offeredBy;
        public List<KarmaUser> acecptedFrom;
        public List<KarmaUser> ignoredFrom;

        public KarmaRequest(string requestId)
        {
            this.requestId = requestId;
        }

        internal static KarmaRequest FromDB(DbRequest request)
        {
            var graphRequest = new KarmaRequest(request.requestId);
            graphRequest.title = request.title;
            graphRequest.location = LocationUtil.FromDbLocation(request.lat, request.lang, request.location, request.locFlags);
            graphRequest.state = RequestFlagsToStatus(request.flags);
            return graphRequest;
        }

        private static RequestState RequestFlagsToStatus(EDBRequestFlags eDBRequestFlags)
        {
            throw new NotImplementedException();
        }
    }


    public class KarmaUser
    {
        public string id {get;set;}
        public string name {get;set;}             // name
        public EGender gender {get;set;}           // gender
        public string pic {get;set;}       // picture url

        public string email { get; set; }            // email

        public Location location = new Location();
        public KarmaPoints points = new KarmaPoints();
        

        public List<KarmaUser> friends = new List<KarmaUser>();
        public List<KarmaUser> blockedFriends = new List<KarmaUser>();
        public List<KarmaRequest> inbox = new List<KarmaRequest>();
        public List<KarmaRequest> outbox = new List<KarmaRequest>();
        public List<KarmaGroup> memberofGroups = new List<KarmaGroup> ();

        public KarmaUser(string id)
        {
            this.id = id;
        }

        public static KarmaUser FromDbUser(DbUserBasic userBasic)
        {
            var user = new KarmaUser(userBasic.fbId);
            user.name = userBasic.name;
            user.gender = GenderUtil.FromDbGender(userBasic.gender);
            user.pic = userBasic.pic;
            user.location = LocationUtil.FromDbLocation(userBasic.lat, userBasic.lang, userBasic.location, userBasic.locFlags);
            user.email = userBasic.email;
            user.points = KarmaPoints.FromDbPoints(userBasic.karmapoints);
            return user;
        }
    }

    public class KarmaGroup
    {
        public string fbId;
        public string name;
        public List<KarmaUser> members;

        public KarmaGroup(string fbId)
        {
            this.fbId = fbId;
        }

        internal static KarmaGroup FromDbGroup(DbGroup dbGroup)
        {
            var graphgroup = new KarmaGroup(dbGroup.fbId);
            graphgroup.name = dbGroup.name;
            return graphgroup;
        }
    }
}
