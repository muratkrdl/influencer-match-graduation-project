using System;
using System.Collections.Generic;
using System.Diagnostics;
using Final.InfluencerMatch.Common;
using Final.InfluencerMatch.Recommendation;
using Final.Tests.Helpers;
using NUnit.Framework;
using UnityEngine;

namespace Final.Tests.Common
{
    [TestFixture]
    public sealed class MatchingServiceTests
    {
        private const float k_FloatTolerance = 0.01f;

        private MatchingService m_Service;
        private PricingService m_Pricing;
        private MatchingConfig m_Config;
        private List<InfluencerData> m_Created;

        [SetUp]
        public void SetUp()
        {
            m_Service = new MatchingService();
            m_Pricing = new PricingService();
            m_Config = TestDataFactory.CreateDefaultMatchingConfig();
            m_Created = new List<InfluencerData>();
        }

        [TearDown]
        public void TearDown()
        {
            TestDataFactory.DestroyAll(m_Created);
            if (m_Config != null)
            {
                UnityEngine.Object.DestroyImmediate(m_Config);
                m_Config = null;
            }
        }

        private InfluencerData NewInfluencer(
            string id,
            int followers,
            float engagement,
            int basePrice,
            params (CategoryId, int)[] scores)
        {
            InfluencerData data = TestDataFactory.CreateInfluencer(id, id, followers, engagement, basePrice, scores);
            m_Created.Add(data);
            return data;
        }

