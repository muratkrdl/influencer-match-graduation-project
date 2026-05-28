using System.Collections.Generic;
using Final.InfluencerMatch.Common;

namespace Final.InfluencerMatch.Recommendation
{
    /// <summary>
    /// Immutable presentation snapshot for the recommendation list panel. <see cref="Ranked"/> is null when there is nothing to rank (no categories selected); it is an empty list when the pipeline ran but produced no matches.
    /// </summary>
    public readonly struct RecommendationListViewModel
    {
        public readonly IReadOnlyList<ScoredInfluencer> Ranked;
        public readonly string Subtitle;

        public RecommendationListViewModel(IReadOnlyList<ScoredInfluencer> ranked, string subtitle)
        {
            Ranked = ranked;
            Subtitle = subtitle;
        }

        public int ResultCount => Ranked?.Count ?? 0;
    }
}
