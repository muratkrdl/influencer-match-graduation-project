using Final.InfluencerMatch.Recommendation;
using UnityEditor;
using UnityEngine;

namespace Final.Editor.Inspectors
{
    /// <summary>
    /// Custom inspector for <see cref="MatchingConfig"/> that surfaces weight-sum and entry-count validation.
    /// </summary>
    [CustomEditor(typeof(MatchingConfig))]
    public class MatchingConfigInspector : UnityEditor.Editor
    {
        private const float k_WeightSumTolerance = 0.001f;
        private const int k_ExpectedScoreMultiplierCount = 5;
        private const int k_ExpectedTierCount = 5;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            MatchingConfig cfg = (MatchingConfig)target;
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Validation", EditorStyles.boldLabel);

            float sum = cfg.CategoryWeight + cfg.FollowersWeight + cfg.EngagementWeight;
            if (Mathf.Abs(sum - 1.0f) < k_WeightSumTolerance)
            {
                EditorGUILayout.HelpBox($"Weights sum: {sum:F3} OK", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox($"Weights sum: {sum:F3} — expected 1.000.", MessageType.Error);
            }

            if (cfg.CategoryScoreMultipliers.Count == k_ExpectedScoreMultiplierCount)
            {
                EditorGUILayout.HelpBox($"CategoryScoreMultipliers: {k_ExpectedScoreMultiplierCount} entries OK", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox($"CategoryScoreMultipliers: {cfg.CategoryScoreMultipliers.Count} entries — expected {k_ExpectedScoreMultiplierCount}.", MessageType.Warning);
            }

            if (cfg.FollowerTierMultipliers.Count == k_ExpectedTierCount)
            {
                EditorGUILayout.HelpBox($"FollowerTierMultipliers: {k_ExpectedTierCount} entries OK", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox($"FollowerTierMultipliers: {cfg.FollowerTierMultipliers.Count} entries — expected {k_ExpectedTierCount}.", MessageType.Warning);
            }

            if (cfg.FollowerNormalizationMin >= cfg.FollowerNormalizationMax)
            {
                EditorGUILayout.HelpBox($"FollowerNormalizationMin ({cfg.FollowerNormalizationMin}) must be less than Max ({cfg.FollowerNormalizationMax}).", MessageType.Error);
            }
        }
    }
}
