using DG.Tweening;
using Final.Systems.ConfigManagement;
using UnityEngine;

namespace Final.InfluencerMatch.Common
{
    /// <summary>
    /// Framework-level cross-cutting UI tunables that apply across every panel and every button.
    /// </summary>
    [CreateAssetMenu(menuName = GlobalEnvironmentVariables.AppName + "/UI Shared Config", fileName = "UISharedConfig", order = 150)]
    public class UISharedConfig : ScriptableObject, IVisibleConfig
    {
        [Header("Panel Transitions")]
        [SerializeField, Min(0f)] private float m_PanelFadeInDuration = 0.18f;
        [SerializeField] private Ease m_PanelFadeInEase = Ease.OutQuad;

        [Header("Button Press Feedback")]
        [SerializeField, Range(0f, 1f)] private float m_ButtonPressedScale = 0.95f;
        [SerializeField, Min(0f)] private float m_ButtonPressDuration = 0.08f;
        [SerializeField, Min(0f)] private float m_ButtonReleaseDuration = 0.12f;
        [SerializeField] private Ease m_ButtonPressEase = Ease.OutQuad;

        string IVisibleConfig.ConfigName => "UI Shared Config";
        string IVisibleConfig.Category => "UI";

        public float PanelFadeInDuration => m_PanelFadeInDuration;
        public Ease PanelFadeInEase => m_PanelFadeInEase;

        public float ButtonPressedScale => m_ButtonPressedScale;
        public float ButtonPressDuration => m_ButtonPressDuration;
        public float ButtonReleaseDuration => m_ButtonReleaseDuration;
        public Ease ButtonPressEase => m_ButtonPressEase;
    }
}
