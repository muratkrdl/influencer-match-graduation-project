using System.Collections.Generic;
using Final.InfluencerMatch.Common;
using Final.InfluencerMatch.Recommendation;
using Final.Tests.Helpers;
using NUnit.Framework;
using UnityEngine;

namespace Final.Tests.Common
{
    [TestFixture]
    public sealed class PricingServiceTests
    {
        private const float k_FloatTolerance = 0.001f;

        private PricingService m_Service;
        private MatchingConfig m_Config;
        private List<InfluencerData> m_Created;

        [SetUp]
        public void SetUp()
        {
            m_Service = new PricingService();
            m_Config = TestDataFactory.CreateDefaultMatchingConfig();
            m_Created = new List<InfluencerData>();
        }

        [TearDown]
        public void TearDown()
        {
            TestDataFactory.DestroyAll(m_Created);
            if (m_Config != null)
            {
                Object.DestroyImmediate(m_Config);
                m_Config = null;
            }
        }

        private InfluencerData NewInfluencer(string id, int followers, int basePrice, params (CategoryId, int)[] scores)
        {
            InfluencerData data = TestDataFactory.CreateInfluencer(id, id, followers, 0.05f, basePrice, scores);
            m_Created.Add(data);
            return data;
        }

        [Test]
        public void Calculate_ReturnsBasePrice_WhenAllMultipliersAreOne()
        {
            // category score 3 -> multiplier 1.0; followers 50_000 -> medium tier 1.0.
            InfluencerData influencer = NewInfluencer("inf_001", 50_000, 10_000, (CategoryId.Education, 3));
            IReadOnlyList<CategoryId> categories = new List<CategoryId> { CategoryId.Education };

            PriceBreakdown breakdown = m_Service.Calculate(influencer, categories, m_Config);

            Assert.AreEqual(10_000, breakdown.BasePrice);
            Assert.AreEqual(1.0f, breakdown.CategoryMultiplier, k_FloatTolerance);
            Assert.AreEqual(1.0f, breakdown.FollowerMultiplier, k_FloatTolerance);
            Assert.AreEqual(10_000, breakdown.FinalPrice);
        }

        [Test]
        public void Calculate_AppliesCategoryMultiplier()
        {
            InfluencerData influencer = NewInfluencer("inf_002", 50_000, 10_000, (CategoryId.Education, 5));
            IReadOnlyList<CategoryId> categories = new List<CategoryId> { CategoryId.Education };

            PriceBreakdown breakdown = m_Service.Calculate(influencer, categories, m_Config);

            Assert.AreEqual(1.5f, breakdown.CategoryMultiplier, k_FloatTolerance);
            Assert.AreEqual(15_000, breakdown.FinalPrice);
        }

        [Test]
        public void Calculate_AppliesFollowerMultiplier()
        {
            // 5_000_000 followers -> mega tier (2.0); score 3 -> 1.0.
            InfluencerData influencer = NewInfluencer("inf_003", 5_000_000, 10_000, (CategoryId.Education, 3));
            IReadOnlyList<CategoryId> categories = new List<CategoryId> { CategoryId.Education };

            PriceBreakdown breakdown = m_Service.Calculate(influencer, categories, m_Config);

            Assert.AreEqual(2.0f, breakdown.FollowerMultiplier, k_FloatTolerance);
            Assert.AreEqual(20_000, breakdown.FinalPrice);
        }

        [Test]
        public void Calculate_RoundsUpToNearestHundred()
        {
            // basePrice 9_801, multipliers 1.0/1.0 -> raw 9_801 -> ceil to 9_900.
            InfluencerData influencer = NewInfluencer("inf_004", 50_000, 9_801, (CategoryId.Education, 3));
            IReadOnlyList<CategoryId> categories = new List<CategoryId> { CategoryId.Education };

            PriceBreakdown breakdown = m_Service.Calculate(influencer, categories, m_Config);

            Assert.AreEqual(9_900, breakdown.FinalPrice);
        }

        [Test]
        public void Calculate_UsesAverageCategoryScore_ForMultipleCategories()
        {
            // scores 3 and 4 -> avg 3.5 -> ceil 4 -> category multiplier 1.2.
            InfluencerData influencer = NewInfluencer(
                "inf_005",
                50_000,
                10_000,
                (CategoryId.Education, 3),
                (CategoryId.Technology, 4));
            IReadOnlyList<CategoryId> categories = new List<CategoryId> { CategoryId.Education, CategoryId.Technology };

            PriceBreakdown breakdown = m_Service.Calculate(influencer, categories, m_Config);

            Assert.AreEqual(1.2f, breakdown.CategoryMultiplier, k_FloatTolerance);
            Assert.AreEqual(12_000, breakdown.FinalPrice);
        }