        [Test]
        public void Rank_ReturnsEmpty_WhenPoolIsEmpty()
        {
            List<InfluencerData> pool = new List<InfluencerData>();
            List<CategoryId> categories = new List<CategoryId> { CategoryId.Education };

            IReadOnlyList<ScoredInfluencer> result = m_Service.Rank(categories, 10_000m, pool, m_Config, m_Pricing);

            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void Rank_FiltersOut_ZeroScoreInfluencers()
        {
            InfluencerData zero = NewInfluencer("inf_zero", 50_000, 0.05f, 5_000, (CategoryId.Education, 0));
            InfluencerData good = NewInfluencer("inf_good", 50_000, 0.05f, 5_000, (CategoryId.Education, 4));
            List<InfluencerData> pool = new List<InfluencerData> { zero, good };
            List<CategoryId> categories = new List<CategoryId> { CategoryId.Education };

            IReadOnlyList<ScoredInfluencer> result = m_Service.Rank(categories, 100_000m, pool, m_Config, m_Pricing);

            Assert.AreEqual(1, result.Count);
            Assert.AreSame(good, result[0].Influencer);
        }

        [Test]
        public void Rank_OrdersByHybridScore_Descending()
        {
            InfluencerData low = NewInfluencer("inf_low", 5_000, 0.02f, 5_000, (CategoryId.Education, 2));
            InfluencerData high = NewInfluencer("inf_high", 500_000, 0.08f, 5_000, (CategoryId.Education, 5));
            InfluencerData mid = NewInfluencer("inf_mid", 50_000, 0.05f, 5_000, (CategoryId.Education, 3));
            List<InfluencerData> pool = new List<InfluencerData> { low, high, mid };
            List<CategoryId> categories = new List<CategoryId> { CategoryId.Education };

            IReadOnlyList<ScoredInfluencer> result = m_Service.Rank(categories, 100_000m, pool, m_Config, m_Pricing);

            Assert.AreEqual(3, result.Count);
            Assert.GreaterOrEqual(result[0].HybridScore, result[1].HybridScore);
            Assert.GreaterOrEqual(result[1].HybridScore, result[2].HybridScore);
            Assert.AreSame(high, result[0].Influencer);
        }

        [Test]
        public void Rank_IsDeterministic_OnSameInputs()
        {
            List<InfluencerData> pool = new List<InfluencerData>
            {
                NewInfluencer("inf_a", 50_000, 0.05f, 5_000, (CategoryId.Education, 4)),
                NewInfluencer("inf_b", 200_000, 0.07f, 5_000, (CategoryId.Education, 3)),
                NewInfluencer("inf_c", 10_000, 0.04f, 5_000, (CategoryId.Education, 5))
            };
            List<CategoryId> categories = new List<CategoryId> { CategoryId.Education };

            IReadOnlyList<ScoredInfluencer> reference = m_Service.Rank(categories, 100_000m, pool, m_Config, m_Pricing);
            InfluencerData[] referenceOrder = new InfluencerData[reference.Count];
            for (int i = 0; i < reference.Count; i++)
            {
                referenceOrder[i] = reference[i].Influencer;
            }

            for (int run = 0; run < 100; run++)
            {
                IReadOnlyList<ScoredInfluencer> run_n = m_Service.Rank(categories, 100_000m, pool, m_Config, m_Pricing);
                Assert.AreEqual(referenceOrder.Length, run_n.Count);
                for (int i = 0; i < run_n.Count; i++)
                {
                    Assert.AreSame(referenceOrder[i], run_n[i].Influencer, "Run " + run + " diverged at index " + i);
                }
            }
        }

        [Test]
        public void Rank_TieBreaker_IsDeterministic()
        {
            // Two influencers with identical inputs -> same score.
            // Tie-break is by SerializableGuid CompareTo; the actual order is
            // GUID-dependent but must be stable across repeated calls.
            InfluencerData zeta = NewInfluencer("inf_zeta", 50_000, 0.05f, 5_000, (CategoryId.Education, 4));
            InfluencerData alpha = NewInfluencer("inf_alpha", 50_000, 0.05f, 5_000, (CategoryId.Education, 4));
            List<InfluencerData> pool = new List<InfluencerData> { zeta, alpha };
            List<CategoryId> categories = new List<CategoryId> { CategoryId.Education };

            IReadOnlyList<ScoredInfluencer> result1 = m_Service.Rank(categories, 100_000m, pool, m_Config, m_Pricing);
            Assert.AreEqual(2, result1.Count);
            Assert.AreEqual(result1[0].HybridScore, result1[1].HybridScore, 1e-6f);

            InfluencerData first = result1[0].Influencer;
            InfluencerData second = result1[1].Influencer;

            for (int run = 0; run < 50; run++)
            {
                IReadOnlyList<ScoredInfluencer> run_n = m_Service.Rank(categories, 100_000m, pool, m_Config, m_Pricing);
                Assert.AreSame(first, run_n[0].Influencer, "Run " + run + " tie-break diverged at index 0");
                Assert.AreSame(second, run_n[1].Influencer, "Run " + run + " tie-break diverged at index 1");
            }
        }

        [Test]
        public void Rank_AppliesOverBudgetPenalty_WhenPriceExceedsBudget()
        {
            // Two influencers, identical scoring inputs but vastly different base prices.
            // A's price is within budget; B's exceeds budget -> B gets HybridScore * 0.5.
            InfluencerData cheap = NewInfluencer("inf_a_cheap", 50_000, 0.05f, 5_000, (CategoryId.Education, 4));
            InfluencerData expensive = NewInfluencer("inf_b_expensive", 50_000, 0.05f, 200_000, (CategoryId.Education, 4));
            List<InfluencerData> pool = new List<InfluencerData> { cheap, expensive };
            List<CategoryId> categories = new List<CategoryId> { CategoryId.Education };

            IReadOnlyList<ScoredInfluencer> result = m_Service.Rank(categories, 10_000m, pool, m_Config, m_Pricing);

            Assert.AreEqual(2, result.Count);
            Assert.AreSame(cheap, result[0].Influencer);
            Assert.AreSame(expensive, result[1].Influencer);
            Assert.AreEqual(result[0].HybridScore * 0.5f, result[1].HybridScore, k_FloatTolerance);
        }

        [Test]
        public void Rank_MarksIsOverBudgetTrue_WhenPriceExceedsBudget()
        {
            List<InfluencerData> pool = new List<InfluencerData>
            {
                NewInfluencer("inf_expensive", 50_000, 0.05f, 200_000, (CategoryId.Education, 4))
            };
            List<CategoryId> categories = new List<CategoryId> { CategoryId.Education };

            IReadOnlyList<ScoredInfluencer> result = m_Service.Rank(categories, 10_000m, pool, m_Config, m_Pricing);

            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(result[0].IsOverBudget);
        }

        [Test]
        public void Rank_CompatibilityPercent_EqualsHybridScoreTimesHundred()
        {
            List<InfluencerData> pool = new List<InfluencerData>
            {
                NewInfluencer("inf_x", 100_000, 0.05f, 5_000, (CategoryId.Education, 5))
            };
            List<CategoryId> categories = new List<CategoryId> { CategoryId.Education };

            IReadOnlyList<ScoredInfluencer> result = m_Service.Rank(categories, 100_000m, pool, m_Config, m_Pricing);

            Assert.AreEqual(1, result.Count);
            int expected = (int)Math.Round(result[0].HybridScore * 100.0, MidpointRounding.AwayFromZero);
            Assert.AreEqual(expected, result[0].CompatibilityPercent);
        }

        [Test]
        public void Rank_ThrowsArgumentException_WhenSelectedCategoriesIsEmpty()
        {
            List<InfluencerData> pool = new List<InfluencerData>();
            List<CategoryId> categories = new List<CategoryId>();

            Assert.Throws<ArgumentException>(() => m_Service.Rank(categories, 10_000m, pool, m_Config, m_Pricing));
        }

        [Test]
        public void Rank_IncludesInfluencer_WhenScoreEqualsMinimumThreshold()
        {
            // Default config: MinimumCategoryScoreToInclude = 1. The filter is
            // `averageCategoryScore < minScoreThreshold`, so score == threshold is INCLUDED.
            InfluencerData borderline = NewInfluencer("inf_at_threshold", 50_000, 0.05f, 5_000, (CategoryId.Education, 1));
            List<InfluencerData> pool = new List<InfluencerData> { borderline };
            List<CategoryId> categories = new List<CategoryId> { CategoryId.Education };

            IReadOnlyList<ScoredInfluencer> result = m_Service.Rank(categories, 100_000m, pool, m_Config, m_Pricing);

            Assert.AreEqual(1, result.Count);
            Assert.AreSame(borderline, result[0].Influencer);
        }

        [Test]
        public void Rank_AllPoolOverBudget_AllMarkedIsOverBudget_AndSortedByPenalisedScore()
        {
            // Budget too low to afford anyone. All influencers receive the over-budget penalty
            // but must still be sorted deterministically (HybridScore desc, tiebreak by Id).
            InfluencerData topScore = NewInfluencer("inf_top", 50_000, 0.05f, 200_000, (CategoryId.Education, 5));
            InfluencerData lowScore = NewInfluencer("inf_low", 50_000, 0.05f, 200_000, (CategoryId.Education, 3));
            InfluencerData midScore = NewInfluencer("inf_mid", 50_000, 0.05f, 200_000, (CategoryId.Education, 4));
            List<InfluencerData> pool = new List<InfluencerData> { topScore, lowScore, midScore };
            List<CategoryId> categories = new List<CategoryId> { CategoryId.Education };

            IReadOnlyList<ScoredInfluencer> result = m_Service.Rank(categories, 1_000m, pool, m_Config, m_Pricing);

            Assert.AreEqual(3, result.Count);
            for (int i = 0; i < result.Count; i++)
            {
                Assert.IsTrue(result[i].IsOverBudget, "Index " + i + " should be over-budget.");
            }
            Assert.GreaterOrEqual(result[0].HybridScore, result[1].HybridScore);
            Assert.GreaterOrEqual(result[1].HybridScore, result[2].HybridScore);
            // Highest raw score should still bubble to the top within the over-budget bucket.
            Assert.AreSame(topScore, result[0].Influencer);
        }

        [Test]
        public void Rank_PerformanceTest_100Influencers3Categories_Under50ms()
        {
            List<InfluencerData> pool = TestDataFactory.CreatePool(100);
            m_Created.AddRange(pool);
            List<CategoryId> categories = new List<CategoryId>
            {
                CategoryId.Education,
                CategoryId.Technology,
                CategoryId.Sports
            };

            Stopwatch stopwatch = Stopwatch.StartNew();
            IReadOnlyList<ScoredInfluencer> result = m_Service.Rank(categories, 100_000m, pool, m_Config, m_Pricing);
            stopwatch.Stop();

            Assert.Greater(result.Count, 0);
            Assert.Less(stopwatch.ElapsedMilliseconds, 50, "Rank took " + stopwatch.ElapsedMilliseconds + "ms");
        }
    }
}
