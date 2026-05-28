using DG.Tweening;
using Final.InfluencerMatch.Common;
using UnityEngine;
using VContainer;

namespace Final.InfluencerMatch.UI
{
    /// <summary>
    /// Base class for UIManager-driven panels; implements the show/hide contract and entrance fade.
    /// </summary>
    public abstract class UIPanelBase : MonoBehaviour, IUIPanel
    {
        [SerializeField] private CanvasGroup m_CanvasGroup;

        [Inject] private UISharedConfig m_SharedConfig;

        private Tween m_FadeTween;

        void IUIPanel.Show()
        {
            gameObject.SetActive(true);
            PlayEntranceFade();
        }

        void IUIPanel.Hide()
        {
            gameObject.SetActive(false);
        }

        private void PlayEntranceFade()
        {
            m_FadeTween?.Kill();
            m_CanvasGroup.alpha = 0f;
            m_FadeTween = m_CanvasGroup
                .DOFade(1f, m_SharedConfig.PanelFadeInDuration)
                .SetEase(m_SharedConfig.PanelFadeInEase)
                .SetLink(gameObject);
        }
    }
}