        [Test]
        public void Calculate_ReturnsZero_WhenBasePriceIsZero()
        {
            InfluencerData influencer = NewInfluencer("inf_006", 50_000, 0, (CategoryId.Education, 5));
            IReadOnlyList<CategoryId> categories = new List<CategoryId> { CategoryId.Education };

            PriceBreakdown breakdown = m_Service.Calculate(influencer, categories, m_Config);

            Assert.AreEqual(0, breakdown.BasePrice);
            Assert.AreEqual(0, breakdown.FinalPrice);
        }

        [Test]
        public void Calculate_HandlesEdge_NanoTier_ZeroFollowers()
        {
            // 0 followers -> nano tier (0.5).
            InfluencerData influencer = NewInfluencer("inf_007", 0, 10_000, (CategoryId.Education, 3));
            IReadOnlyList<CategoryId> categories = new List<CategoryId> { CategoryId.Education };

            PriceBreakdown breakdown = m_Service.Calculate(influencer, categories, m_Config);

            Assert.AreEqual(0.5f, breakdown.FollowerMultiplier, k_FloatTolerance);
            Assert.AreEqual(5_000, breakdown.FinalPrice);
        }

        [Test]
        public void Calculate_HandlesEdge_MegaTier_TenMillionFollowers()
        {
            InfluencerData influencer = NewInfluencer("inf_008", 10_000_000, 10_000, (CategoryId.Education, 3));
            IReadOnlyList<CategoryId> categories = new List<CategoryId> { CategoryId.Education };

            PriceBreakdown breakdown = m_Service.Calculate(influencer, categories, m_Config);

            Assert.AreEqual(2.0f, breakdown.FollowerMultiplier, k_FloatTolerance);
            Assert.AreEqual(20_000, breakdown.FinalPrice);
        }

        [Test]
        public void Calculate_FollowersAtTierUpperBoundary_UsesUpperTier()
        {
            // Tiers: micro [1000, 10000), medium [10000, 100000). 10_000 is exclusive of micro,
            // inclusive of medium → follower multiplier 1.0.
            InfluencerData influencer = NewInfluencer("inf_tier_upper", 10_000, 10_000, (CategoryId.Education, 3));
            IReadOnlyList<CategoryId> categories = new List<CategoryId> { CategoryId.Education };

            PriceBreakdown breakdown = m_Service.Calculate(influencer, categories, m_Config);

            Assert.AreEqual(1.0f, breakdown.FollowerMultiplier, k_FloatTolerance);
        }

        [Test]
        public void Calculate_FollowersAtTierLowerBoundary_UsesUpperTier()
        {
            // 1_000 is the boundary between nano [0,1000) and micro [1000,10000). Inclusive of micro → 0.8.
            InfluencerData influencer = NewInfluencer("inf_tier_lower", 1_000, 10_000, (CategoryId.Education, 3));
            IReadOnlyList<CategoryId> categories = new List<CategoryId> { CategoryId.Education };

            PriceBreakdown breakdown = m_Service.Calculate(influencer, categories, m_Config);

            Assert.AreEqual(0.8f, breakdown.FollowerMultiplier, k_FloatTolerance);
        }

        [Test]
        public void Calculate_EmptyCategories_UsesNeutralCategoryMultiplier()
        {
            // Empty selection short-circuits to the neutral 1.0 multiplier (ResolveCategoryMultiplier).
            InfluencerData influencer = NewInfluencer("inf_no_cats", 50_000, 10_000);
            IReadOnlyList<CategoryId> categories = new List<CategoryId>();

            PriceBreakdown breakdown = m_Service.Calculate(influencer, categories, m_Config);

            Assert.AreEqual(1.0f, breakdown.CategoryMultiplier, k_FloatTolerance);
            Assert.AreEqual(10_000, breakdown.FinalPrice);
        }

        [Test]
        public void Calculate_RoundingAtExactStep_DoesNotCeilUp()
        {
            // basePrice 10_000 × 1.0 × 1.0 = 10_000 — an exact 100-multiple, no ceil applies.
            InfluencerData influencer = NewInfluencer("inf_exact_step", 50_000, 10_000, (CategoryId.Education, 3));
            IReadOnlyList<CategoryId> categories = new List<CategoryId> { CategoryId.Education };

            PriceBreakdown breakdown = m_Service.Calculate(influencer, categories, m_Config);

            Assert.AreEqual(10_000, breakdown.FinalPrice);
        }

        [Test]
        public void Calculate_AverageScore_CeilsToIntegerMultiplierBucket()
        {
            // scores 3 and 3 -> avg 3.0 -> ceil 3 -> multiplier 1.0 (the existing test for
            // 3,4 covers ceil from 3.5; this verifies an exact integer avg stays at its bucket).
            InfluencerData influencer = NewInfluencer(
                "inf_avg_int",
                50_000,
                10_000,
                (CategoryId.Education, 3),
                (CategoryId.Technology, 3));
            IReadOnlyList<CategoryId> categories = new List<CategoryId> { CategoryId.Education, CategoryId.Technology };

            PriceBreakdown breakdown = m_Service.Calculate(influencer, categories, m_Config);

            Assert.AreEqual(1.0f, breakdown.CategoryMultiplier, k_FloatTolerance);
        }
    }
}
