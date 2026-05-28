using System.Collections.Generic;
using Final.Editor.Tools;
using Final.InfluencerMatch.Common;
using Final.InfluencerMatch.Recommendation;
using UnityEditor;
using UnityEngine;

namespace Final.Editor.Inspectors
{
    /// <summary>
    /// Custom inspector for <see cref="InfluencerData"/> that surfaces per-asset validation errors and warnings.
    /// </summary>
    [CustomEditor(typeof(InfluencerData))]
    public class InfluencerDataInspector : UnityEditor.Editor
    {
        private const int k_MaxDisplayNameLength = 60;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            InfluencerData data = (InfluencerData)target;
            EditorGUILayout.Space();

            List<string> errors = new List<string>();
            List<string> warnings = new List<string>();

            if (data.Id.IsEmpty)
            {
                errors.Add("Id is empty (will be auto-filled on next import).");
            }

            if (string.IsNullOrEmpty(data.DisplayName))
            {
                errors.Add("DisplayName is empty.");
            }
            else if (data.DisplayName.Length > k_MaxDisplayNameLength)
            {
                warnings.Add($"DisplayName length {data.DisplayName.Length} exceeds {k_MaxDisplayNameLength}.");
            }

            if (string.IsNullOrEmpty(data.Email))
            {
                errors.Add("Email is empty.");
            }
            else if (InfluencerDatabaseEditorHelper.IsEmailMissingOrMalformed(data.Email))
            {
                errors.Add($"Email is malformed: '{data.Email}'.");
            }

            if (data.Followers < 0)
            {
                errors.Add("Followers is negative.");
            }

            if (data.EngagementRate < 0f || data.EngagementRate > 1f)
            {
                errors.Add($"EngagementRate {data.EngagementRate} is out of [0,1].");
            }

            if (data.BasePrice < 0)
            {
                errors.Add("BasePrice is negative.");
            }

            if (data.Avatar == null)
            {
                warnings.Add("Avatar is null — runtime fallback will be used.");
            }

            HashSet<CategoryId> seen = new HashSet<CategoryId>();
            for (int i = 0; i < data.CategoryScores.Count; i++)
            {
                CategoryId cat = data.CategoryScores[i].Category;
                if (cat == CategoryId.None)
                {
                    warnings.Add($"CategoryScores[{i}] has Category=None.");
                }
                else if (!seen.Add(cat))
                {
                    errors.Add($"Duplicate CategoryScores entry for '{cat}'.");
                }
            }

            EditorGUILayout.LabelField("Validation", EditorStyles.boldLabel);

            for (int i = 0; i < errors.Count; i++)
            {
                EditorGUILayout.HelpBox(errors[i], MessageType.Error);
            }

            for (int i = 0; i < warnings.Count; i++)
            {
                EditorGUILayout.HelpBox(warnings[i], MessageType.Warning);
            }

            if (errors.Count == 0 && warnings.Count == 0)
            {
                EditorGUILayout.HelpBox("All validation checks passed.", MessageType.Info);
            }
        }
    }
}
