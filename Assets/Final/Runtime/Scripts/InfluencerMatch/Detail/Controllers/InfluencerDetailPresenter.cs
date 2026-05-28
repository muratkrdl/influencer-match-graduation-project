using System;
using System.Collections.Generic;
using Final.InfluencerMatch.Common;
using Final.InfluencerMatch.Recommendation;

namespace Final.InfluencerMatch.Detail
{
    /// <summary>
    /// Pure presentation logic for the influencer detail panel.
    /// </summary>
    public class InfluencerDetailPresenter
    {
        private const int k_SinglePoolCapacity = 1;

        private readonly IMatchingService m_MatchingService;
        private readonly IPricingService m_PricingService;
        private readonly InfluencerDatabase m_Database;
        private readonly MatchingConfig m_MatchingConfig;
        private readonly PlatformConfig m_PlatformConfig;

        private readonly List<InfluencerData> m_SingleInfluencerPool = new List<InfluencerData>(k_SinglePoolCapacity);
        private readonly List<PlatformDefinition> m_ResolvedPlatforms = new List<PlatformDefinition>();

        public InfluencerDetailPresenter(
            IMatchingService matchingService,
            IPricingService pricingService,
            InfluencerDatabase database,
            MatchingConfig matchingConfig,
            PlatformConfig platformConfig)
        {
            m_MatchingService = matchingService;
            m_PricingService = pricingService;
            m_Database = database;
            m_MatchingConfig = matchingConfig;
            m_PlatformConfig = platformConfig;
        }

        public InfluencerDetailViewModel Build(
            SerializableGuid influencerId,
            IReadOnlyList<CategoryId> selectedCategories,
            decimal budget)
        {
            if (!m_Database.TryFindById(influencerId, out InfluencerData influencer))
            {
                throw new ArgumentException("Influencer not found.", nameof(influencerId));
            }

            int compatibilityPercent = ComputeCompatibilityPercent(influencer, selectedCategories, budget);
            int engagementCount = ComputeEngagementCount(influencer);
            int finalPrice = m_PricingService.Calculate(influencer, selectedCategories, m_MatchingConfig).FinalPrice;
            IReadOnlyList<PlatformDefinition> activePlatforms = ResolveActivePlatforms(influencer);

            return new InfluencerDetailViewModel(influencer, compatibilityPercent, engagementCount, finalPrice, activePlatforms);
        }

        private static int ComputeEngagementCount(InfluencerData influencer)
        {
            double count = influencer.Followers * (double)influencer.EngagementRate;
            return (int)Math.Round(count, MidpointRounding.AwayFromZero);
        }

        private int ComputeCompatibilityPercent(InfluencerData influencer, IReadOnlyList<CategoryId> selectedCategories, decimal budget)
        {
            if (selectedCategories.Count == 0)
            {
                return 0;
            }

            m_SingleInfluencerPool.Clear();
            m_SingleInfluencerPool.Add(influencer);

            IReadOnlyList<ScoredInfluencer> ranked = m_MatchingService.Rank(
                selectedCategories, budget, m_SingleInfluencerPool, m_MatchingConfig, m_PricingService);

            return ranked.Count == 0 ? 0 : ranked[0].CompatibilityPercent;
        }

        private IReadOnlyList<PlatformDefinition> ResolveActivePlatforms(InfluencerData influencer)
        {
            m_ResolvedPlatforms.Clear();
            IReadOnlyList<PlatformId> ids = influencer.ActivePlatforms;
            for (int i = 0; i < ids.Count; i++)
            {
                if (m_PlatformConfig.TryGetDefinition(ids[i], out PlatformDefinition definition))
                {
                    m_ResolvedPlatforms.Add(definition);
                }
            }
            return m_ResolvedPlatforms;
        }
    }
}
