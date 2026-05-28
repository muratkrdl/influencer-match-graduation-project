using System.Collections.Generic;
using Final.InfluencerMatch.Common;
using Final.InfluencerMatch.Recommendation;
using Final.Tests.Helpers;
using NUnit.Framework;

namespace Final.Tests.Common
{
    [TestFixture]
    public sealed class ScoredInfluencerTests
    {
        private List<InfluencerData> m_Created;
        private InfluencerData m_Influencer;

        [SetUp]
        public void SetUp()
        {
            m_Created = new List<InfluencerData>();
            m_Influencer = TestDataFactory.CreateInfluencer("a", "A", 1_000, 0.05f, 1_000);
            m_Created.Add(m_Influencer);
        }

        [TearDown]
        public void TearDown()
        {
            TestDataFactory.DestroyAll(m_Created);
        }

        [Test]
        public void Ctor_HybridScoreBelowZero_ClampedToZero()
        {
            ScoredInfluencer s = new ScoredInfluencer(m_Influencer, -0.5f, false);
            Assert.AreEqual(0f, s.HybridScore);
        }

        [Test]
        public void Ctor_HybridScoreAboveOne_ClampedToOne()
        {
            ScoredInfluencer s = new ScoredInfluencer(m_Influencer, 1.5f, false);
            Assert.AreEqual(1f, s.HybridScore);
        }

        [Test]
        public void Ctor_InRangeHybridScore_Preserved()
        {
            ScoredInfluencer s = new ScoredInfluencer(m_Influencer, 0.42f, false);
            Assert.AreEqual(0.42f, s.HybridScore);
        }

        [Test]
        public void Ctor_StoresIsOverBudget()
        {
            Assert.IsTrue(new ScoredInfluencer(m_Influencer, 0.5f, true).IsOverBudget);
            Assert.IsFalse(new ScoredInfluencer(m_Influencer, 0.5f, false).IsOverBudget);
        }

        [Test]
        public void Ctor_StoresInfluencerReference()
        {
            ScoredInfluencer s = new ScoredInfluencer(m_Influencer, 0.5f, false);
            Assert.AreSame(m_Influencer, s.Influencer);
        }

        [Test]
        public void CompatibilityPercent_DerivedFromHybridScore_ZeroAndBoundaries()
        {
            Assert.AreEqual(0, new ScoredInfluencer(m_Influencer, 0f, false).CompatibilityPercent);
            Assert.AreEqual(50, new ScoredInfluencer(m_Influencer, 0.5f, false).CompatibilityPercent);
            Assert.AreEqual(100, new ScoredInfluencer(m_Influencer, 1f, false).CompatibilityPercent);
        }

        [Test]
        public void CompatibilityPercent_ClampedToOneHundred_WhenHybridScoreClampedFromAbove()
        {
            // hybridScore 1.5f clamps to 1.0 → percent 100.
            Assert.AreEqual(100, new ScoredInfluencer(m_Influencer, 1.5f, false).CompatibilityPercent);
        }

        [Test]
        public void CompatibilityPercent_ClampedToZero_WhenHybridScoreClampedFromBelow()
        {
            // hybridScore -0.5f clamps to 0.0 → percent 0.
            Assert.AreEqual(0, new ScoredInfluencer(m_Influencer, -0.5f, false).CompatibilityPercent);
        }
    }
}
