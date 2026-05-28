using System;
using UnityEngine;

namespace Final.InfluencerMatch.Recommendation
{
    /// <summary>
    /// Maps a rounded category score (1-5) to a price multiplier.
    /// </summary>
    [Serializable]
    public struct CategoryScoreMultiplier
    {
        [SerializeField, Range(1, 5)] private int m_Score;
        [SerializeField, Min(0f)] private float m_Multiplier;

        public int Score => m_Score;
        public float Multiplier => m_Multiplier;
    }
}
