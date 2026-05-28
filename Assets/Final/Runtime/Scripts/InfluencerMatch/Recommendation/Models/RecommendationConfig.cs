using DG.Tweening;
using Final.InfluencerMatch.Common;
using Final.Systems.ConfigManagement;
using UnityEngine;

namespace Final.InfluencerMatch.Recommendation
{
    /// <summary>
    /// Tunable copy, card animation timings and alpha values for the recommendation list and the individual influencer cards inside it.
    /// </summary>
    [CreateAssetMenu(menuName = GlobalEnvironmentVariables.AppName + "/Recommendation Config", fileName = "RecommendationConfig", order = 120)]
    public class RecommendationConfig : ScriptableObject, IVisibleConfig
    {
        [Header("Card Alpha")]
        [SerializeField, Range(0f, 1f)] private float m_NormalAlpha = 1.0f;
        [SerializeField, Range(0f, 1f)] private float m_DisabledAlpha = 0.5f;

        [Header("Stagger Spawn")]
        [SerializeField, Min(0)] private float m_StaggerDelay = 0.025f;
        [SerializeField, Min(0f)] private float m_CardFadeInDuration = 0.18f;
        [SerializeField] private Ease m_CardFadeInEase = Ease.OutQuad;

        [Header("Tap Animation")]
        [SerializeField, Min(0f)] private float m_TapPunchScale = 0.08f;
        [SerializeField, Min(0f)] private float m_TapPunchDuration = 0.18f;
        [SerializeField, Range(0f, 1f)] private float m_TapPunchElasticity = 0.5f;

        string IVisibleConfig.ConfigName => "Recommendation Config";
        string IVisibleConfig.Category => "Recommendation";

        public float NormalAlpha => m_NormalAlpha;
        public float DisabledAlpha => m_DisabledAlpha;
        public float StaggerDelay => m_StaggerDelay;
        public float CardFadeInDuration => m_CardFadeInDuration;
        public Ease CardFadeInEase => m_CardFadeInEase;
        public float TapPunchScale => m_TapPunchScale;
        public float TapPunchDuration => m_TapPunchDuration;
        public float TapPunchElasticity => m_TapPunchElasticity;
    }
}
