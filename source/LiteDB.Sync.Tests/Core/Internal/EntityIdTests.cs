using System;
using System.Collections.Generic;
using LiteDB.Sync.Internal;
using NUnit.Framework;

namespace LiteDB.Sync.Tests.Core.Internal
{
    [TestFixture]
    public class EntityIdTests
    {
        public class WhenCreating : EntityIdTests
        {
            [Test]
            public void MinValueShouldThrow()
            {
                Assert.Throws<ArgumentOutOfRangeException>(() => new EntityId("Coll", BsonValue.MinValue));
            }

            [Test]
            public void MaxValueShouldThrow()
            {
                Assert.Throws<ArgumentOutOfRangeException>(() => new EntityId("Coll", BsonValue.MaxValue));
            }

            [Test]
            public void NullBsonValueShouldThrow()
            {
                Assert.Throws<ArgumentOutOfRangeException>(() => new EntityId("Coll", BsonValue.Null));
            }

            [Test]
            public void NullCollectionShouldThrow()
            {
                Assert.Throws<ArgumentNullException>(() => new EntityId(null, new BsonValue("Hello")));
            }

            [Test]
            public void EmptyCollectionShouldThrow()
            {
                Assert.Throws<ArgumentNullException>(() => new EntityId(string.Empty, new BsonValue("Hello")));
            }

            [Test]
            public void NullValueShouldThrow()
            {
                Assert.Throws<ArgumentNullException>(() => new EntityId("Coll", null));
            }
        }

        public class WhenComparing : EntityIdTests
        {
            [Test]
            public void ShouldBeEqualWhenPropertiesAreEqual()
            {
                var a = new EntityId("MyColl", 123);
                var b = new EntityId("MyColl", 123);

                Assert.IsTrue(a.Equals(b));
            }

            [Test]
            public void ShouldNotBeEqualWhenCollectionNameIsDifferent()
            {
                var a = new EntityId("MyColl", 123);
                var b = new EntityId("AAAA", 123);

                Assert.IsFalse(a.Equals(b));
            }

            [Test]
            public void ShouldNotBeEqualWhenCollectionIdIsDifferent()
            {
                var a = new EntityId("MyColl", 123);
                var b = new EntityId("MyColl", 456);

                Assert.IsFalse(a.Equals(b));
            }
        }

        public class WhenComparingWithEqualsOperator : EntityIdTests
        {
            [Test]
            public void ShouldBeEqualWhenPropertiesAreEqual()
            {
                var a = new EntityId("MyColl", 123);
                var b = new EntityId("MyColl", 123);

                Assert.IsTrue(a == b);
            }

            [Test]
            public void ShouldNotBeEqualWhenCollectionNameIsDifferent()
            {
                var a = new EntityId("MyColl", 123);
                var b = new EntityId("AAAA", 123);

                Assert.IsFalse(a == b);
            }

            [Test]
            public void ShouldNotBeEqualWhenCollectionIdIsDifferent()
            {
                var a = new EntityId("MyColl", 123);
                var b = new EntityId("MyColl", 456);

                Assert.IsFalse(a == b);
            }
        }

        public class WhenComparingWithNotEqualsOperator : EntityIdTests
        {
            [Test]
            public void ShouldBeEqualWhenPropertiesAreEqual()
            {
                var a = new EntityId("MyColl", 123);
                var b = new EntityId("MyColl", 123);

                Assert.IsFalse(a != b);
            }

            [Test]
            public void ShouldNotBeEqualWhenCollectionNameIsDifferent()
            {
                var a = new EntityId("MyColl", 123);
                var b = new EntityId("AAAA", 123);

                Assert.IsTrue(a != b);
            }

            [Test]
            public void ShouldNotBeEqualWhenCollectionIdIsDifferent()
            {
                var a = new EntityId("MyColl", 123);
                var b = new EntityId("MyColl", 456);

                Assert.IsTrue(a != b);
            }
        }

        public class WhenGettingHashCode : EntityIdTests
        {
            [Test]
            public void ShouldBeEqualWhenPropertiesAreEqual()
            {
                var a = new EntityId("MyColl", 123);
                var b = new EntityId("MyColl", 123);

                Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
            }

            [Test]
            public void ShouldNotBeEqualWhenCollectionNameIsDifferent()
            {
                var a = new EntityId("MyColl", 123);
                var b = new EntityId("AAAA", 123);

                Assert.AreNotEqual(a.GetHashCode(), b.GetHashCode());
            }

            [Test]
            public void ShouldNotBeEqualWhenCollectionIdIsDifferent()
            {
                var a = new EntityId("MyColl", 123);
                var b = new EntityId("MyColl", 456);

                Assert.AreNotEqual(a.GetHashCode(), b.GetHashCode());
            }
        }

        public class WhenConvertingToString : EntityIdTests
        {
            [Test]
            public void ShouldSerializeString()
            {
                this.ShouldSerialize("Hello");               
            }

            [Test]
            public void ShouldSerializeInt32()
            {
                this.ShouldSerialize(123);
            }

            [Test]
            public void ShouldSerializeInt64()
            {
                this.ShouldSerialize(long.MaxValue);
            }

            [Test]
            public void ShouldSerializeDouble()
            {
                this.ShouldSerialize(0.35);
            }

            [Test]
            public void ShouldSerializeDecimal()
            {
                this.ShouldSerialize(0.35m);
            }

            [Test]
            public void ShouldSerializeBool()
            {
                this.ShouldSerialize(true);
            }

            [Test]
            public void ShouldSerializeDateTime()
            {
                this.ShouldSerialize(DateTime.Now);
            }

            [Test]
            public void ShouldSerializeGuid()
            {
                this.ShouldSerialize(Guid.NewGuid());
            }

            [Test]
            public void ShouldSerializeObjectId()
            {
                var bytes = new byte[12];
                new Random().NextBytes(bytes);

                this.ShouldSerialize(new ObjectId(bytes));
            }

            [Test]
            public void ShouldSerializeIntArray()
            {
                this.ShouldSerialize(new[] { 123, 456 });
            }

            [Test]
            public void ShouldSerializeBinary()
            {
                this.ShouldSerialize(new BsonValue(new byte[] { 123, 222 }));
            }

            [Test]
            public void ShouldSerializeDictionary()
            {
                var doc = new Dictionary<string, BsonValue>();
                doc["Prop1"] = "Hello";
                doc["Prop2"] = 123;

                this.ShouldSerialize(doc);
            }

            private void ShouldSerialize(object value)
            {
                var entityId = new EntityId("Coll", value);

                var str = entityId.ToString();
                var actual = EntityId.FromString(str);

                Assert.AreEqual("Coll", actual.CollectionName);

                //if (expected.IsDocument)
                //{
                    
                //}
                //else
                //{
                //    Assert.AreEqual(expected.RawValue, actual.Id);
                //}
            }
        }
    }
}