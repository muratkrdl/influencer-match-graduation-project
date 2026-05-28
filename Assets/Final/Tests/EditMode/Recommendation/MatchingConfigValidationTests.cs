using Final.InfluencerMatch.Recommendation;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Final.Tests.Recommendation
{
    /// <summary>
    /// Loads the production MatchingConfig asset and asserts the same invariants the custom inspector surfaces (weight sum, multiplier counts, normalization bounds).
    /// </summary>
    [TestFixture]
    public sealed class MatchingConfigValidationTests
    {
        private const string k_ConfigPath = "Assets/Final/Runtime/Data/Configs/MatchingConfig.asset";
        private const float k_WeightSumTolerance = 0.001f;
        private const int k_ExpectedScoreMultiplierCount = 5;
        private const int k_ExpectedTierCount = 5;

        private MatchingConfig m_Config;

        [SetUp]
        public void SetUp()
        {
            m_Config = AssetDatabase.LoadAssetAtPath<MatchingConfig>(k_ConfigPath);
            Assert.IsNotNull(m_Config, "MatchingConfig asset not found at " + k_ConfigPath);
        }

        [Test]
        public void Config_WeightsSumToOne()
        {
            float sum = m_Config.CategoryWeight + m_Config.FollowersWeight + m_Config.EngagementWeight;
            Assert.That(Mathf.Abs(sum - 1.0f), Is.LessThan(k_WeightSumTolerance),
                "Weights sum to " + sum.ToString("F3") + ", expected ≈ 1.000.");
        }

        [Test]
        public void Config_CategoryScoreMultipliers_HasExpectedCount()
        {
            Assert.AreEqual(k_ExpectedScoreMultiplierCount, m_Config.CategoryScoreMultipliers.Count);
        }

        [Test]
        public void Config_FollowerTierMultipliers_HasExpectedCount()
        {
            Assert.AreEqual(k_ExpectedTierCount, m_Config.FollowerTierMultipliers.Count);
        }

        [Test]
        public void Config_FollowerNormalizationMin_IsLessThanMax()
        {
            Assert.Less(m_Config.FollowerNormalizationMin, m_Config.FollowerNormalizationMax);
        }

        [Test]
        public void Config_MinimumCategoryScoreToInclude_IsInRange()
        {
            Assert.GreaterOrEqual(m_Config.MinimumCategoryScoreToInclude, 0);
            Assert.LessOrEqual(m_Config.MinimumCategoryScoreToInclude, 5);
        }

        [Test]
        public void Config_PriceRoundingTL_IsPositive()
        {
            Assert.Greater(m_Config.PriceRoundingTL, 0);
        }
    }
}
