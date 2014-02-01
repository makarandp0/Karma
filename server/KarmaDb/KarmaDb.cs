using KarmaDb.Types;
using KarmaWeb.Utilities;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KarmaDb
{
    public class KarmaDb
    {
        private CloudTable _peopleTable { get; set; }
        private  CloudTable _requestTable { get; set; }
        
        
        private CloudTable _groupTable { get; set; }

        // extended user information
        private CloudTable _userExtended { get; set; }
        
        private ILogger _logger = new NullLogger();

        public void SetStorageAccount(Microsoft.WindowsAzure.Storage.CloudStorageAccount storageAccount)
        {
            var tableClient = storageAccount.CreateCloudTableClient();
            this._peopleTable = tableClient.GetTableReference("karmapeople");
            this._peopleTable.CreateIfNotExists();

            this._requestTable = tableClient.GetTableReference("karmarequests");
            this._requestTable.CreateIfNotExists();

            this._groupTable = tableClient.GetTableReference("karmagroups");
            this._groupTable.CreateIfNotExists();

            this._userExtended = tableClient.GetTableReference("karmauserExtended");
            this._userExtended.CreateIfNotExists();

        }

        
        public void SetLogger(ILogger logger)
        {
            this._logger = logger;
        }

        public bool ReadAll(
            out List<DbUserBasic> userBasic,
            out List<DbGroup> groups,
            out List<DbRequest> requests
            )
        {
            userBasic = new List<DbUserBasic>();
            groups = new List<DbGroup>();
            requests = new List<DbRequest>();

            // TO: we are doing very inefficient reading here.
            // but its very extendable. so for some time as long as we have
            // lots of flux, and small users this would work.

            // people.
            TableQuery query = (new TableQuery());
            var everything = this._peopleTable.ExecuteQuery(query, new EntityResolver<DbEntry>(DbEntry.Resolver), null, null);
            foreach(var thing in everything)
            {
                if (thing == null) continue; // for the entries that we do not understand.

                switch(thing.EntityType)
                {
                    case "DbUserBasic":
                        userBasic.Add((DbUserBasic)thing);
                        break;
                    case "DbRequest":
                        requests.Add((DbRequest)thing);
                        break;
                    case "DbGroup":
                        groups.Add((DbGroup) thing);
                        break;

                     case "DbUserExtended":
                        // we dont care for this right now.
                        break;;

                    default:
                        break;
                }
            }

            // requests
            everything = this._requestTable.ExecuteQuery(query, new EntityResolver<DbEntry>(DbEntry.Resolver), null, null);
            foreach (var thing in everything)
            {
                if (thing == null) continue; // for the entries that we do not understand.

                switch (thing.EntityType)
                {
                    case "DbUserBasic":
                        userBasic.Add((DbUserBasic)thing);
                        break;
                    case "DbRequest":
                        requests.Add((DbRequest)thing);
                        break;
                    case "DbGroup":
                        groups.Add((DbGroup)thing);
                        break;

                    case "DbUserExtended":
                        // we dont care for this right now.
                        break; ;

                    default:
                        break;
                }
            }

            // groups
            everything = this._groupTable.ExecuteQuery(query, new EntityResolver<DbEntry>(DbEntry.Resolver), null, null);
            foreach (var thing in everything)
            {
                if (thing == null) continue; // for the entries that we do not understand.

                switch (thing.EntityType)
                {
                    case "DbUserBasic":
                        userBasic.Add((DbUserBasic)thing);
                        break;
                    case "DbRequest":
                        requests.Add((DbRequest)thing);
                        break;
                    case "DbGroup":
                        groups.Add((DbGroup)thing);
                        break;

                    case "DbUserExtended":
                        // we dont care for this right now.
                        break; ;

                    default:
                        break;
                }
            }

            return true;
        }

        public DbUserBasic ReadUserBasic(string facebookId)
        {
            string pk, rk;
            if (DbUserBasic.GetPartitionRow(facebookId, out pk, out rk))
            {
                return Read<DbUserBasic>(pk, rk);
            }
            return null;
        }

        public DbRequest ReadRequest(string requestId)
        {
            string pk, rk;
            if (DbRequest.GetPartitionRow(requestId, out pk, out rk))
            {
                return Read<DbRequest>(pk, rk);
            }
            return null;
        }


        public TableResult InsertOrReplace(CloudTable table, TableEntity entity)
        {
            if (!ArekeysValid(entity))
            {
                throw new ArgumentException("row key or partition key contains invalid chars");
            }
            var insertOp = TableOperation.InsertOrReplace(entity);
            return table.Execute(insertOp);
        }

        /// <summary>
        /// this adds any new propperties, but does not delete any existing properties.
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public TableResult InsertOrMerge(CloudTable table, TableEntity entity)
        {
            if (!ArekeysValid(entity))
            {
                throw new ArgumentException("row key or partition key contains invalid chars");
            }
            entity.ETag = "*"; // this tells azure that overwrite the entry even if its modified before us.
            var insertOp = TableOperation.InsertOrMerge(entity);
            return table.Execute(insertOp);
        }


        public T Read<T>(string pk, string rk) where T : DbEntry
        {
            var table = GetEntryTable(typeof(T).Name);
            if (table != null)
            {
                var retrieveOperation = TableOperation.Retrieve<T>(pk, rk);
                var retrievedResult = table.Execute(retrieveOperation);
                if (retrievedResult.Result != null)
                {
                    var dbEntry = (DbEntry)retrievedResult.Result;
                    if (dbEntry != null && dbEntry.EntityType == typeof(T).Name)
                    {
                        return (T)dbEntry;
                    }
                }
            }
            return null;
        }

        // checks if row key and partition key are valid.
        bool ArekeysValid(TableEntity entity)
        {
            /*
             * following are illegal characters.
                The forward slash (/) character
                The backslash (\) character
                The number sign (#) character
                The question mark (?) character
                Control characters from U+0000 to U+001F, including:
                The horizontal tab (\t) character
                The linefeed (\n) character
                The carriage return (\r) character
                Control characters from U+007F to U+009F             * 
             */
            char[] illegalChars = {
                '/', '\\', '#', '?', '\t', '\n', '\r'
            };

            if (entity.RowKey.IndexOfAny(illegalChars) != -1)
                return false;

            if (entity.PartitionKey.IndexOfAny(illegalChars) != -1)
                return false;

            // there are other characters to consider, but for now dont worry.
            return true;
        }

        //
        // replaces existing entity with the new one.
        //
        public TableResult InsertOrReplace(DbEntry dbEntry)
        {
            // find which table should this entry go to.
            CloudTable table = GetEntryTable(dbEntry.EntityType);
            if (table != null)
            {
                return InsertOrReplace(table,dbEntry);
            }
            return null;
        }

        private CloudTable GetEntryTable(string EntityType)
        {
            switch (EntityType)
            {
                case "DbUserBasic":
                    return this._peopleTable;
                case "DbRequest":
                    return this._requestTable;
                case "DbGroup":
                    return this._groupTable;
                case "DbUserExtended":
                    return this._userExtended;
            }

            return null;
        }
        
    }
}
