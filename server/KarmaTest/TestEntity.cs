using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.Diagnostics;

namespace Sepialabs.Azure.Test
{
    public class TestEntity : TableEntity
    {
        public byte[] Payload { get; set; }

        public string GetPayloadString()
        {
            if (Payload == null)
            {
                return null;
            }
            else if (Payload.Length == 0)
            {
                return "";
            }
            else
            {
                return Encoding.Unicode.GetString(Payload);
            }
        }

        public void SetPayloadString(string s)
        {
            if (s == null)
            {
                Payload = null;
            }
            else if (s.Length == 0)
            {
                Payload = new byte[0];
            }
            else
            {
                Payload = Encoding.Unicode.GetBytes(s);
            }
        }

        public static TestEntity CreateRandomEntity()
        {
            var u = new TestEntity()
            {
                PartitionKey = Guid.NewGuid().ToString(),
                RowKey = Guid.NewGuid().ToString()
            };
            
            u.SetPayloadString(DateTime.UtcNow.ToString("r"));
            
            return u;
        }

        public static TestEntity CreateRandomEntity(string pk, string rk)
        {
            var u = new TestEntity()
            {
                PartitionKey = pk,
                RowKey = rk
            };

            u.SetPayloadString(DateTime.UtcNow.ToString("r"));

            return u;
        }

        public static IEnumerable<TestEntity> CreateRandomEntities(string pk, string rkPrefix, int count = 50)
        {
            for (int i = 0; i < count; i++)
            {
                yield return CreateRandomEntity(pk, rkPrefix + Guid.NewGuid().ToString());
            }
        }

        public static IEnumerable<TestEntity> CreateRandomEntities(int count = 50)
        {
            for (int i = 0; i < count; i++)
            {
                yield return CreateRandomEntity();
            }
        }

        internal static void AssertEquivalent(IEnumerable<TestEntity> expected, IEnumerable<TestEntity> actual)
        {
            Assert.AreEqual(expected.Count(), actual.Count(), "Number of entities is different");
            foreach (var e in expected)
            {
                var a = actual.FirstOrDefault(x => x.PartitionKey == e.PartitionKey && x.RowKey == e.RowKey);
                Assert.IsNotNull(a, "Didn't find matching entity for PKey {0}, RKey {1}", e.PartitionKey, e.RowKey);

                AssertEqual(e, a);
            }
        }

        public static void AssertEqual(TestEntity expected, TestEntity actual)
        {
            Assert.IsNotNull(expected, "Expected is null");
            Assert.IsNotNull(actual, "Actual is null");
            CollectionAssert.AreEqual(expected.Payload, actual.Payload, "Payload is not equal");
            Assert.AreEqual(expected.PartitionKey, actual.PartitionKey, "PartitionKey doesn't match");
            Assert.AreEqual(expected.RowKey, actual.RowKey, "RowKey doesn't match");
        }

        public static List<TestEntity> InList(TestEntity item)
        {
            return new List<TestEntity> { item };
        }

        internal static List<TestEntity> InList(TableResult queryResult)
        {
            return new List<TestEntity> { (TestEntity)(queryResult.Result) };
        }
    }

    public class ResolvedEntity : TableEntity
    {
        public virtual string EntityType { get; set; }
        public virtual bool TypeOne() 
        {
            throw new InvalidOperationException();
        }

        public ResolvedEntity()
        {
            this.EntityType = this.GetType().Name;
            Debug.Assert(this.GetType() != typeof(ResolvedEntity));
        }

        public static ResolvedEntity Resolver(
            string pk,
            string rk,
            DateTimeOffset ts,
            IDictionary<string, EntityProperty> props,
            string etag)
        {
            ResolvedEntity resolvedEntity = null;
            string entityType = props["EntityType"].StringValue;

            if (entityType == "EntityTypeOne") { resolvedEntity = new EntityTypeOne(); }
            else if (entityType == "EntityTypeTwo") { resolvedEntity = new EntityTypeTwo(); }
            else 
            {
                Assert.Fail("Unknown EntityType:" + entityType);
                return null;
            }

            resolvedEntity.PartitionKey = pk;
            resolvedEntity.RowKey = rk;
            resolvedEntity.Timestamp = ts;
            resolvedEntity.ETag = etag;
            resolvedEntity.ReadEntity(props, null);

            return resolvedEntity;
        }
    }

    public class EntityTypeOne : ResolvedEntity
    {
        public string payload = "";

