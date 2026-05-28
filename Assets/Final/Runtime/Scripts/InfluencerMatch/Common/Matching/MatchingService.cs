using System;
using System.Collections.Generic;
using Final.InfluencerMatch.Recommendation;

namespace Final.InfluencerMatch.Common
{
    /// <summary>
    /// Concrete matching service with single-pass scoring and deterministic tie-break sort.
    /// </summary>
    public class MatchingService : IMatchingService
    {
        private const float k_MaxRawCategoryScore = 5f;

        private static readonly ScoredInfluencerComparer s_Comparer = new ScoredInfluencerComparer();
        private static readonly ScoredInfluencer[] s_EmptyResult = Array.Empty<ScoredInfluencer>();

        private readonly List<ScoredInfluencer> m_ResultsCache = new List<ScoredInfluencer>();

        public IReadOnlyList<ScoredInfluencer> Rank(
            IReadOnlyList<CategoryId> selectedCategories,
            decimal budget,
            IReadOnlyList<InfluencerData> pool,
            MatchingConfig config,
            IPricingService pricing)
        {
            if (selectedCategories.Count == 0)
            {
                throw new ArgumentException("Selected categories must contain at least one entry.", nameof(selectedCategories));
            }

            int poolCount = pool.Count;
            if (poolCount == 0)
            {
                return s_EmptyResult;
            }

            m_ResultsCache.Clear();
            int categoryCount = selectedCategories.Count;
            int minScoreThreshold = config.MinimumCategoryScoreToInclude;
            float categoryWeight = config.CategoryWeight;
            float followersWeight = config.FollowersWeight;
            float engagementWeight = config.EngagementWeight;
            float overBudgetPenalty = config.OverBudgetPenalty;
            int normMin = config.FollowerNormalizationMin;
            int normMax = config.FollowerNormalizationMax;

            for (int i = 0; i < poolCount; i++)
            {
                InfluencerData influencer = pool[i];

                int scoreSum = 0;
                for (int c = 0; c < categoryCount; c++)
                {
                    scoreSum += influencer.GetScoreFor(selectedCategories[c]);
                }

                float averageCategoryScore = (float)scoreSum / categoryCount;
                if (averageCategoryScore < minScoreThreshold)
                {
                    continue;
                }

                float normCategory = averageCategoryScore / k_MaxRawCategoryScore;
                float normFollowers = FollowerNormalizer.Normalize(influencer.Followers, normMin, normMax);
                float normEngagement = influencer.EngagementRate;

                float rawScore = categoryWeight * normCategory
                                 + followersWeight * normFollowers
                                 + engagementWeight * normEngagement;

                PriceBreakdown breakdown = pricing.Calculate(influencer, selectedCategories, config);
                bool isOverBudget = breakdown.FinalPrice > budget;

                float finalScore = isOverBudget ? rawScore * overBudgetPenalty : rawScore;

                ScoredInfluencer scored = new ScoredInfluencer(influencer, finalScore, isOverBudget);

                m_ResultsCache.Add(scored);
            }

            m_ResultsCache.Sort(s_Comparer);
            return m_ResultsCache;
        }

        private readonly struct ScoredInfluencerComparer : IComparer<ScoredInfluencer>
        {
            public int Compare(ScoredInfluencer x, ScoredInfluencer y)
            {
                if (x.IsOverBudget != y.IsOverBudget)
                {
                    return x.IsOverBudget ? 1 : -1;
                }

                int primary = y.HybridScore.CompareTo(x.HybridScore);
                if (primary != 0)
                {
                    return primary;
                }

                return x.Influencer.Id.CompareTo(y.Influencer.Id);
            }
        }
    }
}
