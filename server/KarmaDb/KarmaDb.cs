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
        private CloudTable _peopleTable;
        private ILogger _logger = new NullLogger();

        public void SetTable(CloudTable peopleTable)
        {
            this._peopleTable = peopleTable;
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
            return true;
        }

        public DbUserBasic ReadUserBasic(string facebookId)
        {
            return DbUserBasic.ReadFromDatabase(this._peopleTable, facebookId);
        }

        public DbRequest ReadRequest(string requestId)
        {
            return DbRequest.ReadFromDatabase(this._peopleTable, requestId);
        }


        //
        // replaces existing entity with the new one.
        //
        public TableResult InsertOrReplace(DbEntry dbEntry)
        {
            return dbEntry.InsertOrReplace(this._peopleTable);
        }

    }
}
