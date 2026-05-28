using DG.Tweening;
using Final.InfluencerMatch.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Final.InfluencerMatch.Budget
{
    /// <summary>
    /// Single category toggle entry in the category list.
    /// </summary>
    public class CategoryToggleView : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Toggle m_Toggle;
        [SerializeField] private TMP_Text m_LabelText;
        [SerializeField] private Image m_AccentImage;
        [SerializeField] private Image m_IconImage;

        [Inject] private BudgetConfig m_Config;

        private CategoryId m_CategoryId = CategoryId.None;
        private Tween m_BounceTween;

        public event ToggleChangedHandler ToggleChanged;

        public CategoryId CategoryId => m_CategoryId;

        private void Awake()
        {
            m_Toggle.onValueChanged.AddListener(HandleToggleChanged);
        }

        private void OnDestroy()
        {
            m_Toggle.onValueChanged.RemoveListener(HandleToggleChanged);
            m_BounceTween?.Kill();
            m_BounceTween = null;
        }

        public void Bind(CategoryDefinition definition)
        {
            m_CategoryId = definition.Id;

            m_LabelText.text = definition.DisplayName;
            m_AccentImage.color = definition.AccentColor;

            m_IconImage.sprite = definition.Icon;
            m_IconImage.enabled = true;

            m_Toggle.SetIsOnWithoutNotify(false);
        }

        public void SetIsOn(bool isOn)
        {
            m_Toggle.SetIsOnWithoutNotify(isOn);
        }

        private void HandleToggleChanged(bool isOn)
        {
            PlayBounce();
            ToggleChanged?.Invoke(m_CategoryId, isOn);
        }

        private void PlayBounce()
        {
            m_BounceTween?.Kill();
            m_BounceTween = m_Toggle.transform
                .DOPunchScale(
                    Vector3.one * m_Config.TogglePunchScale,
                    m_Config.ToggleBounceDuration,
                    m_Config.ToggleBounceVibrato,
                    m_Config.ToggleBounceElasticity)
                .SetLink(m_Toggle.gameObject);
        }

        public delegate void ToggleChangedHandler(CategoryId id, bool isOn);
    }
}
