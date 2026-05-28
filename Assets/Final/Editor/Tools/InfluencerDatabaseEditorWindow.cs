using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Final.InfluencerMatch.Common;
using Final.InfluencerMatch.Recommendation;
using UnityEditor;
using UnityEngine;

namespace Final.Editor.Tools
{
    /// <summary>
    /// Master-detail editor window for the <see cref="InfluencerDatabase"/> asset.
    /// </summary>
    public class InfluencerDatabaseEditorWindow : EditorWindow
    {
        private enum SortMode
        {
            Name,
            Followers,
            BasePrice
        }

        private const string k_MenuItemPath = "Tools/" + GlobalEnvironmentVariables.AppName + "/Influencer Database";
        private const string k_WindowTitle = "Influencer Database";
        private const string k_NewInfluencerPrefix = "Influencer";

        private const float k_LeftPanelWidth = 340f;
        private const float k_SeparatorWidth = 1f;
        private const float k_ListItemHeight = 56f;
        private const float k_ListIconSize = 40f;
        private const float k_ToolbarHeight = 22f;
        private const float k_HeaderHeight = 72f;
        private const float k_HeaderActionButtonHeight = 24f;
        private const float k_DetailActionButtonHeight = 28f;
        private const float k_DetailLabelWidth = 180f;
        private const float k_FollowersColumnWidth = 80f;

        private static readonly Vector2 k_MinSize = new Vector2(1000f, 600f);

        [SerializeField] private InfluencerData m_SelectedInfluencer;
        [SerializeField] private string m_SearchFilter = string.Empty;
        [SerializeField] private SortMode m_SortMode = SortMode.Name;

        private InfluencerDatabase m_Database;
        private SerializedObject m_DatabaseSO;
        private SerializedProperty m_InfluencersProperty;
        private UnityEditor.Editor m_SelectedEditor;
        private readonly List<int> m_FilteredIndices = new List<int>();
        private Vector2 m_ListScroll;
        private Vector2 m_DetailScroll;

        private int m_CachedArraySize = -1;
        private string m_CachedSearchFilter;
        private SortMode m_CachedSortMode;
        private int m_NullRefCount;

        [MenuItem(k_MenuItemPath)]
        public static void Open()
        {
            InfluencerDatabaseEditorWindow window = GetWindow<InfluencerDatabaseEditorWindow>(k_WindowTitle);
            window.minSize = k_MinSize;
            window.Show();
        }

        private void OnEnable()
        {
            wantsMouseMove = true;
            LoadDatabase();
        }

        private void OnDisable()
        {
            DestroySelectedEditor();
        }

        private void OnGUI()
        {
            if (m_Database == null)
            {
                DrawNoDatabaseView();
                return;
            }

            if (m_DatabaseSO == null || m_DatabaseSO.targetObject == null)
            {
                RebuildSerializedObject();
                if (m_DatabaseSO == null)
                {
                    DrawNoDatabaseView();
                    return;
                }
            }

            m_DatabaseSO.Update();
            EnsureFilteredList();

            DrawHeader();
            EditorGUILayout.BeginHorizontal();
            DrawLeftPanel();
            DrawPanelDivider();
            DrawRightPanel();
            EditorGUILayout.EndHorizontal();
        }

        private void LoadDatabase()
        {
            m_Database = InfluencerDatabaseEditorHelper.GetDatabase();
            RebuildSerializedObject();
            InvalidateFilterCache();
        }

        private void RebuildSerializedObject()
        {
            if (m_Database == null)
            {
                m_DatabaseSO = null;
                m_InfluencersProperty = null;
                return;
            }

            m_DatabaseSO = new SerializedObject(m_Database);
            m_InfluencersProperty = m_DatabaseSO.FindProperty("m_Influencers");
        }

        private void InvalidateFilterCache()
        {
            m_CachedArraySize = -1;
            m_CachedSearchFilter = null;
        }

        private void EnsureFilteredList()
        {
            int currentSize = m_InfluencersProperty.arraySize;
            if (currentSize == m_CachedArraySize
                && string.Equals(m_CachedSearchFilter, m_SearchFilter, StringComparison.Ordinal)
                && m_CachedSortMode == m_SortMode)
            {
                return;
            }

            RebuildFilteredList();
            m_CachedArraySize = currentSize;
            m_CachedSearchFilter = m_SearchFilter;
            m_CachedSortMode = m_SortMode;
        }

