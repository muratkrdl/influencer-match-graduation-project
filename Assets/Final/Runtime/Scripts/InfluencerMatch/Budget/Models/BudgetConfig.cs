using Final.InfluencerMatch.Common;
using Final.Systems.ConfigManagement;
using UnityEngine;

namespace Final.InfluencerMatch.Budget
{
    /// <summary>
    /// Interaction animation tunables scoped to the budget + category input feature (toggle bounce, validation shake).
    /// </summary>
    [CreateAssetMenu(menuName = GlobalEnvironmentVariables.AppName + "/Budget Config", fileName = "BudgetConfig", order = 100)]
    public class BudgetConfig : ScriptableObject, IVisibleConfig
    {
        [Header("Toggle Bounce")]
        [SerializeField, Min(0f)] private float m_TogglePunchScale = 0.08f;
        [SerializeField, Min(0f)] private float m_ToggleBounceDuration = 0.18f;
        [SerializeField, Min(0)] private int m_ToggleBounceVibrato = 1;
        [SerializeField, Range(0f, 1f)] private float m_ToggleBounceElasticity = 0.5f;

        [Header("Validation Shake")]
        [SerializeField, Min(0f)] private float m_ValidationShakeDuration = 0.3f;
        [SerializeField, Min(0f)] private float m_ValidationShakeStrengthX = 10f;
        [SerializeField, Min(0)] private int m_ValidationShakeVibrato = 10;

        string IVisibleConfig.ConfigName => "Budget Config";
        string IVisibleConfig.Category => "Budget";

        public float TogglePunchScale => m_TogglePunchScale;
        public float ToggleBounceDuration => m_ToggleBounceDuration;
        public int ToggleBounceVibrato => m_ToggleBounceVibrato;
        public float ToggleBounceElasticity => m_ToggleBounceElasticity;

        public float ValidationShakeDuration => m_ValidationShakeDuration;
        public float ValidationShakeStrengthX => m_ValidationShakeStrengthX;
        public int ValidationShakeVibrato => m_ValidationShakeVibrato;
    }
}
