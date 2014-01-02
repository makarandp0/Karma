using KarmaDb.Types;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KarmaGraph.Types
{
    /// <summary>
    /// contains in memeory representation of Karma object.
    /// </summary>
    public partial class GraphRoot
    {
        public Dictionary<string, KarmaUser> Users = new Dictionary<string,KarmaUser>();
        public Dictionary<string, KarmaGroup> Groups = new Dictionary<string,KarmaGroup>();
        public Dictionary<string, KarmaRequest> Requests = new Dictionary<string,KarmaRequest>();
    }

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
            loc.lat = (decimal)lat;
            loc.lan = (decimal)lan;
            loc.name = location;
            loc.nameIsValid = ((flags & EDBLocationFlags.Location_NameIsValid) != 0);
            loc.latlanIsValid = ((flags & EDBLocationFlags.Location_LatLanIsValid) != 0);
            return loc;
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
        public bool nameIsValid;
        public bool latlanIsValid;
        public string name;
        public Decimal lat;
        public Decimal lan;
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

        public KarmaUser       from;
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
        public string id;               // userid
        public string name;             // name
        public EGender gender;           // gender
        public string pic;       // picture url
        public Location location;       // user location.
        public KarmaPoints points;      // karma points.
        public string email;            // email

        public List<KarmaUser>     friends;
        public List<KarmaUser> blockedFriends;
        public List<KarmaRequest> inbox;
        public List<KarmaRequest> outbox;
        public List<KarmaGroup> memberofGroups;

        public KarmaUser(string id)
        {
            this.id = id;
        }
        
        public static KarmaUser FromDbRequest(DbUserBasic userBasic)
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