        private void RebuildFilteredList()
        {
            m_FilteredIndices.Clear();
            int count = m_InfluencersProperty.arraySize;
            string filter = string.IsNullOrEmpty(m_SearchFilter) ? null : m_SearchFilter.Trim();
            int nullRefs = 0;

            for (int i = 0; i < count; i++)
            {
                InfluencerData entry = m_InfluencersProperty.GetArrayElementAtIndex(i).objectReferenceValue as InfluencerData;
                if (entry == null)
                {
                    nullRefs++;
                    m_FilteredIndices.Add(i);
                    continue;
                }

                if (filter != null)
                {
                    bool matches = entry.DisplayName.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0
                                   || (!string.IsNullOrEmpty(entry.Handle)
                                       && entry.Handle.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0);
                    if (!matches)
                    {
                        continue;
                    }
                }
                m_FilteredIndices.Add(i);
            }

            m_NullRefCount = nullRefs;

            switch (m_SortMode)
            {
                case SortMode.Name:
                    m_FilteredIndices.Sort(CompareByName);
                    break;
                case SortMode.Followers:
                    m_FilteredIndices.Sort(CompareByFollowersDesc);
                    break;
                case SortMode.BasePrice:
                    m_FilteredIndices.Sort(CompareByBasePriceDesc);
                    break;
            }
        }

        private int CompareByName(int a, int b)
        {
            InfluencerData ea = ResolveEntry(a);
            InfluencerData eb = ResolveEntry(b);
            string na = ea != null ? ea.DisplayName : string.Empty;
            string nb = eb != null ? eb.DisplayName : string.Empty;
            return string.Compare(na, nb, StringComparison.OrdinalIgnoreCase);
        }

        private int CompareByFollowersDesc(int a, int b)
        {
            InfluencerData ea = ResolveEntry(a);
            InfluencerData eb = ResolveEntry(b);
            int fa = ea != null ? ea.Followers : 0;
            int fb = eb != null ? eb.Followers : 0;
            return fb.CompareTo(fa);
        }

        private int CompareByBasePriceDesc(int a, int b)
        {
            InfluencerData ea = ResolveEntry(a);
            InfluencerData eb = ResolveEntry(b);
            int pa = ea != null ? ea.BasePrice : 0;
            int pb = eb != null ? eb.BasePrice : 0;
            return pb.CompareTo(pa);
        }

        private InfluencerData ResolveEntry(int dbIndex)
        {
            return m_InfluencersProperty.GetArrayElementAtIndex(dbIndex).objectReferenceValue as InfluencerData;
        }

