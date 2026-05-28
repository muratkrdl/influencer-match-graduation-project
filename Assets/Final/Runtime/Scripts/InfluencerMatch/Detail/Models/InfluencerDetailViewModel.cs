using System.Collections.Generic;
using Final.InfluencerMatch.Common;
using Final.InfluencerMatch.Recommendation;

namespace Final.InfluencerMatch.Detail
{
    /// <summary>
    /// Immutable presentation snapshot for the influencer detail panel.
    /// </summary>
    public readonly struct InfluencerDetailViewModel
    {
        public readonly InfluencerData Influencer;
        public readonly int CompatibilityPercent;
        public readonly int EngagementCount;
        public readonly int FinalPrice;
        public readonly IReadOnlyList<PlatformDefinition> ActivePlatforms;

        public InfluencerDetailViewModel(
            InfluencerData influencer,
            int compatibilityPercent,
            int engagementCount,
            int finalPrice,
            IReadOnlyList<PlatformDefinition> activePlatforms)
        {
            Influencer = influencer;
            CompatibilityPercent = compatibilityPercent;
            EngagementCount = engagementCount;
            FinalPrice = finalPrice;
            ActivePlatforms = activePlatforms;
        }
    }
}
