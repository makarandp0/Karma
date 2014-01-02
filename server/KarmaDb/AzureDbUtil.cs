using KarmaDb.Types;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KarmaDb
{
    class AzureDbUtil
    {
        public async Task<List<DbEntry>> RunQuerySegmentedAsync(CloudTable table, TableQuery<DbEntry> query)
        {
            TableQuerySegment<DbEntry> querySegment = null;
            var returnList = new List<DbEntry>();
            while (querySegment == null || querySegment.ContinuationToken != null)
            {
                querySegment = await table.ExecuteQuerySegmentedAsync(
                    query,
                    new EntityResolver<DbEntry>(DbEntry.Resolver),
                    querySegment != null ? querySegment.ContinuationToken : null
                    );
                returnList.AddRange(querySegment);
            }
            return returnList;
        }

    }
}