        private void DrawNoDatabaseView()
        {
            GUILayout.Space(EditorStyleCache.Spacing24);
            EditorGUILayout.LabelField("No InfluencerDatabase asset found in project.", EditorStyles.centeredGreyMiniLabel);
            GUILayout.Space(EditorStyleCache.Spacing8);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Find and Load", EditorStyleCache.FlatActionButton, GUILayout.Width(140f), GUILayout.Height(k_DetailActionButtonHeight)))
            {
                LoadDatabase();
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawHeader()
        {
            Rect headerRect = EditorGUILayout.GetControlRect(false, k_HeaderHeight, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(headerRect, EditorStyleCache.HeaderBackgroundColor);

            GUILayout.BeginArea(headerRect);
            GUILayout.Space(EditorStyleCache.Spacing12);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(EditorStyleCache.Spacing16);

            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Influencer Database", EditorStyleCache.TitleMedium);
            EditorGUILayout.LabelField(BuildSummary(), EditorStyleCache.CaptionLabel);
            EditorGUILayout.EndVertical();

            GUILayout.FlexibleSpace();

            EditorGUILayout.BeginVertical();
            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginHorizontal();
            DrawHeaderActionButton("Refresh", RefreshFromFolder);
            GUILayout.Space(EditorStyleCache.Spacing4);
            DrawHeaderActionButton("Validate", () => InfluencerDatabaseEditorHelper.ValidateAll(m_Database));
            GUILayout.Space(EditorStyleCache.Spacing4);
            DrawHeaderActionButton("Regenerate IDs", RegenerateAllIds);
            GUILayout.Space(EditorStyleCache.Spacing4);
            DrawHeaderActionButton("Ping", () => EditorGUIUtility.PingObject(m_Database));
            EditorGUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndVertical();

            GUILayout.Space(EditorStyleCache.Spacing16);
            EditorGUILayout.EndHorizontal();
            GUILayout.EndArea();

            Rect bottomBorder = new Rect(headerRect.x, headerRect.yMax - EditorStyleCache.DividerThickness, headerRect.width, EditorStyleCache.DividerThickness);
            EditorGUI.DrawRect(bottomBorder, EditorStyleCache.PanelDividerColor);
        }

        private static void DrawHeaderActionButton(string label, Action onClick)
        {
            if (GUILayout.Button(label, EditorStyleCache.FlatActionButton, GUILayout.Height(k_HeaderActionButtonHeight)))
            {
                onClick();
            }
        }

        private string BuildSummary()
        {
            int total = m_InfluencersProperty.arraySize;
            int filtered = m_FilteredIndices.Count;
            string assetPath = AssetDatabase.GetAssetPath(m_Database);
            if (m_NullRefCount > 0)
            {
                return $"Total: {total}  ·  Filtered: {filtered}  ·  Null refs: {m_NullRefCount}  ·  {assetPath}";
            }
            return $"Total: {total}  ·  Filtered: {filtered}  ·  {assetPath}";
        }

        private void DrawLeftPanel()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(k_LeftPanelWidth));
            DrawListToolbar();
            DrawList();
            EditorGUILayout.EndVertical();
        }

        private void DrawListToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.Height(k_ToolbarHeight));
            m_SearchFilter = EditorGUILayout.TextField(m_SearchFilter, EditorStyles.toolbarSearchField, GUILayout.MinWidth(120f));
            m_SortMode = (SortMode)EditorGUILayout.EnumPopup(m_SortMode, EditorStyles.toolbarPopup, GUILayout.Width(90f));
            if (GUILayout.Button("+", EditorStyles.toolbarButton, GUILayout.Width(24f)))
            {
                AddInfluencer();
            }
            EditorGUI.BeginDisabledGroup(m_SelectedInfluencer == null);
            if (GUILayout.Button("-", EditorStyles.toolbarButton, GUILayout.Width(24f)))
            {
                RemoveSelectedInfluencer();
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawList()
        {
            m_ListScroll = EditorGUILayout.BeginScrollView(m_ListScroll);
            for (int i = 0; i < m_FilteredIndices.Count; i++)
            {
                int dbIndex = m_FilteredIndices[i];
                DrawListItem(dbIndex);
            }
            EditorGUILayout.EndScrollView();
        }

        private void DrawListItem(int dbIndex)
        {
            InfluencerData entry = ResolveEntry(dbIndex);
            bool isSelected = entry != null && m_SelectedInfluencer == entry;
            Rect rect = EditorGUILayout.GetControlRect(false, k_ListItemHeight);

            Event evt = Event.current;
            bool isHover = rect.Contains(evt.mousePosition);

            if (isSelected)
            {
                EditorStyleCache.DrawSelectionHighlight(rect);
            }
            else if (isHover)
            {
                EditorStyleCache.DrawHoverHighlight(rect);
            }

            if (evt.type == EventType.MouseDown && evt.button == 0 && isHover)
            {
                SelectInfluencer(entry);
                GUI.FocusControl(null);
                evt.Use();
                Repaint();
            }
            else if (evt.type == EventType.MouseMove && isHover)
            {
                Repaint();
            }

            float iconLeft = rect.x + EditorStyleCache.AccentBarWidth + EditorStyleCache.Spacing12;
            Rect iconRect = new Rect(iconLeft, rect.y + (rect.height - k_ListIconSize) * 0.5f, k_ListIconSize, k_ListIconSize);
            DrawAvatar(iconRect, entry != null ? entry.Avatar : null);

            float textX = iconRect.xMax + EditorStyleCache.Spacing12;
            float followersX = rect.xMax - k_FollowersColumnWidth - EditorStyleCache.Spacing12;
            float nameColumnWidth = followersX - textX - EditorStyleCache.Spacing8;

            string displayName = entry != null && !string.IsNullOrEmpty(entry.DisplayName)
                ? entry.DisplayName
                : "(missing)";
            Rect nameRect = new Rect(textX, rect.y + EditorStyleCache.Spacing8, nameColumnWidth, 18f);
            EditorGUI.LabelField(nameRect, displayName, isSelected ? EditorStyleCache.ListItemSelected : EditorStyles.boldLabel);

            string handle = entry != null && !string.IsNullOrEmpty(entry.Handle) ? "@" + entry.Handle : "(no handle)";
            Rect handleRect = new Rect(textX, nameRect.yMax + 2f, nameColumnWidth, 14f);
            EditorGUI.LabelField(handleRect, handle, EditorStyleCache.MutedMiniLabel);

            Rect followersRect = new Rect(followersX, rect.y + EditorStyleCache.Spacing8, k_FollowersColumnWidth, 18f);
            string followersText = entry != null ? FormatFollowerCount(entry.Followers) : "—";
            EditorGUI.LabelField(followersRect, followersText, EditorStyleCache.RightAlignedBoldLabel);
        }

        private static void DrawAvatar(Rect rect, Sprite sprite)
        {
            if (sprite == null || sprite.texture == null)
            {
                EditorGUI.DrawRect(rect, EditorStyleCache.IconPlaceholderColor);
                return;
            }

            Texture2D tex = sprite.texture;
            Rect textureRect = sprite.textureRect;
            Rect uv = new Rect(
                textureRect.x / tex.width,
                textureRect.y / tex.height,
                textureRect.width / tex.width,
                textureRect.height / tex.height);
            GUI.DrawTextureWithTexCoords(rect, tex, uv);
        }

        private static string FormatFollowerCount(int count)
        {
            if (count >= 1_000_000)
            {
                double m = count / 1_000_000.0;
                return m.ToString("0.#", CultureInfo.InvariantCulture) + "M";
            }
            if (count >= 1_000)
            {
                double k = count / 1_000.0;
                return k.ToString("0.#", CultureInfo.InvariantCulture) + "K";
            }
            return count.ToString(CultureInfo.InvariantCulture);
        }

        private static void DrawPanelDivider()
        {
            Rect rect = EditorGUILayout.GetControlRect(false, GUILayout.Width(k_SeparatorWidth), GUILayout.ExpandHeight(true));
            EditorGUI.DrawRect(rect, EditorStyleCache.PanelDividerColor);
        }

        private static void DrawHorizontalDivider()
        {
            Rect rect = EditorGUILayout.GetControlRect(false, EditorStyleCache.DividerThickness);
            EditorGUI.DrawRect(rect, EditorStyleCache.SeparatorColor);
        }

        private void DrawRightPanel()
        {
            EditorGUILayout.BeginVertical();

            if (m_SelectedInfluencer == null)
            {
                GUILayout.Space(EditorStyleCache.Spacing24);
                EditorGUILayout.LabelField("Select an influencer from the list.", EditorStyles.centeredGreyMiniLabel);
                EditorGUILayout.EndVertical();
                return;
            }

            EnsureSelectedEditor();

            GUILayout.Space(EditorStyleCache.Spacing24);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(EditorStyleCache.Spacing24);
            EditorGUILayout.BeginVertical();

            string displayName = !string.IsNullOrEmpty(m_SelectedInfluencer.DisplayName)
                ? m_SelectedInfluencer.DisplayName
                : m_SelectedInfluencer.name;
            EditorGUILayout.LabelField(displayName, EditorStyleCache.TitleLarge);

            string subtitle = !string.IsNullOrEmpty(m_SelectedInfluencer.Handle)
                ? "@" + m_SelectedInfluencer.Handle
                : m_SelectedInfluencer.name;
            EditorGUILayout.LabelField(subtitle, EditorStyleCache.CaptionLabel);
            GUILayout.Space(EditorStyleCache.Spacing16);
            DrawHorizontalDivider();
            GUILayout.Space(EditorStyleCache.Spacing16);

            m_DetailScroll = EditorGUILayout.BeginScrollView(m_DetailScroll);
            if (m_SelectedEditor != null)
            {
                float oldLabelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = k_DetailLabelWidth;
                m_SelectedEditor.OnInspectorGUI();
                EditorGUIUtility.labelWidth = oldLabelWidth;
            }
            EditorGUILayout.EndScrollView();

            GUILayout.Space(EditorStyleCache.Spacing16);
            DrawHorizontalDivider();
            GUILayout.Space(EditorStyleCache.Spacing12);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Regenerate ID", EditorStyleCache.FlatActionButton, GUILayout.Height(k_DetailActionButtonHeight)))
            {
                RegenerateSelectedId();
            }
            GUILayout.Space(EditorStyleCache.Spacing8);
            if (GUILayout.Button("Ping in Project", EditorStyleCache.FlatActionButton, GUILayout.Height(k_DetailActionButtonHeight)))
            {
                EditorGUIUtility.PingObject(m_SelectedInfluencer);
            }
            GUILayout.Space(EditorStyleCache.Spacing8);
            if (GUILayout.Button("Select in Project", EditorStyleCache.FlatActionButton, GUILayout.Height(k_DetailActionButtonHeight)))
            {
                Selection.activeObject = m_SelectedInfluencer;
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(EditorStyleCache.Spacing16);
            EditorGUILayout.EndVertical();
            GUILayout.Space(EditorStyleCache.Spacing24);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void RegenerateAllIds()
        {
            int total = m_InfluencersProperty.arraySize;
            if (total == 0)
            {
                Debug.LogWarning("InfluencerDatabaseEditorWindow: database is empty — nothing to regenerate.");
                return;
            }

            bool firstConfirm = EditorUtility.DisplayDialog(
                "Regenerate ALL IDs",
                $"Generate new Ids for ALL {total} influencer(s) in the database?\n\n" +
                "This breaks every external reference that stored the previous Ids (saved state, analytics, deep-links).",
                "Continue",
                "Cancel");
            if (!firstConfirm)
            {
                return;
            }

            bool secondConfirm = EditorUtility.DisplayDialog(
                "Final Confirmation",
                $"Last chance — regenerate Ids for {total} influencer(s)?",
                "Yes, regenerate",
                "Cancel");
            if (!secondConfirm)
            {
                return;
            }

            List<UnityEngine.Object> recordTargets = new List<UnityEngine.Object>(total);
            for (int i = 0; i < total; i++)
            {
                InfluencerData entry = ResolveEntry(i);
                if (entry != null)
                {
                    recordTargets.Add(entry);
                }
            }
            Undo.RecordObjects(recordTargets.ToArray(), "Regenerate All Influencer Ids");

            int regenerated = 0;
            for (int i = 0; i < total; i++)
            {
                InfluencerData entry = ResolveEntry(i);
                if (entry == null)
                {
                    continue;
                }

                AssignNewIdWithoutUndo(entry);
                regenerated++;
            }

            AssetDatabase.SaveAssets();
            DestroySelectedEditor();
            Repaint();
            Debug.Log($"InfluencerDatabaseEditorWindow: regenerated Ids for {regenerated} influencer(s).");
        }

        private void RegenerateSelectedId()
        {
            if (m_SelectedInfluencer == null)
            {
                return;
            }

            bool confirmed = EditorUtility.DisplayDialog(
                "Regenerate ID",
                $"Generate a new Id for '{m_SelectedInfluencer.name}'?\n\nThis breaks any external reference that stored the previous Id (saved state, analytics, etc).",
                "Regenerate",
                "Cancel");
            if (!confirmed)
            {
                return;
            }

            AssignNewId(m_SelectedInfluencer);
            DestroySelectedEditor();
            Repaint();
        }

        private static void AssignNewId(InfluencerData target)
        {
            SerializedObject so = new SerializedObject(target);
            WriteNewGuid(so.FindProperty("m_Id"));
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
        }

        private static void AssignNewIdWithoutUndo(InfluencerData target)
        {
            SerializedObject so = new SerializedObject(target);
            WriteNewGuid(so.FindProperty("m_Id"));
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void WriteNewGuid(SerializedProperty idProp)
        {
            byte[] bytes = Guid.NewGuid().ToByteArray();
            idProp.FindPropertyRelative("m_Part1").longValue = BitConverter.ToUInt32(bytes, 0);
            idProp.FindPropertyRelative("m_Part2").longValue = BitConverter.ToUInt32(bytes, 4);
            idProp.FindPropertyRelative("m_Part3").longValue = BitConverter.ToUInt32(bytes, 8);
            idProp.FindPropertyRelative("m_Part4").longValue = BitConverter.ToUInt32(bytes, 12);
        }

        private void EnsureSelectedEditor()
        {
            if (m_SelectedEditor != null && m_SelectedEditor.target == m_SelectedInfluencer)
            {
                return;
            }

            DestroySelectedEditor();
            if (m_SelectedInfluencer != null)
            {
                m_SelectedEditor = UnityEditor.Editor.CreateEditor(m_SelectedInfluencer);
            }
        }

        private void DestroySelectedEditor()
        {
            if (m_SelectedEditor != null)
            {
                DestroyImmediate(m_SelectedEditor);
                m_SelectedEditor = null;
            }
        }

        private void SelectInfluencer(InfluencerData influencer)
        {
            if (m_SelectedInfluencer == influencer)
            {
                return;
            }

            m_SelectedInfluencer = influencer;
            DestroySelectedEditor();
        }

        private void AddInfluencer()
        {
            if (!AssetDatabase.IsValidFolder(EditorAssetPaths.InfluencersFolder))
            {
                Debug.LogError($"InfluencerDatabaseEditorWindow: target folder '{EditorAssetPaths.InfluencersFolder}' does not exist.");
                return;
            }

            string newAssetPath = FindNextInfluencerAssetPath();
            InfluencerData newAsset = ScriptableObject.CreateInstance<InfluencerData>();
            AssetDatabase.CreateAsset(newAsset, newAssetPath);
            AssetDatabase.SaveAssets();

            int newIndex = m_InfluencersProperty.arraySize;
            m_InfluencersProperty.InsertArrayElementAtIndex(newIndex);
            m_InfluencersProperty.GetArrayElementAtIndex(newIndex).objectReferenceValue = newAsset;
            m_DatabaseSO.ApplyModifiedProperties();
            EditorUtility.SetDirty(m_Database);
            AssetDatabase.SaveAssets();

            SelectInfluencer(newAsset);
            Repaint();
        }

        private string FindNextInfluencerAssetPath()
        {
            int n = 1;
            while (true)
            {
                string candidate = $"{EditorAssetPaths.InfluencersFolder}/{k_NewInfluencerPrefix}{n:D3}.asset";
                if (!File.Exists(candidate))
                {
                    return candidate;
                }
                n++;
            }
        }

        private void RemoveSelectedInfluencer()
        {
            if (m_SelectedInfluencer == null)
            {
                return;
            }

            int dbIndex = FindDatabaseIndex(m_SelectedInfluencer);
            if (dbIndex < 0)
            {
                m_SelectedInfluencer = null;
                DestroySelectedEditor();
                return;
            }

            int choice = EditorUtility.DisplayDialogComplex(
                "Remove Influencer",
                $"Remove '{m_SelectedInfluencer.name}' from the database?\n\nChoosing 'Delete Asset' also removes the .asset file from disk.",
                "Remove Reference Only",
                "Cancel",
                "Delete Asset");
            if (choice == 1)
            {
                return;
            }
            bool deleteAsset = choice == 2;

            string assetPath = AssetDatabase.GetAssetPath(m_SelectedInfluencer);

            m_InfluencersProperty.GetArrayElementAtIndex(dbIndex).objectReferenceValue = null;
            m_InfluencersProperty.DeleteArrayElementAtIndex(dbIndex);
            m_DatabaseSO.ApplyModifiedProperties();
            EditorUtility.SetDirty(m_Database);
            AssetDatabase.SaveAssets();

            m_SelectedInfluencer = null;
            DestroySelectedEditor();

            if (deleteAsset && !string.IsNullOrEmpty(assetPath))
            {
                AssetDatabase.DeleteAsset(assetPath);
                AssetDatabase.SaveAssets();
            }

            Repaint();
        }

        private int FindDatabaseIndex(InfluencerData target)
        {
            for (int i = 0; i < m_InfluencersProperty.arraySize; i++)
            {
                if (m_InfluencersProperty.GetArrayElementAtIndex(i).objectReferenceValue == target)
                {
                    return i;
                }
            }
            return -1;
        }

        private void RefreshFromFolder()
        {
            InfluencerDatabaseEditorHelper.RefreshFromFolder(m_Database);
        }
    }
}
