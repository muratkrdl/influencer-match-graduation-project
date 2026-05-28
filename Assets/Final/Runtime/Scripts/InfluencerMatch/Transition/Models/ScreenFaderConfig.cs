using DG.Tweening;
using Final.InfluencerMatch.Common;
using Final.Systems.ConfigManagement;
using UnityEngine;

namespace Final.InfluencerMatch.Transition
{
    /// <summary>
    /// Tunable timing for the screen-transition fade overlay (<see cref="ScreenFaderView"/>).
    /// </summary>
    [CreateAssetMenu(menuName = GlobalEnvironmentVariables.AppName + "/Screen Fader Config", fileName = "ScreenFaderConfig", order = 100)]
    public class ScreenFaderConfig : ScriptableObject, IVisibleConfig
    {
        [Header("Timing")]
        [SerializeField, Min(0f)] private float m_FadeOutDuration = 0.3f;
        [SerializeField, Min(0f)] private float m_FadeInDuration = 0.3f;
        [SerializeField] private Ease m_Ease = Ease.OutQuad;

        string IVisibleConfig.ConfigName => "Screen Fader Config";
        string IVisibleConfig.Category => "UI";

        public float FadeOutDuration => m_FadeOutDuration;
        public float FadeInDuration => m_FadeInDuration;
        public Ease Ease => m_Ease;
    }
}
