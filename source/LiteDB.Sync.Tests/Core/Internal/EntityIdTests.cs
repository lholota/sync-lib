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
    }
}