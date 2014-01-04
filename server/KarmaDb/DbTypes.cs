using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.Globalization;

namespace KarmaDb.Types
{
    public class DbConstants
    {
        public const string DBGENDER_MALE = "male";
        public const string DBGENER_FEMALE = "female";
        public const string DBGENDER_UNKNOWN = "unknown";
    }

    [Flags]
    public enum EDBLocationFlags
    {
        Location_LatLanIsValid = 0x1,
        Location_NameIsValid = 0x2
    }

    [Flags]
    public enum EDBUserFlags
    {
    }

    [Flags]
    public enum EDBRequestFlags
    {
    }


    public enum EDBGroupFlags
    {
    }

    /// <summary>
    /// represents a karam entity in database.
    /// each entity has special "entityType" field
    /// which tells us which type is it.
    /// this is helpful in resolving the entity to its approprate class.
    /// this class ideally should be abstract. However I found that
    /// some of the AzureStorage methods dont like it to be abstract
    /// so to make them happy I am not making it abstract, but throwing an
    /// asserting in constructor that its abstract.
    /// </summary>
    public class DbEntry : TableEntity
    {
        public virtual string EntityType { get; set; }

        public DbEntry()
        {
            this.EntityType = this.GetType().Name;
            Debug.Assert(this.GetType() != typeof(DbEntry));
        }

        /// <summary>
        ///  this delegate implementation resolves the entity to appropriate derived type. 
        ///  add a new type here everytime you add a new type.
        ///  also for older types provide methods to upgrade.
        /// </summary>
        /// <param name="pk"></param>
        /// <param name="rk"></param>
        /// <param name="ts"></param>
        /// <param name="props"></param>
        /// <param name="etag"></param>
        /// <returns></returns>
        public static DbEntry Resolver(
            string pk,
            string rk,
            DateTimeOffset ts,
            IDictionary<string, EntityProperty> props,
            string etag)
        {
            DbEntry resolvedEntity = null;
            if (!props.ContainsKey("EntityType"))
            {
                // TODO: we do not understand this entity.
                // create a special list to take care of such.
                return null;
            }

            string entityType = props["EntityType"].StringValue;

            switch (entityType)
            {
                case "DbUserBasic":
                    resolvedEntity = new DbUserBasic(pk);
                    break;

                case "DbUserExtended":
                    resolvedEntity = new DbUserExtended(pk);
                    break;

                case "DbGroup":
                    resolvedEntity = new DbGroup(pk);
                    break;

                case "DbRequest": 
                    resolvedEntity = new DbRequest(pk, rk);
                    break;

                default:
                    Debug.Assert(false, "Unknown entityType:" + entityType);
                    return null;
            }

            resolvedEntity.PartitionKey = pk;
            resolvedEntity.RowKey = rk;
            resolvedEntity.Timestamp = ts;
            resolvedEntity.ETag = etag;
            resolvedEntity.ReadEntity(props, null);
            return resolvedEntity;
        }


        public TableResult InsertOrMerge(CloudTable table)
        {
            this.ETag = "*"; // this tells azure that overwrite the entry even if its modified before us.
            var insertOp = TableOperation.InsertOrMerge(this);
            return table.Execute(insertOp);
        }


        public static DbRequest Read(CloudTable table, string pk, string rk)
        {
            var retrieveOperation = TableOperation.Retrieve<DbRequest>(pk, rk);
            var retrievedResult = table.Execute(retrieveOperation);
            if (retrievedResult.Result != null)
            {
                return (DbRequest)retrievedResult.Result;
            }
            return null;
        }

    }

    /// <summary>
    /// this contains users basic information.
    /// something that we would may want to keep in memory.
    /// </summary>
    public class DbUserBasic : DbEntry
    {
        public const string ROW_ID = "user_basic";
        public DbUserBasic(string facebookId)
        {
            this.PartitionKey = facebookId;
            this.RowKey = ROW_ID;
        }
        [IgnoreProperty]
        public string fbId 
        { 
            get { return this.PartitionKey; }
        }                
        public string firstname { get; set; }           // first name
        public string name { get; set; }                // name
        public string gender { get; set; }              // gender
        public string pic { get; set; }          // picture url
        public string karmaFriends { get; set; }        // karma friends
        public string blockedFriends { get; set; }      // blocked friends.
        public string karmapoints { get; set; }         // karma points.
        public double lat { get; set; }                 // latitude
        public double lang { get; set; }                // langitude
        public string location { get; set; }            // location
        public EDBLocationFlags locFlags { get; set; }            
        public string email { get; set; }               // email
        public string groups { get; set; }              // groups user belongs to
        public EDBUserFlags userflags { get; set; }         // various flag values.
    }

    public class DbUserExtended : DbEntry
    {
        public const string ROW_ID = "user_extended";
        public DbUserExtended(string facebookId)
        {
            this.PartitionKey = facebookId;
            this.RowKey = ROW_ID;
        }

        [IgnoreProperty]
        public string fbId
        {
            get { return this.PartitionKey; }
        }                
        public string accessToken { get; set; }         // user access token
        public string nonkarmaFriends { get; set; }     // karma friends
    }

    public class DbRequest: DbEntry
    {
        const char SEPERATOR = '!';
        public DbRequest(string owner)
        {
            this.PartitionKey = owner;

            // generate a unique request id.
            this.RowKey = DateTime.UtcNow.ToString("s", CultureInfo.InvariantCulture); // someting like  2008-04-10T06:30:00

        }

        public static DbRequest ReadFromDatabase(CloudTable table, string requestId)
        {
            try
            {
                var keys = requestId.Split(SEPERATOR);
                if (keys.Length != 2) 
                {
                    Debug.Assert(false); // convert to dbgmsg.
                    return null;
                }

                var dbEntry = DbEntry.Read(table, keys[0], keys[1]);
                if (dbEntry != null && dbEntry.EntityType == typeof(DbRequest).ToString())
                {
                    return (DbRequest)dbEntry;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception retriving request:{0} from database:{1}", requestId, ex);
            }
            return null;
        }


        public DbRequest(string pk, string rk)
        {
            this.PartitionKey = pk;
            this.RowKey = rk;
        }
        [IgnoreProperty]
        public string requestId 
        {
            get { return this.PartitionKey + SEPERATOR + this.RowKey; }
        }


        [IgnoreProperty]
        public string createdBy
        {
            get { return this.PartitionKey; }
        }
        public string title { get; set; }               // title
        public string moreinfo { get; set; }            // more info
        public string dueDate { get; set; }             // due date
        public double lat { get; set; }                 // latitude
        public double lang { get; set; }                // langitude
        public string location { get; set; }            // location
        public EDBLocationFlags locFlags { get; set; }    // location flags        
        public EDBRequestFlags flags { get; set; }      // request flags (contains status)
        public string delieverTo { get; set; }          // whom should the request be delievered to
        public string delieveredTo { get; set; }          // whom should the request be delievered to
        public string offeredBy { get; set; }           // who has offered.
        public string ignoredBy { get; set; }           // people who responded "no" to request.
        public string offerAccepted { get; set; }       // whos offers have been accepted
        public string offersIgnored { get; set; }       // whos offers have been ignored.
        
    }

    public class DbGroup : DbEntry
    {
        public const string ROW_ID = "group_basic";
        public DbGroup(string facebookId)
        {
            this.PartitionKey = facebookId;
            this.RowKey = ROW_ID;
        }


        [IgnoreProperty]
        public string fbId
        {
            get { return this.PartitionKey; }
        }
        public string name { get; set; }

        public EDBGroupFlags flags { get; set; }                // group flags.
    }
}
