using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sepialabs.Azure;
using Microsoft.WindowsAzure;
using System.Data.Services.Client;
using KarmaWeb.Utilities;
using System.Configuration;
using Sepialabs.Azure.Test;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Table.DataServices;
using System.Threading.Tasks;
using KarmaDb.Utils;

namespace UnitTestProject1
{
    public class TestTable
    {
        const string TableName = "testtable";
        public CloudTable Table;

        public void CreateFresh()
        {
            var settingsReader = new AppSettingsReader();
            var storageAccount = CloudStorageAccount.Parse((string)settingsReader.GetValue("AzureStorage", typeof(string)));
            var tableClient = storageAccount.CreateCloudTableClient();
            Table = tableClient.GetTableReference(TableName);
            Table.DeleteIfExists();
            Table.CreateIfNotExists();
        }

        public List<TestEntity> CreateRandomEntities(int count, string pk = null, string rkPrefix = null)
        {
            List<TestEntity> entities;
            if (pk != null && rkPrefix != null)
            {
                entities = TestEntity.CreateRandomEntities(pk, rkPrefix, count).ToList();
            }
            else
            {
                entities = TestEntity.CreateRandomEntities(count).ToList();
            }

            foreach (var eSet in entities.InSetsOf(100))
            {
                TableBatchOperation batchOperation = new TableBatchOperation();
                foreach (var entity in eSet)
                {
                    batchOperation.Insert(entity);
                }
                Table.ExecuteBatch(batchOperation);
            }
            return entities;
        }

    }
    [TestClass]
    public class SimpleReadTests
    {
        private TestTable PeopleTable = new TestTable();

        [TestInitialize()]
        public void Setup()
        {
            PeopleTable.CreateFresh();
        }

        [TestMethod]
        public void RetriveSingleRow()
        {
            var testEntities = PeopleTable.CreateRandomEntities(1);
            var retrieveOperation = TableOperation.Retrieve<TestEntity>(testEntities.First().PartitionKey, testEntities.First().RowKey);
            var queryResult = PeopleTable.Table.Execute(retrieveOperation);

            TestEntity.AssertEquivalent(testEntities, TestEntity.InList(queryResult));
        }

        [TestMethod]
        public void Read50()
        {
            int NUM_ENTRIES = 50;
            var testEntities = PeopleTable.CreateRandomEntities(NUM_ENTRIES, "read50", "row");
            string pkFilter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "read50");
            TableQuery<TestEntity> query = new TableQuery<TestEntity>().Where(pkFilter);
            var results = PeopleTable.Table.ExecuteQuery(query);

            var count = results.Count();
            Assert.AreEqual(count, NUM_ENTRIES, NUM_ENTRIES.ToString() + " != " + count.ToString());
            TestEntity.AssertEquivalent(testEntities, results);
        }

        [TestMethod]
        public void Read1001()
        {
            int NUM_ENTRIES = 1001;
            var testEntities = PeopleTable.CreateRandomEntities(NUM_ENTRIES, "read1001", "row");
            string pkFilter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "read1001");
            TableQuery<TestEntity> query = new TableQuery<TestEntity>().Where(pkFilter);
            var results = PeopleTable.Table.ExecuteQuery(query);

