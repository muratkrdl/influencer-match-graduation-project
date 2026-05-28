using System;
using Final.InfluencerMatch.Recommendation;

namespace Final.InfluencerMatch.Common
{
    /// <summary>
    /// Immutable result of ranking a single influencer against the current selection.
    /// </summary>
    public readonly struct ScoredInfluencer
    {
        private const float k_MinScore = 0f;
        private const float k_MaxScore = 1f;
        private const int k_MaxPercent = 100;

        public readonly InfluencerData Influencer;
        public readonly float HybridScore;
        public readonly bool IsOverBudget;
        public readonly int CompatibilityPercent;

        public ScoredInfluencer(
            InfluencerData influencer,
            float hybridScore,
            bool isOverBudget)
        {
            float clampedHybrid = hybridScore;
            if (clampedHybrid < k_MinScore)
            {
                clampedHybrid = k_MinScore;
            }
            else if (clampedHybrid > k_MaxScore)
            {
                clampedHybrid = k_MaxScore;
            }

            Influencer = influencer;
            HybridScore = clampedHybrid;
            IsOverBudget = isOverBudget;

            int percent = (int)Math.Round(clampedHybrid * 100.0, MidpointRounding.AwayFromZero);
            if (percent < 0)
            {
                percent = 0;
            }
            else if (percent > k_MaxPercent)
            {
                percent = k_MaxPercent;
            }

            CompatibilityPercent = percent;
        }
    }
}
