using Final.InfluencerMatch.Common;
using Final.Systems.ConfigManagement;
using UnityEngine;

namespace Final.InfluencerMatch.Detail
{
    /// <summary>
    /// Visual tunables for the per-category score bar used in the detail panel.
    /// </summary>
    [CreateAssetMenu(menuName = GlobalEnvironmentVariables.AppName + "/Score Bar Config", fileName = "ScoreBarConfig", order = 140)]
    public class ScoreBarConfig : ScriptableObject, IVisibleConfig
    {
        [Header("Dot Colors")]
        [SerializeField] private Color m_FilledColor = new Color(0.29f, 0.56f, 0.89f, 1f);
        [SerializeField] private Color m_EmptyColor = new Color(0.85f, 0.85f, 0.88f, 1f);
        [SerializeField] private Color m_HighlightColor = new Color(0.29f, 0.56f, 0.89f, 0.15f);

        [Header("Score Mapping")]
        [SerializeField, Min(1)] private int m_MaxScore = 5;
        [SerializeField, Min(1)] private int m_DotCount = 10;

        string IVisibleConfig.ConfigName => "Score Bar Config";
        string IVisibleConfig.Category => "Detail";

        public Color FilledColor => m_FilledColor;
        public Color EmptyColor => m_EmptyColor;
        public Color HighlightColor => m_HighlightColor;
        public int MaxScore => m_MaxScore;
        public int DotCount => m_DotCount;
    }
}
