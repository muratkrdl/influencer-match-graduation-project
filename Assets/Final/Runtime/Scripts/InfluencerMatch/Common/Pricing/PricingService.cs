using System;
using System.Collections.Generic;
using Final.InfluencerMatch.Recommendation;

namespace Final.InfluencerMatch.Common
{
    /// <summary>
    /// Concrete pricing service applying category-score and follower-tier multipliers with step rounding.
    /// </summary>
    public class PricingService : IPricingService
    {
        private const float k_NeutralMultiplier = 1.0f;
        private const int k_MinCategoryScore = 1;
        private const int k_MaxCategoryScore = 5;

        public PriceBreakdown Calculate(
            InfluencerData influencer,
            IReadOnlyList<CategoryId> selectedCategories,
            MatchingConfig config)
        {
            int basePrice = influencer.BasePrice;
            if (basePrice <= 0)
            {
                return new PriceBreakdown(0, k_NeutralMultiplier, k_NeutralMultiplier, 0);
            }

            float categoryMultiplier = ResolveCategoryMultiplier(influencer, selectedCategories, config);
            float followerMultiplier = config.GetFollowerTier(influencer.Followers).Multiplier;

            double rawPrice = (double)basePrice * categoryMultiplier * followerMultiplier;
            rawPrice = Math.Round(rawPrice, 2, MidpointRounding.AwayFromZero);

            int rounding = config.PriceRoundingTL;
            if (rounding < 1)
            {
                rounding = 1;
            }

            double scaled = rawPrice / rounding;
            double rounded = Math.Ceiling(scaled);
            int finalPrice = (int)(rounded * rounding);
            if (finalPrice < 0)
            {
                finalPrice = 0;
            }

            return new PriceBreakdown(basePrice, categoryMultiplier, followerMultiplier, finalPrice);
        }

        private static float ResolveCategoryMultiplier(
            InfluencerData influencer,
            IReadOnlyList<CategoryId> selectedCategories,
            MatchingConfig config)
        {
            if (selectedCategories == null || selectedCategories.Count == 0)
            {
                return k_NeutralMultiplier;
            }

            int sum = 0;
            int count = selectedCategories.Count;
            for (int i = 0; i < count; i++)
            {
                sum += influencer.GetScoreFor(selectedCategories[i]);
            }

            double rawAvg = (double)sum / count;
            int avgScoreCeil = (int)Math.Ceiling(rawAvg);
            if (avgScoreCeil < k_MinCategoryScore)
            {
                avgScoreCeil = k_MinCategoryScore;
            }
            else if (avgScoreCeil > k_MaxCategoryScore)
            {
                avgScoreCeil = k_MaxCategoryScore;
            }

            return config.GetCategoryMultiplier(avgScoreCeil);
        }
    }
}