        public EntityTypeOne()
        {
            this.IgnoredProperty = "must get ignored";
            this.NotIgnoredProperty = "must not get ignored";
        }

        public static IEnumerable<EntityTypeOne> CreateRandomEntities(string pk, string rkPrefix, int count = 50)
        {
            for (int i = 0; i < count; i++)
            {
                yield return CreateRandomEntity(pk, rkPrefix + Guid.NewGuid().ToString());
            }
        }

        public static EntityTypeOne CreateRandomEntity(string pk, string rk)
        {
            var u = new EntityTypeOne()
            {
                PartitionKey = pk,
                RowKey = rk
            };

            u.payload = DateTime.UtcNow.ToString("r");

            return u;
        }

        public override bool TypeOne()
        {
            return true;
        }

        public string publicProperty;

        [IgnoreProperty]
        public string IgnoredProperty {get;set;}
        public string NotIgnoredProperty { get; set; }


    }

    public class EntityTypeTwo : ResolvedEntity
    {
        public const string DEFAULT_VALUE = "defaultvalue";
        public string payload {get;set;}

        public EntityTypeTwo()
        {
            this.updatefield = DEFAULT_VALUE;
            this.do_not_updatefield = DEFAULT_VALUE;
            this.payload = "";
        }
        public static IEnumerable<EntityTypeTwo> CreateRandomEntities(string pk, string rkPrefix, int count = 50)
        {
            for (int i = 0; i < count; i++)
            {
                yield return CreateRandomEntity(pk, rkPrefix + Guid.NewGuid().ToString());
            }
        }

        public static EntityTypeTwo CreateRandomEntity(string pk, string rk)
        {
            var u = new EntityTypeTwo()
            {
                PartitionKey = pk,
                RowKey = rk
            };

            u.payload = DateTime.UtcNow.ToString("r");

            return u;
        }

        public override bool TypeOne()
        {
            return false;
        }

        public string updatefield {get;set;}
        public string do_not_updatefield {get;set;}

        public static void AssertEqual(EntityTypeTwo expected, EntityTypeTwo actual)
        {
            Assert.IsNotNull(expected, "Expected is null");
            Assert.IsNotNull(actual, "Actual is null");
            Assert.AreEqual(expected.payload, actual.payload, "payload is not equal");
            Assert.AreEqual(expected.PartitionKey, actual.PartitionKey, "PartitionKeyis not equal");
            Assert.AreEqual(expected.RowKey, actual.RowKey, "RowKey not equal");
            Assert.AreEqual(expected.RowKey, actual.RowKey, "RowKey not equal");
            Assert.AreEqual(expected.updatefield, actual.updatefield, "updatefield is not equal");
            Assert.AreEqual(expected.do_not_updatefield, actual.do_not_updatefield, "do_not_updatefield is not equal");
        }
    }

    public enum EFlag
    {
        EZero = 0,
        EOne = 1,
        ETwo = 2,
        EThree = 3,
        EFour = 4,
        EFive = 5,
        ESix = 6,
        ESeven = 7,
        EMax = 8
    }

    public struct InsideStruct
    {
        public  InsideStruct(int value)
        {
            this.intField = value;
        }


        public int intvalue {
            get
            {
                return intField;
            }
            set
            {
                intField = value;
            }
        }

        public int intField;
    }
    public class InsideClass
    {
        public InsideClass(int i)
        {
            this.intvalue = i;
            this.stringvalue = i.ToString();
        }

        public int intvalue {get;set;}
        public string stringvalue {get;set;}
    }
    public class NestedEntity : TableEntity
    {
        public NestedEntity()
        {
            // setup default values.
            this.inStruct = new InsideStruct((int)EFlag.EMax);
            this.inClass = new InsideClass((int)EFlag.EMax);
            this.flagValue = EFlag.EMax;
            this.int64Value = (Int64)EFlag.EMax;
        }

        public NestedEntity(string pk, string rk, int Value)
        {
            this.PartitionKey = pk;
            this.RowKey = rk;

            // use seeding value to populate the members.
            Assert.IsTrue(EFlag.EMax > (EFlag)Value);
            this.inStruct = new InsideStruct(Value);
            this.inClass = new InsideClass(Value);
            this.flagValue = (EFlag)Value;
            this.int64Value = Value;

            Assert.IsTrue((int) this.flagValue == Value);
        }

        public InsideStruct inStruct { get; set; }
        public InsideClass inClass { get; set; }
        public EFlag flagValue { get; set; }
        public Int64 int64Value { get; set; }
    }
}
