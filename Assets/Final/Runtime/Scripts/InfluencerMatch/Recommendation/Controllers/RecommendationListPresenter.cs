using System.Collections.Generic;
using Final.InfluencerMatch.Common;

namespace Final.InfluencerMatch.Recommendation
{
    /// <summary>
    /// Pure presentation logic for the recommendation list.
    /// </summary>
    public class RecommendationListPresenter
    {
        private readonly IMatchingService m_MatchingService;
        private readonly IPricingService m_PricingService;
        private readonly MatchingConfig m_MatchingConfig;

        public RecommendationListPresenter(
            IMatchingService matchingService,
            IPricingService pricingService,
            MatchingConfig matchingConfig)
        {
            m_MatchingService = matchingService;
            m_PricingService = pricingService;
            m_MatchingConfig = matchingConfig;
        }

        public RecommendationListViewModel Build(
            IReadOnlyList<CategoryId> selectedCategories,
            decimal budget,
            IReadOnlyList<InfluencerData> pool)
        {
            if (selectedCategories.Count == 0)
            {
                return new RecommendationListViewModel(null, "No categories selected.");
            }

            IReadOnlyList<ScoredInfluencer> ranked = m_MatchingService.Rank(
                selectedCategories,
                budget,
                pool,
                m_MatchingConfig,
                m_PricingService);

            return new RecommendationListViewModel(ranked, FormatSubtitle(ranked.Count));
        }

        private static string FormatSubtitle(int count)
        {
            if (count == 0)
            {
                return "No influencers found";
            }

            if (count == 1)
            {
                return "1 influencer matches your criteria";
            }

            return StringUtils.GetNumberString(count) + " influencers match your criteria";
        }
    }
}