            var count = results.Count();
            Assert.AreEqual(count, NUM_ENTRIES, NUM_ENTRIES.ToString() + " != " + count.ToString());
            TestEntity.AssertEquivalent(testEntities, results);
        }
    }

    [TestClass]
    public class ReadAllEntries
    {
        private TestTable PeopleTable = new TestTable();
        private int num_entries = 26 * 26 * 10;
        private string partitionPrefix = "partition_";

        [TestInitialize()]
        public void Setup()
        {
            PeopleTable.CreateFresh();
            
            // create 26 partitions with 1001 entries in each partition.
            for (char ch1 = 'A'; ch1 <= 'Z'; ch1++ )
                for (char ch2 = 'A'; ch2 <= 'Z'; ch2++)
                    PeopleTable.CreateRandomEntities(10, partitionPrefix + ch1 + ch2, "row");
        }

        [TestMethod]
        public void ReadAll_Simple()
        {
            // read all entries into a dictionary.
            int totalEntitiesRetrieved = 0;
            {
                TableQuery<TestEntity> query = (new TableQuery<TestEntity>());
                var allpeople = PeopleTable.Table.ExecuteQuery<TestEntity>(query);
                foreach (var person in allpeople)
                {
                    totalEntitiesRetrieved++;
                }
            }
            Assert.AreEqual(totalEntitiesRetrieved, num_entries);
        }

        private List<TestEntity> RunQuerySegmented(TableQuery<TestEntity> query)
        {
            TableQuerySegment<TestEntity> querySegment = null;
            var returnList = new List<TestEntity>();
            while (querySegment == null || querySegment.ContinuationToken != null)
            {
                querySegment = PeopleTable.Table.ExecuteQuerySegmented(query, querySegment != null ? querySegment.ContinuationToken : null);
                returnList.AddRange(querySegment);
            }
            return returnList;
        }

        private async Task<List<TestEntity>> RunQuerySegmentedAsync(TableQuery<TestEntity> query)
        {
            TableQuerySegment<TestEntity> querySegment = null;
            var returnList = new List<TestEntity>();
            while (querySegment == null || querySegment.ContinuationToken != null)
            {
                querySegment = await PeopleTable.Table.ExecuteQuerySegmentedAsync(query, querySegment != null ? querySegment.ContinuationToken : null);
                returnList.AddRange(querySegment);
            }
            return returnList;
        }

        [TestMethod]
        public void ReadAll_Segmented()
        {
            TableQuery<TestEntity> query = new TableQuery<TestEntity>();
            var list = RunQuerySegmented(query);
            Assert.AreEqual(list.Count, num_entries);
        }

        [TestMethod]
        public void ReadAll_Segmented_PerPartition()
        {
            int totalRead = 0;
            for (char ch1 = 'A'; ch1 <= 'Z'; ch1++)
                for (char ch2 = 'A'; ch2 <= 'Z'; ch2++)
                {
                    string pkFilter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionPrefix + ch1 + ch2);
                    TableQuery<TestEntity> query = new TableQuery<TestEntity>().Where(pkFilter);
                    var list = RunQuerySegmented(query);
                    totalRead += list.Count;
                }

            Assert.AreEqual(totalRead, num_entries);
        }

        [TestMethod]
        public void ReadAll_Segmented_ByPartitionRange()
        {
            int totalRead = 0;
            for (char ch1 = 'A'; ch1 <= 'Z'; ch1++)
                {
                    string pkFilter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.GreaterThanOrEqual, partitionPrefix + ch1);
                    if (ch1 != 'Z')
                    {
                        string pkFilter2 = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.LessThan, partitionPrefix + (char)(ch1 + 1));
                        pkFilter = TableQuery.CombineFilters(pkFilter, TableOperators.And, pkFilter2);
                    }
                    
                    TableQuery<TestEntity> query = new TableQuery<TestEntity>().Where(pkFilter);
                    var list = RunQuerySegmented(query);
                    totalRead += list.Count;
                }

            Assert.AreEqual(totalRead, num_entries);
        }
        
        [TestMethod]
        public void ReadAll_Segmented_PerPartitionAsync()
        {
            var taskList = new List<Task<List<TestEntity>>>();
            for (char ch1 = 'A'; ch1 <= 'Z'; ch1++)
                for (char ch2= 'A'; ch2<= 'Z'; ch2++)
            {
                string pkFilter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionPrefix + ch1 + ch2);
                TableQuery<TestEntity> query = new TableQuery<TestEntity>().Where(pkFilter);

                taskList.Add(RunQuerySegmentedAsync(query));
            }
            
            Task.WaitAll(taskList.ToArray());
            long totalEntitiesRetrieved = 0;
            foreach(var task in taskList)
            {
                totalEntitiesRetrieved += task.Result.Count;
            }
            Assert.AreEqual(totalEntitiesRetrieved, num_entries);
        }

        [TestMethod]
        public void ReadAll_Segmented_ByPartitionRangeAsync()
        {
            var taskList = new List<Task<List<TestEntity>>>();
            for (char ch1 = 'A'; ch1 <= 'Z'; ch1++)
            {
                string pkFilter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.GreaterThanOrEqual, partitionPrefix + ch1);
                if (ch1 != 'Z')
                {
                    string pkFilter2 = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.LessThan, partitionPrefix + (char)(ch1 + 1));
                    pkFilter = TableQuery.CombineFilters(pkFilter, TableOperators.And, pkFilter2);
                }

                TableQuery<TestEntity> query = new TableQuery<TestEntity>().Where(pkFilter);
                taskList.Add(RunQuerySegmentedAsync(query));
            }

            Task.WaitAll(taskList.ToArray());
            long totalEntitiesRetrieved = 0;
            foreach (var task in taskList)
            {
                totalEntitiesRetrieved += task.Result.Count;
            }
            Assert.AreEqual(totalEntitiesRetrieved, num_entries);
        }
    }


    [TestClass]
    public class TestEntityResolver
    {
        private TestTable PeopleTable = new TestTable();
        private int num_entries = 4;
        private string partitionprefix = "partition";

        [TestInitialize()]
        public void CreateEntities()
        {
            PeopleTable.CreateFresh();

            var entitiesA = EntityTypeOne.CreateRandomEntities(partitionprefix + "100", "row", 2);
            var entitiesB = EntityTypeTwo.CreateRandomEntities(partitionprefix + "200", "row", 2);

            foreach (var eSet in entitiesA.InSetsOf(100))
            {
                TableBatchOperation batchOperation = new TableBatchOperation();
                foreach (var entity in eSet)
                {
                    batchOperation.Insert(entity);
                }
                PeopleTable.Table.ExecuteBatch(batchOperation);
            }
            foreach (var eSet in entitiesB.InSetsOf(100))
            {
                TableBatchOperation batchOperation = new TableBatchOperation();
                foreach (var entity in eSet)
                {
                    batchOperation.Insert(entity);
                }
                PeopleTable.Table.ExecuteBatch(batchOperation);
            }
        }

        [TestMethod]
        public void ReadEntities()
        {
            int typeOne = 0;
            int typeTwo = 0;
            List<ResolvedEntity> entities = new List<ResolvedEntity>() ;
            {
                TableQuery query = (new TableQuery());
                var allpeople = PeopleTable.Table.ExecuteQuery(query, new EntityResolver<ResolvedEntity>(ResolvedEntity.Resolver), null, null);
                foreach (var person in allpeople)
                {
                    entities.Add(person);
                    if (person.EntityType == "EntityTypeOne")
                    {
                        typeOne++;
                    }
                    else if (person.EntityType == "EntityTypeTwo")
                    {
                        typeTwo++;
                    }
                    else
                    {
                        Assert.Fail("unknown entity");
                    }
                    
                }
            }

            Assert.AreEqual(entities.Count, num_entries);
            Assert.AreEqual(typeOne, num_entries/2);
            Assert.AreEqual(typeTwo, num_entries/2);

        }


        private async Task<List<ResolvedEntity>> RunQuerySegmentedAsync(TableQuery<ResolvedEntity> query)
        {
            TableQuerySegment<ResolvedEntity> querySegment = null;
            var returnList = new List<ResolvedEntity>();
            while (querySegment == null || querySegment.ContinuationToken != null)
            {
                querySegment = await PeopleTable.Table.ExecuteQuerySegmentedAsync(
                    query, 
                    new EntityResolver<ResolvedEntity>(ResolvedEntity.Resolver), 
                    querySegment != null ? querySegment.ContinuationToken : null
                    );
                returnList.AddRange(querySegment);
            }
            return returnList;
        }

        [TestMethod]
        public void ReadAll_Segmented_ByPartitionRangeAsync()
        {
            var taskList = new List<Task<List<ResolvedEntity>>>();
            for (char ch1 = '1'; ch1 <= '2'; ch1++)
            {
                string pkFilter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.GreaterThanOrEqual, partitionprefix + ch1);
                if (ch1 != '2')
                {
                    string pkFilter2 = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.LessThan, partitionprefix + (char)(ch1 + 1));
                    pkFilter = TableQuery.CombineFilters(pkFilter, TableOperators.And, pkFilter2);
                }

                TableQuery<ResolvedEntity> query = new TableQuery<ResolvedEntity>().Where(pkFilter);
                taskList.Add(RunQuerySegmentedAsync(query));
            }

            Task.WaitAll(taskList.ToArray());
            
            long totalEntitiesRetrieved = 0;
            int typeOne = 0;
            int typeTwo = 0;
            foreach (var task in taskList)
            {
                foreach (var entry in task.Result)
                {
                    if (entry.TypeOne())
                        typeOne++;
                    else
                        typeTwo++;
                }
                totalEntitiesRetrieved += task.Result.Count;
            }
            Assert.AreEqual(totalEntitiesRetrieved, num_entries);
            Assert.AreEqual(typeOne, num_entries/2);
            Assert.AreEqual(typeTwo, num_entries/2);
        }

    }

    [TestClass]
    public class NestedAndEnumTypes
    {
        private TestTable PeopleTable = new TestTable();
        [TestInitialize()]
        public void CreateEntities()
        {
            PeopleTable.CreateFresh();
        }
        [TestMethod]
        public void CreateNestedEntryAndRetrive()
        {
            var entry = new NestedEntity("pk1", "rk1", 2);

            var insertOperation = TableOperation.Insert(entry);
            var insertResult = PeopleTable.Table.Execute(insertOperation);

            var retrieveOperation = TableOperation.Retrieve<NestedEntity>(entry.PartitionKey, entry.RowKey);
            var queryResult = PeopleTable.Table.Execute(retrieveOperation);

            // types comes equal
            Assert.AreEqual(entry.GetType(), queryResult.Result.GetType());

            var result = (NestedEntity)queryResult.Result;

            // int64 values gets stored fine.
            Assert.IsTrue(entry.int64Value == result.int64Value);

            // but nested classes DO NOT get saved!
            Assert.IsFalse(entry.inClass.intvalue == result.inClass.intvalue);

            // nested structures do not get saved either.
            Assert.IsFalse(entry.inStruct.intvalue == result.inStruct.intvalue);

            // enum values do not get stored either.
            Assert.IsFalse(entry.flagValue == result.flagValue);

        }

    }

    [TestClass]
    public class TableUpdateTests
    {
        private TestTable PeopleTable = new TestTable();
        [TestInitialize()]
        public void CreateEntities()
        {
            PeopleTable.CreateFresh();
        }

        [TestMethod]
        public void CreateSingleEntryAndRetrive()
        {
            var entry = EntityTypeTwo.CreateRandomEntity("xyz", "row");
            Assert.AreEqual(EntityTypeTwo.DEFAULT_VALUE, entry.updatefield);
            Assert.AreEqual(EntityTypeTwo.DEFAULT_VALUE, entry.do_not_updatefield);

            entry.updatefield = "newvalue";

            var insertOperation = TableOperation.Insert(entry);
            var insertResult = PeopleTable.Table.Execute(insertOperation);

            var retrieveOperation = TableOperation.Retrieve<EntityTypeTwo>(entry.PartitionKey, entry.RowKey);
            var queryResult = PeopleTable.Table.Execute(retrieveOperation);

            EntityTypeTwo.AssertEqual(entry, (EntityTypeTwo)queryResult.Result);

        }


        [TestMethod]
        public void CreateSingleEntry_AndUpdateSingleField()
        {
            var entry = EntityTypeTwo.CreateRandomEntity("xyz", "row");
            Assert.AreEqual(EntityTypeTwo.DEFAULT_VALUE, entry.updatefield);
            Assert.AreEqual(EntityTypeTwo.DEFAULT_VALUE, entry.do_not_updatefield);

            entry.updatefield = "newvalue_1";

            var insertOperation = TableOperation.Insert(entry);
            var insertResult = PeopleTable.Table.Execute(insertOperation);

            var retrieveOperation = TableOperation.Retrieve<EntityTypeTwo>(entry.PartitionKey, entry.RowKey);
            var queryResult = PeopleTable.Table.Execute(retrieveOperation);
            var newEntry = (EntityTypeTwo)queryResult.Result;

            EntityTypeTwo.AssertEqual(entry, newEntry);
            Assert.AreEqual(newEntry.do_not_updatefield, EntityTypeTwo.DEFAULT_VALUE);
            Assert.AreEqual(newEntry.updatefield, "newvalue_1");
        }

    }

    [TestClass]
    public class UsingDynamicObjects
    {
        private TestTable PeopleTable = new TestTable();
        [TestInitialize()]
        public void CreateEntities()
        {
            PeopleTable.CreateFresh();
        }

        [TestMethod]
        public void CreateDynamicEntry() 
        { 
            try 
            {
                // create a new entry of type EntityTypeTwo
                var entry = EntityTypeTwo.CreateRandomEntity("xyz", "row");
                Assert.AreEqual(EntityTypeTwo.DEFAULT_VALUE, entry.updatefield);
                Assert.AreEqual(EntityTypeTwo.DEFAULT_VALUE, entry.do_not_updatefield);

                entry.updatefield = "newvalue_1";

                var insertOperation = TableOperation.Insert(entry);
                var insertResult = PeopleTable.Table.Execute(insertOperation);

                var retrieveOperation = TableOperation.Retrieve<EntityTypeTwo>(entry.PartitionKey, entry.RowKey);
                var queryResult = PeopleTable.Table.Execute(retrieveOperation);
                var newEntry = (EntityTypeTwo)queryResult.Result;

                EntityTypeTwo.AssertEqual(entry, newEntry);
                Assert.AreEqual(newEntry.do_not_updatefield, EntityTypeTwo.DEFAULT_VALUE);
                Assert.AreEqual(newEntry.updatefield, "newvalue_1");

                dynamic entity = new DynamicObjectTableEntity("xyz", "row");
                entity.ETag = "*";
                entity.updatefield = "newvalue_2"; ; 
                PeopleTable.Table.Execute(TableOperation.Merge(entity));

                var retrieveOperation2 = TableOperation.Retrieve<EntityTypeTwo>(entry.PartitionKey, entry.RowKey);
                var queryResult2 = PeopleTable.Table.Execute(retrieveOperation);
                var newEntry2 = (EntityTypeTwo)queryResult2.Result;

                Assert.AreEqual(newEntry2.do_not_updatefield, EntityTypeTwo.DEFAULT_VALUE);
                Assert.AreEqual(newEntry2.updatefield, "newvalue_2");
            } 
            catch (Exception ex) 
            { 
                Assert.Fail(ex.Message); 
            } 
        } 
    }
}


