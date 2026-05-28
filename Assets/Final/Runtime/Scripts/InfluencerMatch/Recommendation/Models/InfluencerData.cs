using System.Collections.Generic;
using Final.InfluencerMatch.Common;
using UnityEngine;

namespace Final.InfluencerMatch.Recommendation
{
    /// <summary>
    /// Static authored data for a single influencer.
    /// </summary>
    [CreateAssetMenu(menuName = GlobalEnvironmentVariables.AppName + "/Influencer", fileName = "Influencer_", order = 100)]
    public class InfluencerData : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private SerializableGuid m_Id = SerializableGuid.NewGuid();
        [SerializeField] private string m_DisplayName = string.Empty;
        [SerializeField] private string m_Handle = string.Empty;
        [SerializeField] private string m_Email = string.Empty;

        [Header("Profile")]
        [SerializeField] private Sprite m_Avatar;
        [SerializeField, TextArea(2, 4)] private string m_ShortBio = string.Empty;

        [Header("Metrics")]
        [SerializeField, Min(0)] private int m_Followers;
        [SerializeField, Range(0f, 1f)] private float m_EngagementRate = 0.05f;

        [Header("Pricing")]
        [SerializeField, Min(0)] private int m_BasePrice;

        [Header("Categories")]
        [SerializeField] private List<CategoryScoreEntry> m_CategoryScores = new List<CategoryScoreEntry>();

        [Header("Platforms")]
        [SerializeField] private List<PlatformId> m_ActivePlatforms = new List<PlatformId>();

        public SerializableGuid Id => m_Id;
        public string DisplayName => m_DisplayName;
        public string Handle => m_Handle;
        public string Email => m_Email;
        public Sprite Avatar => m_Avatar;
        public string ShortBio => m_ShortBio;
        public int Followers => m_Followers;
        public float EngagementRate => m_EngagementRate;
        public int BasePrice => m_BasePrice;
        public IReadOnlyList<CategoryScoreEntry> CategoryScores => m_CategoryScores;
        public IReadOnlyList<PlatformId> ActivePlatforms => m_ActivePlatforms;

        public int GetScoreFor(CategoryId id)
        {
            for (int i = 0; i < m_CategoryScores.Count; i++)
            {
                if (m_CategoryScores[i].Category == id)
                {
                    return m_CategoryScores[i].Score;
                }
            }

            return 0;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (m_Id.IsEmpty)
            {
                m_Id = SerializableGuid.NewGuid();
                UnityEditor.EditorUtility.SetDirty(this);
            }

            if (!string.IsNullOrEmpty(m_Handle) && !m_Handle.StartsWith("@"))
            {
                m_Handle = "@" + m_Handle;
            }
        }
#endif
    }
}
