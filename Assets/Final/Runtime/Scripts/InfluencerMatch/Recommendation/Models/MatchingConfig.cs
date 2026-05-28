using System.Collections.Generic;
using Final.InfluencerMatch.Common;
using Final.Systems.ConfigManagement;
using UnityEngine;

namespace Final.InfluencerMatch.Recommendation
{
    /// <summary>
    /// Project-wide weights, normalization parameters, and price multiplier tables.
    /// </summary>
    [CreateAssetMenu(menuName = GlobalEnvironmentVariables.AppName + "/Matching Config", fileName = "MatchingConfig", order = 70)]
    public class MatchingConfig : ScriptableObject, IVisibleConfig
    {
        string IVisibleConfig.ConfigName => "Matching Config";
        string IVisibleConfig.Category => "Matching";

        [Header("Score Weights")]
        [SerializeField, Range(0f, 1f)] private float m_CategoryWeight = 0.5f;
        [SerializeField, Range(0f, 1f)] private float m_FollowersWeight = 0.3f;
        [SerializeField, Range(0f, 1f)] private float m_EngagementWeight = 0.2f;

        [Header("Follower Normalization")]
        [SerializeField, Min(0)] private int m_FollowerNormalizationMin = 1000;
        [SerializeField, Min(1)] private int m_FollowerNormalizationMax = 10_000_000;

        [Header("Pricing")]
        [SerializeField, Range(0f, 1f)] private float m_OverBudgetPenalty = 0.5f;
        [SerializeField] private List<CategoryScoreMultiplier> m_CategoryScoreMultipliers = new List<CategoryScoreMultiplier>();
        [SerializeField] private List<FollowerTierMultiplier> m_FollowerTierMultipliers = new List<FollowerTierMultiplier>();
        [SerializeField, Min(1)] private int m_PriceRoundingTL = 100;

        [Header("Ranking")]
        [SerializeField, Range(0, 5)] private int m_MinimumCategoryScoreToInclude = 1;

        public float CategoryWeight => m_CategoryWeight;
        public float FollowersWeight => m_FollowersWeight;
        public float EngagementWeight => m_EngagementWeight;
        public int FollowerNormalizationMin => m_FollowerNormalizationMin;
        public int FollowerNormalizationMax => m_FollowerNormalizationMax;
        public float OverBudgetPenalty => m_OverBudgetPenalty;
        public IReadOnlyList<CategoryScoreMultiplier> CategoryScoreMultipliers => m_CategoryScoreMultipliers;
        public IReadOnlyList<FollowerTierMultiplier> FollowerTierMultipliers => m_FollowerTierMultipliers;
        public int PriceRoundingTL => m_PriceRoundingTL;
        public int MinimumCategoryScoreToInclude => m_MinimumCategoryScoreToInclude;

        public float GetCategoryMultiplier(int score)
        {
            for (int i = 0; i < m_CategoryScoreMultipliers.Count; i++)
            {
                if (m_CategoryScoreMultipliers[i].Score == score)
                {
                    return m_CategoryScoreMultipliers[i].Multiplier;
                }
            }
            Debug.LogWarning($"MatchingConfig '{name}' has no CategoryScoreMultiplier entry for score {score}; defaulting to 1.0.", this);
            return 1.0f;
        }

        public FollowerTierMultiplier GetFollowerTier(int followers)
        {
            for (int i = 0; i < m_FollowerTierMultipliers.Count; i++)
            {
                if (m_FollowerTierMultipliers[i].Contains(followers))
                {
                    return m_FollowerTierMultipliers[i];
                }
            }

            Debug.LogWarning($"MatchingConfig '{name}' has no FollowerTierMultiplier covering {followers} followers; returning last tier.", this);
            return m_FollowerTierMultipliers.Count > 0
                ? m_FollowerTierMultipliers[m_FollowerTierMultipliers.Count - 1]
                : default;
        }
    }
}
