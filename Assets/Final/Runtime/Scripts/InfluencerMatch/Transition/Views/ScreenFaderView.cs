using System;
using DG.Tweening;
using UnityEngine;
using VContainer;

namespace Final.InfluencerMatch.Transition
{
    /// <summary>
    /// Persistent full-screen fade overlay that drives scene-transition fades.
    /// </summary>
    public class ScreenFaderView : MonoBehaviour, IScreenFader
    {
        [Header("References")]
        [SerializeField] private CanvasGroup m_CanvasGroup;

        [Inject] private ScreenFaderConfig m_Config;

        private Tween m_Tween;

        private void Awake()
        {
            m_CanvasGroup.alpha = 0f;
            m_CanvasGroup.blocksRaycasts = false;
        }

        private void OnDestroy()
        {
            m_Tween?.Kill();
            m_Tween = null;
        }

        void IScreenFader.FadeOut(Action onComplete)
        {
            m_CanvasGroup.blocksRaycasts = true;
            FadeTo(1f, m_Config.FadeOutDuration, onComplete);
        }

        void IScreenFader.FadeIn(Action onComplete)
        {
            FadeTo(0f, m_Config.FadeInDuration, () =>
            {
                m_CanvasGroup.blocksRaycasts = false;
                onComplete?.Invoke();
            });
        }

        private void FadeTo(float targetAlpha, float duration, Action onComplete)
        {
            m_Tween?.Kill();
            m_Tween = m_CanvasGroup
                .DOFade(targetAlpha, duration)
                .SetEase(m_Config.Ease)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    m_Tween = null;
                    onComplete?.Invoke();
                });
        }
    }
}
