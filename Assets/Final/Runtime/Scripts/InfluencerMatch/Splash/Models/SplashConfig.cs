using Final.InfluencerMatch.Common;
using Final.Systems.ConfigManagement;
using UnityEngine;

namespace Final.InfluencerMatch.Splash
{
    /// <summary>
    /// Tunable copy + timing for the splash screen.
    /// </summary>
    [CreateAssetMenu(menuName = GlobalEnvironmentVariables.AppName + "/Splash Config", fileName = "SplashConfig", order = 100)]
    public class SplashConfig : ScriptableObject, IVisibleConfig
    {
        [Header("Copy")]
        [SerializeField] private string m_Title = "INFLUENCER MATCH";

        [Header("Timing")]
        [SerializeField, Min(0f)] private float m_DurationSeconds = 1.5f;

        string IVisibleConfig.ConfigName => "Splash Config";
        string IVisibleConfig.Category => "Splash";

        public string Title => m_Title;
        public float DurationSeconds => m_DurationSeconds;
    }
}
