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
        
        public void SetDatabase(CloudTable peopleTable)
        {
            this._peopleTable = peopleTable;
        }
        
        public void SetLogger(ILogger logger)
        {
            this._logger = logger;
        }

        public bool ReadAll(
            out List<DbUserBasic> userBasic,
            out List<DbUserExtended> userExtended,
            out List<DbGroup> groups,
            out List<DbRequest> requests
            );

        public List<DbUserBasic> ReadAllPeopleBasic();
        public List<DbUserExtended> ReadAllPeopleExtended();
        public List<DbGroup> ReadAllGroups();
        public List<DbRequest> ReadAllRequests();
    }
}
