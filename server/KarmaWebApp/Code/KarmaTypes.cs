using KarmaDb.Types;
using KarmaWeb.Utilities;
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

    public class RequestStateUtil
    {
        public static string FromDBToJson(RequestState state)
        {
            throw new NotImplementedException();
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

    public class KarmaDate
    {
        public string ToDBDate()
        {
            throw new NotImplementedException();
        }
        public static KarmaDate FromDBDate(string dbDate)
        {
            throw new NotImplementedException();
        }

        public string ToJsonDate()
        {
            throw new NotImplementedException();
        }
        public static KarmaDate FromJsonDate(string jsonDate)
        {
            throw new NotImplementedException();
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
        public string requestId {get;private set;}
        public string title { get; private set; }
        public Location location { get; private set; }

        public KarmaDate dueDate { get; private set; }
        public RequestState state { get; private set; }

        public KarmaUser from { get; private set; }
        public List<KarmaUser> delieverTo { get; private set; }
        public List<KarmaUser> delieveredTo { get; private set; }
        public List<KarmaUser> offeredBy { get; private set; }
        public List<KarmaUser> ignoredBy { get; private set; }
        public List<KarmaUser> acecptedFrom { get; private set; }
        public List<KarmaUser> ignoredFrom { get; private set; }

        public KarmaRequest(string requestId)
        {
            this.requestId = requestId;
            this.delieverTo = new List<KarmaUser>();
            this.delieveredTo = new List<KarmaUser>();
            this.offeredBy = new List<KarmaUser>();
            this.ignoredBy = new List<KarmaUser>();
            this.acecptedFrom = new List<KarmaUser>();
            this.ignoredFrom = new List<KarmaUser>();
            this.location = new Location();
        }

        internal static KarmaRequest FromDB(DbRequest request, Graph graph)
        {
            var graphRequest = new KarmaRequest(request.requestId);
            graphRequest.title = request.title;
            graphRequest.dueDate = KarmaDate.FromDBDate(request.dueDate);
            graphRequest.location = LocationUtil.FromDbLocation(request.lat, request.lang, request.location, request.locFlags);
            graphRequest.state = RequestFlagsToStatus(request.flags);

            KarmaUser graphUser;
            if (graph.Users.TryGetValue(request.createdBy, out graphUser))
            {
                graphRequest.from = graphUser;
            }

            graph.FillListWithUsers(graphRequest.delieverTo, ListUtils.ListFromCSV(request.delieverTo));
            graph.FillListWithUsers(graphRequest.delieveredTo, ListUtils.ListFromCSV(request.delieveredTo));
            graph.FillListWithUsers(graphRequest.acecptedFrom, ListUtils.ListFromCSV(request.offerAccepted));
            graph.FillListWithUsers(graphRequest.ignoredFrom, ListUtils.ListFromCSV(request.offersIgnored));
            graph.FillListWithUsers(graphRequest.ignoredBy, ListUtils.ListFromCSV(request.ignoredBy));

            return graphRequest;
        }

        private static RequestState RequestFlagsToStatus(EDBRequestFlags eDBRequestFlags)
        {
            throw new NotImplementedException();
        }

    }


    public class KarmaUser
    {
        public string id { get; private set; }
        public string name { get; private set; }             // name
        public EGender gender { get; private set; }           // gender
        public string pic { get; private set; }       // picture url

        public string email { get; private set; }            // email

        public Location location = new Location();
        public KarmaPoints points = new KarmaPoints();
        

        public List<KarmaUser> friends { get; private set; }
        public List<KarmaUser> blockedFriends { get; private set; }
        public List<KarmaRequest> inbox { get; private set; }
        public List<KarmaRequest> outbox { get; private set; }
        public List<KarmaGroup> memberofGroups { get; private set; }

        public KarmaUser(string id)
        {
            this.id = id;
            this.friends = new List<KarmaUser>();
            this.blockedFriends = new List<KarmaUser>();
            this.inbox = new List<KarmaRequest>();
            this.outbox = new List<KarmaRequest>();
            this.memberofGroups = new List<KarmaGroup>();
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

        internal bool HasBlocked(KarmaUser user)
        {
            return this.blockedFriends.Contains(user);
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
