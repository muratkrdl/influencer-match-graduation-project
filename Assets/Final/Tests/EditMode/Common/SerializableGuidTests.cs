using System;
using Final.InfluencerMatch.Common;
using NUnit.Framework;

namespace Final.Tests.Common
{
    [TestFixture]
    public sealed class SerializableGuidTests
    {
        [Test]
        public void Empty_HasIsEmptyTrue()
        {
            Assert.IsTrue(SerializableGuid.Empty.IsEmpty);
        }

        [Test]
        public void DefaultStruct_IsEmpty()
        {
            SerializableGuid def = default;
            Assert.IsTrue(def.IsEmpty);
        }

        [Test]
        public void NewGuid_IsNonEmpty()
        {
            Assert.IsFalse(SerializableGuid.NewGuid().IsEmpty);
        }

        [Test]
        public void NewGuid_ProducesUniqueValues()
        {
            SerializableGuid a = SerializableGuid.NewGuid();
            SerializableGuid b = SerializableGuid.NewGuid();
            Assert.AreNotEqual(a, b);
        }

        [Test]
        public void Equals_ReturnsTrue_ForSameValue()
        {
            Guid g = Guid.NewGuid();
            SerializableGuid a = new SerializableGuid(g);
            SerializableGuid b = new SerializableGuid(g);

            Assert.IsTrue(a.Equals(b));
            Assert.IsTrue(a == b);
            Assert.IsFalse(a != b);
        }

        [Test]
        public void Equals_ReturnsFalse_ForDifferentValues()
        {
            SerializableGuid a = SerializableGuid.NewGuid();
            SerializableGuid b = SerializableGuid.NewGuid();

            Assert.IsFalse(a.Equals(b));
            Assert.IsFalse(a == b);
            Assert.IsTrue(a != b);
        }

        [Test]
        public void GetHashCode_IsSame_ForEqualGuids()
        {
            Guid g = Guid.NewGuid();
            Assert.AreEqual(new SerializableGuid(g).GetHashCode(), new SerializableGuid(g).GetHashCode());
        }

        [Test]
        public void CompareTo_ReturnsZero_ForEqualGuids()
        {
            Guid g = Guid.NewGuid();
            SerializableGuid a = new SerializableGuid(g);
            SerializableGuid b = new SerializableGuid(g);

            Assert.AreEqual(0, a.CompareTo(b));
        }

        [Test]
        public void CompareTo_IsAntisymmetric()
        {
            SerializableGuid a = SerializableGuid.NewGuid();
            SerializableGuid b = SerializableGuid.NewGuid();
            int ab = a.CompareTo(b);
            int ba = b.CompareTo(a);

            if (ab == 0)
            {
                Assert.AreEqual(0, ba);
            }
            else
            {
                Assert.AreEqual(Math.Sign(ab), -Math.Sign(ba));
            }
        }

        [Test]
        public void ToString_MatchesUnderlyingGuid()
        {
            Guid g = Guid.NewGuid();
            Assert.AreEqual(g.ToString(), new SerializableGuid(g).ToString());
        }
    }
}
