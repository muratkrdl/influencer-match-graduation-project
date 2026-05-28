using System.Collections.Generic;
using Final.InfluencerMatch.Common;
using Final.InfluencerMatch.Recommendation;
using Final.Tests.Helpers;
using NUnit.Framework;

namespace Final.Tests.Recommendation
{
    [TestFixture]
    public sealed class InfluencerDataTests
    {
        private List<InfluencerData> m_Created;

        [SetUp]
        public void SetUp()
        {
            m_Created = new List<InfluencerData>();
        }

        [TearDown]
        public void TearDown()
        {
            TestDataFactory.DestroyAll(m_Created);
        }

        [Test]
        public void GetScoreFor_None_ReturnsZero()
        {
            InfluencerData data = NewInfluencer((CategoryId.Education, 3));
            Assert.AreEqual(0, data.GetScoreFor(CategoryId.None));
        }

        [Test]
        public void GetScoreFor_ExistingCategory_ReturnsAuthoredScore()
        {
            InfluencerData data = NewInfluencer((CategoryId.Education, 3), (CategoryId.Sports, 5));
            Assert.AreEqual(3, data.GetScoreFor(CategoryId.Education));
            Assert.AreEqual(5, data.GetScoreFor(CategoryId.Sports));
        }

        [Test]
        public void GetScoreFor_MissingCategory_ReturnsZero()
        {
            InfluencerData data = NewInfluencer((CategoryId.Education, 3));
            Assert.AreEqual(0, data.GetScoreFor(CategoryId.Fashion));
        }

        [Test]
        public void GetScoreFor_NoCategoryScoresAuthored_ReturnsZero()
        {
            InfluencerData data = NewInfluencer();
            Assert.AreEqual(0, data.GetScoreFor(CategoryId.Education));
        }

        private InfluencerData NewInfluencer(params (CategoryId, int)[] scores)
        {
            InfluencerData data = TestDataFactory.CreateInfluencer("a", "A", 1_000, 0.05f, 1_000, scores);
            m_Created.Add(data);
            return data;
        }
    }
}
