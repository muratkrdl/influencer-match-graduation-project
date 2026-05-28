using System.Collections.Generic;
using Final.InfluencerMatch.Common;
using Final.InfluencerMatch.Recommendation;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Final.Editor.Tools
{
    /// <summary>
    /// Locates, caches, and provides shared maintenance operations for the project-wide <see cref="InfluencerDatabase"/> asset (refresh from folder, bulk validation, common predicates).
    /// </summary>
    public static class InfluencerDatabaseEditorHelper
    {
        private static InfluencerDatabase s_CachedDatabase;

        public static InfluencerDatabase GetDatabase()
        {
            if (s_CachedDatabase != null)
            {
                return s_CachedDatabase;
            }

            string[] guids = AssetDatabase.FindAssets("t:" + nameof(InfluencerDatabase));
            if (guids.Length == 0)
            {
                return null;
            }

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            s_CachedDatabase = AssetDatabase.LoadAssetAtPath<InfluencerDatabase>(path);
            return s_CachedDatabase;
        }

        public static int RefreshFromFolder(InfluencerDatabase database)
        {
            if (database == null)
            {
                return 0;
            }

            string[] guids = AssetDatabase.FindAssets("t:" + nameof(InfluencerData), new[] { EditorAssetPaths.InfluencersFolder });
            SerializedObject so = new SerializedObject(database);
            SerializedProperty listProp = so.FindProperty("m_Influencers");

            HashSet<InfluencerData> existing = new HashSet<InfluencerData>();
            for (int i = 0; i < listProp.arraySize; i++)
            {
                InfluencerData entry = listProp.GetArrayElementAtIndex(i).objectReferenceValue as InfluencerData;
                if (entry != null)
                {
                    existing.Add(entry);
                }
            }

            int added = 0;
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                InfluencerData asset = AssetDatabase.LoadAssetAtPath<InfluencerData>(path);
                if (asset != null && existing.Add(asset))
                {
                    int idx = listProp.arraySize;
                    listProp.InsertArrayElementAtIndex(idx);
                    listProp.GetArrayElementAtIndex(idx).objectReferenceValue = asset;
                    added++;
                }
            }

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();
            Debug.Log($"InfluencerDatabase '{database.name}': refreshed from folder, added {added} new reference(s) (total: {listProp.arraySize}).", database);
            return added;
        }

        public static void ValidateAll(InfluencerDatabase database)
        {
            if (database == null)
            {
                return;
            }

            int errors = 0;
            int warnings = 0;
            HashSet<SerializableGuid> ids = new HashSet<SerializableGuid>();

            for (int i = 0; i < database.Influencers.Count; i++)
            {
                InfluencerData entry = database.Influencers[i];
                if (entry == null)
                {
                    Debug.LogError($"InfluencerDatabase '{database.name}' has null reference at index {i}.", database);
                    errors++;
                    continue;
                }

                if (entry.Id.IsEmpty)
                {
                    Debug.LogError($"InfluencerData '{entry.name}' has empty Id.", entry);
                    errors++;
                }
                else if (!ids.Add(entry.Id))
                {
                    Debug.LogError($"InfluencerData '{entry.name}' has duplicate Id '{entry.Id}'.", entry);
                    errors++;
                }

                if (entry.Avatar == null)
                {
                    Debug.LogWarning($"InfluencerData '{entry.name}' has null Avatar.", entry);
                    warnings++;
                }

                if (IsEmailMissingOrMalformed(entry.Email))
                {
                    Debug.LogError($"InfluencerData '{entry.name}' has missing/malformed Email.", entry);
                    errors++;
                }
            }

            Debug.Log($"InfluencerDatabase '{database.name}': validation complete — {errors} error(s), {warnings} warning(s).", database);
        }

        public static bool IsEmailMissingOrMalformed(string email)
        {
            return string.IsNullOrEmpty(email) || !email.Contains("@") || !email.Contains(".");
        }

        [DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            s_CachedDatabase = null;
        }
    }
}
