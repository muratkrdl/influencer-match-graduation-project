using System.Collections.Generic;
using Final.InfluencerMatch.Recommendation;

namespace Final.InfluencerMatch.Common
{
    /// <summary>
    /// Ranks an influencer pool against the user's selected categories and budget.
    /// </summary>
    public interface IMatchingService
    {
        IReadOnlyList<ScoredInfluencer> Rank(
            IReadOnlyList<CategoryId> selectedCategories,
            decimal budget,
            IReadOnlyList<InfluencerData> pool,
            MatchingConfig config,
            IPricingService pricing);
    }
}
