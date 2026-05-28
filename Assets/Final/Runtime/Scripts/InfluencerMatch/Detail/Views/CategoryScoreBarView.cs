using Final.InfluencerMatch.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Final.InfluencerMatch.Detail
{
    /// <summary>
    /// A single category score row inside the influencer detail page.
    /// </summary>
    public class CategoryScoreBarView : MonoBehaviour
    {
        [Header("Visual")]
        [SerializeField] private Image m_IconImage;
        [SerializeField] private TMP_Text m_LabelText;
        [SerializeField] private Image[] m_ScoreDots;
        [SerializeField] private Image m_HighlightBackground;

        [Inject] private ScoreBarConfig m_Config;

        public void Bind(CategoryDefinition definition, int influencerScore, bool isSelectedByUser)
        {
            BindIcon(definition);
            BindLabel(definition, isSelectedByUser);

            int clampedScore = influencerScore;
            if (clampedScore < 0)
            {
                clampedScore = 0;
            }
            else if (clampedScore > m_Config.MaxScore)
            {
                clampedScore = m_Config.MaxScore;
            }

            int dotsPerScorePoint = m_Config.DotCount / m_Config.MaxScore;
            int filledDots = clampedScore * dotsPerScorePoint;
            BindDots(filledDots);
            BindHighlight(isSelectedByUser);
        }

        private void BindIcon(CategoryDefinition definition)
        {
            m_IconImage.sprite = definition.Icon;
            m_IconImage.enabled = true;
        }

        private void BindLabel(CategoryDefinition definition, bool isSelectedByUser)
        {
            m_LabelText.text = definition.DisplayName;
            m_LabelText.fontStyle = isSelectedByUser ? FontStyles.Bold : FontStyles.Normal;
        }

        private void BindDots(int filledDots)
        {
            if (m_ScoreDots.Length != m_Config.DotCount)
            {
                Debug.LogError($"CategoryScoreBarView: m_ScoreDots must have exactly {m_Config.DotCount} elements.", this);
                return;
            }

            for (int i = 0; i < m_ScoreDots.Length; i++)
            {
                Image dot = m_ScoreDots[i];
                dot.color = i < filledDots ? m_Config.FilledColor : m_Config.EmptyColor;
            }
        }

        private void BindHighlight(bool isSelectedByUser)
        {
            m_HighlightBackground.enabled = isSelectedByUser;
            m_HighlightBackground.color = m_Config.HighlightColor;
        }
    }
}
