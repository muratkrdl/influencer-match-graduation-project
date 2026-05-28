using DG.Tweening;
using Final.InfluencerMatch.Common;
using Final.InfluencerMatch.UI;
using TMPro;
using UnityEngine;
using VContainer;

namespace Final.InfluencerMatch.Splash
{
    /// <summary>
    /// Splash panel view that renders the title and version with a staggered fade-in.
    /// </summary>
    public class SplashView : UIPanelBase
    {
        private const float k_TextFadeDuration = 0.3f;
        private const float k_SubtitleStaggerDelay = 0.15f;

        [Header("Display Elements")]
        [SerializeField] private TMP_Text m_TitleText;
        [SerializeField] private TMP_Text m_SubtitleText;

        [Inject] private SplashConfig m_Config;

        private void Start()
        {
            m_TitleText.text = m_Config.Title;
            m_SubtitleText.text = InfluencerMatchEnvironment.VersionPrefix + GlobalEnvironmentVariables.Version;

            m_TitleText.alpha = 0f;
            m_SubtitleText.alpha = 0f;
            m_TitleText.DOFade(1f, k_TextFadeDuration).SetEase(Ease.OutQuad).SetLink(gameObject);
            m_SubtitleText.DOFade(1f, k_TextFadeDuration).SetDelay(k_SubtitleStaggerDelay).SetEase(Ease.OutQuad).SetLink(gameObject);
        }
    }
}
