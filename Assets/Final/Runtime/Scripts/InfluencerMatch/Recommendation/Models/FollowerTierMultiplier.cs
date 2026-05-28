using System;
using UnityEngine;

namespace Final.InfluencerMatch.Recommendation
{
    /// <summary>
    /// A follower-count tier with an associated price multiplier.
    /// </summary>
    [Serializable]
    public struct FollowerTierMultiplier
    {
        [SerializeField, Min(0)] private int m_MinFollowersInclusive;
        [SerializeField, Min(0)] private int m_MaxFollowersExclusive;
        [SerializeField, Min(0f)] private float m_Multiplier;
        [SerializeField] private string m_Label;

        public int MinFollowersInclusive => m_MinFollowersInclusive;
        public int MaxFollowersExclusive => m_MaxFollowersExclusive;
        public float Multiplier => m_Multiplier;

        public bool Contains(int followers)
        {
            return followers >= m_MinFollowersInclusive && followers < m_MaxFollowersExclusive;
        }
    }
}
