using Final.InfluencerMatch.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Final.InfluencerMatch.EmptyState
{
    /// <summary>
    /// Panel view shown when matching yields zero recommendations, exposing a change-filters event.
    /// </summary>
    public class EmptyStateView : UIPanelBase
    {
        [Header("Texts")]
        [SerializeField] private TMP_Text m_TitleText;
        [SerializeField] private TMP_Text m_DescriptionText;

        [Header("Actions")]
        [SerializeField] private Button m_ChangeFiltersButton;

        public event ChangeFiltersRequestedHandler ChangeFiltersRequested;

        private void Awake()
        {
            m_ChangeFiltersButton.onClick.AddListener(HandleChangeFiltersClicked);
        }

        private void OnDestroy()
        {
            m_ChangeFiltersButton.onClick.RemoveListener(HandleChangeFiltersClicked);
        }

        private void HandleChangeFiltersClicked()
        {
            ChangeFiltersRequested?.Invoke();
        }

        public delegate void ChangeFiltersRequestedHandler();
    }
}
