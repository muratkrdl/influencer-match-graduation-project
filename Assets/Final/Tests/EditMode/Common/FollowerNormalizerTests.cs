using Final.InfluencerMatch.Common;
using NUnit.Framework;

namespace Final.Tests.Common
{
    [TestFixture]
    public sealed class FollowerNormalizerTests
    {
        private const int k_Min = 1000;
        private const int k_Max = 10_000_000;
        private const float k_Tolerance = 0.01f;

        [Test]
        public void Normalize_ReturnsZero_WhenFollowersIsZeroOrNegative()
        {
            Assert.AreEqual(0f, FollowerNormalizer.Normalize(0, k_Min, k_Max), k_Tolerance);
            Assert.AreEqual(0f, FollowerNormalizer.Normalize(-100, k_Min, k_Max), k_Tolerance);
        }

        [Test]
        public void Normalize_ReturnsZero_WhenAtOrBelowMin()
        {
            Assert.AreEqual(0f, FollowerNormalizer.Normalize(500, k_Min, k_Max), k_Tolerance);
            Assert.AreEqual(0f, FollowerNormalizer.Normalize(k_Min, k_Min, k_Max), k_Tolerance);
        }

        [Test]
        public void Normalize_ReturnsOne_WhenAtOrAboveMax()
        {
            Assert.AreEqual(1f, FollowerNormalizer.Normalize(k_Max, k_Min, k_Max), k_Tolerance);
            Assert.AreEqual(1f, FollowerNormalizer.Normalize(50_000_000, k_Min, k_Max), k_Tolerance);
        }

        [Test]
        public void Normalize_IsBetweenZeroAndOne_ForMidrange()
        {
            float result = FollowerNormalizer.Normalize(100_000, k_Min, k_Max);
            Assert.Greater(result, 0f);
            Assert.Less(result, 1f);
        }

        [Test]
        public void Normalize_IsMonotonicallyIncreasing()
        {
            float a = FollowerNormalizer.Normalize(5_000, k_Min, k_Max);
            float b = FollowerNormalizer.Normalize(50_000, k_Min, k_Max);
            float c = FollowerNormalizer.Normalize(500_000, k_Min, k_Max);
            float d = FollowerNormalizer.Normalize(5_000_000, k_Min, k_Max);

            Assert.LessOrEqual(a, b);
            Assert.LessOrEqual(b, c);
            Assert.LessOrEqual(c, d);
        }

        [Test]
        public void Normalize_ReturnsZero_OnInvalidRange()
        {
            Assert.AreEqual(0f, FollowerNormalizer.Normalize(50_000, 5_000, 1_000), k_Tolerance);
            Assert.AreEqual(0f, FollowerNormalizer.Normalize(50_000, 5_000, 5_000), k_Tolerance);
        }

        [Test]
        public void Normalize_MatchesExpectedValuesAtKnownPoints()
        {
            float result100K = FollowerNormalizer.Normalize(100_000, k_Min, k_Max);
            Assert.AreEqual(0.4999f, result100K, k_Tolerance);

            float result1M = FollowerNormalizer.Normalize(1_000_000, k_Min, k_Max);
            Assert.AreEqual(0.7500f, result1M, k_Tolerance);
        }
    }
}
