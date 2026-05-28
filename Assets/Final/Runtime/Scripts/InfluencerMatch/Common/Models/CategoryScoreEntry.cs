using System;
using UnityEngine;

namespace Final.InfluencerMatch.Common
{
    /// <summary>
    /// A single category score entry for an influencer.
    /// </summary>
    [Serializable]
    public struct CategoryScoreEntry
    {
        [SerializeField] private CategoryId m_Category;
        [SerializeField, Range(0, 5)] private int m_Score;

        public CategoryId Category => m_Category;
        public int Score => m_Score;

        public CategoryScoreEntry(CategoryId category, int score)
        {
            m_Category = category;
            m_Score = Mathf.Clamp(score, 0, 5);
        }
    }
}
