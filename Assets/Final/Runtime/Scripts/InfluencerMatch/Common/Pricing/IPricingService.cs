using System.Collections.Generic;
using Final.InfluencerMatch.Recommendation;

namespace Final.InfluencerMatch.Common
{
    /// <summary>
    /// Calculates the final price an influencer charges given selected categories and the active matching configuration.
    /// </summary>
    public interface IPricingService
    {
        PriceBreakdown Calculate(
            InfluencerData influencer,
            IReadOnlyList<CategoryId> selectedCategories,
            MatchingConfig config);
    }
}
