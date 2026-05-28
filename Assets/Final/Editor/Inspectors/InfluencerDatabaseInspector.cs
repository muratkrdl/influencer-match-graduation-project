using System.Collections.Generic;
using Final.Editor.Tools;
using Final.InfluencerMatch.Common;
using Final.InfluencerMatch.Recommendation;
using UnityEditor;
using UnityEngine;

namespace Final.Editor.Inspectors
{
    /// <summary>
    /// Custom inspector for <see cref="InfluencerDatabase"/> that surfaces summary diagnostics and bulk operations.
    /// </summary>
    [CustomEditor(typeof(InfluencerDatabase))]
    public class InfluencerDatabaseInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            InfluencerDatabase db = (InfluencerDatabase)target;
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Summary", EditorStyles.boldLabel);

            int total = db.Influencers.Count;
            int nullCount = 0;
            int missingAvatar = 0;
            int missingEmail = 0;
            int duplicateIds = 0;
            HashSet<SerializableGuid> ids = new HashSet<SerializableGuid>();

            for (int i = 0; i < total; i++)
            {
                InfluencerData entry = db.Influencers[i];
                if (entry == null)
                {
                    nullCount++;
                    continue;
                }

                if (entry.Avatar == null)
                {
                    missingAvatar++;
                }

                if (InfluencerDatabaseEditorHelper.IsEmailMissingOrMalformed(entry.Email))
                {
                    missingEmail++;
                }

                if (!entry.Id.IsEmpty && !ids.Add(entry.Id))
                {
                    duplicateIds++;
                }
            }

            EditorGUILayout.HelpBox(
                $"Total: {total}\nNull refs: {nullCount}\nDuplicate Ids: {duplicateIds}\nMissing Avatar: {missingAvatar}\nMissing/malformed Email: {missingEmail}",
                nullCount == 0 && duplicateIds == 0 ? MessageType.Info : MessageType.Error);

            EditorGUILayout.Space();

            if (GUILayout.Button("Refresh from Folder"))
            {
                InfluencerDatabaseEditorHelper.RefreshFromFolder(db);
            }

            if (GUILayout.Button("Validate All"))
            {
                InfluencerDatabaseEditorHelper.ValidateAll(db);
            }
        }
    }
}
