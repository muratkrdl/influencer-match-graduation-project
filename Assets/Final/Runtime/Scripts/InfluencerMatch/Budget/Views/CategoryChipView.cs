using Final.InfluencerMatch.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Final.InfluencerMatch.Budget
{
    /// <summary>
    /// UI sub-component that renders a single category badge with a label.
    /// </summary>
    public class CategoryChipView : MonoBehaviour
    {
        [Header("Visual")]
        [SerializeField] private Image m_Background;
        [SerializeField] private TMP_Text m_LabelText;
        [SerializeField] private Image m_IconImage;

        public void Bind(CategoryDefinition definition)
        {
            m_LabelText.text = definition.DisplayName;

            m_IconImage.sprite = definition.Icon;
            m_IconImage.enabled = true;
        }

        public void SetAlpha(float alpha)
        {
            Color bg = m_Background.color;
            bg.a = alpha;
            m_Background.color = bg;

            Color label = m_LabelText.color;
            label.a = alpha;
            m_LabelText.color = label;

            Color icon = m_IconImage.color;
            icon.a = alpha;
            m_IconImage.color = icon;
        }
    }
}
