using System.Collections.Generic;
using System.Linq;
using Final.InfluencerMatch.Common;
using Final.Systems.ConfigManagement;
using UnityEditor;
using UnityEngine;

namespace Final.Editor.Tools
{
    /// <summary>
    /// Editor window listing every ScriptableObject that implements <see cref="IVisibleConfig"/>.
    /// </summary>
    public class ConfigManagerWindow : EditorWindow
    {
        private const float k_LeftPanelWidth = 250f;
        private const float k_RefreshButtonHeight = 24f;
        private const float k_FooterButtonHeight = 28f;
        private const float k_DetailLabelWidth = 180f;
        private const string k_MenuItemPath = "Tools/" + GlobalEnvironmentVariables.AppName + "/Config Manager";
        private const string k_WindowTitle = "Config Manager";

        private static readonly Vector2 k_MinSize = new Vector2(600f, 400f);

        [SerializeField] private ScriptableObject m_SelectedConfig;

        private List<ScriptableObject> m_AllConfigs;
        private UnityEditor.Editor m_CachedEditor;
        private Vector2 m_LeftScrollPos;
        private Vector2 m_RightScrollPos;

        [MenuItem(k_MenuItemPath)]
        public static void Open()
        {
            ConfigManagerWindow window = GetWindow<ConfigManagerWindow>(k_WindowTitle);
            window.minSize = k_MinSize;
        }

        private void OnEnable()
        {
            wantsMouseMove = true;
            FindAllConfigs();
        }

        private void OnDisable()
        {
            DestroyCachedEditor();
        }

        private void OnGUI()
        {
            if (m_AllConfigs == null || m_AllConfigs.Count == 0)
            {
                EditorGUILayout.HelpBox("No configs found implementing IVisibleConfig.", MessageType.Info);
                if (GUILayout.Button("Refresh"))
                {
                    FindAllConfigs();
                }
                return;
            }

            EditorGUILayout.BeginHorizontal(GUILayout.ExpandHeight(true));
            DrawLeftPanel();
            DrawPanelDivider();
            DrawRightPanel();
            EditorGUILayout.EndHorizontal();
        }

        private void FindAllConfigs()
        {
            m_AllConfigs = AssetDatabase.FindAssets("t:ScriptableObject")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<ScriptableObject>)
                .Where(so => so is IVisibleConfig)
                .OrderBy(so => ((IVisibleConfig)so).Category)
                .ThenBy(so => ((IVisibleConfig)so).ConfigName)
                .ToList();

            if (m_AllConfigs.Count > 0 && m_SelectedConfig == null)
            {
                m_SelectedConfig = m_AllConfigs[0];
            }
        }

        private void DrawLeftPanel()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(k_LeftPanelWidth));

            GUILayout.Space(EditorStyleCache.Spacing16);
            DrawIndentedLabel("Configurations", EditorStyleCache.TitleMedium, 22f);
            GUILayout.Space(EditorStyleCache.Spacing8);
            DrawIndentedButton("Refresh", k_RefreshButtonHeight, FindAllConfigs);
            GUILayout.Space(EditorStyleCache.Spacing16);

            m_LeftScrollPos = EditorGUILayout.BeginScrollView(m_LeftScrollPos);

            string currentCategory = null;
            for (int i = 0; i < m_AllConfigs.Count; i++)
            {
                ScriptableObject config = m_AllConfigs[i];
                if (config is not IVisibleConfig visibleConfig)
                {
                    continue;
                }

                if (currentCategory != visibleConfig.Category)
                {
                    currentCategory = visibleConfig.Category;
                    DrawCategoryHeader(currentCategory);
                }

                DrawListRow(config, visibleConfig);
            }

            GUILayout.Space(EditorStyleCache.Spacing16);
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private static void DrawIndentedLabel(string text, GUIStyle style, float height)
        {
            Rect rect = EditorGUILayout.GetControlRect(false, height);
            rect.x += EditorStyleCache.Spacing16;
            rect.width -= EditorStyleCache.Spacing16 * 2f;
            EditorGUI.LabelField(rect, text, style);
        }

        private static void DrawIndentedButton(string text, float height, System.Action onClick)
        {
            Rect rect = EditorGUILayout.GetControlRect(false, height);
            rect.x += EditorStyleCache.Spacing16;
            rect.width -= EditorStyleCache.Spacing16 * 2f;
            if (GUI.Button(rect, text, EditorStyleCache.FlatActionButton))
            {
                onClick();
            }
        }

        private static void DrawCategoryHeader(string category)
        {
            GUILayout.Space(EditorStyleCache.Spacing12);
            Rect rect = EditorGUILayout.GetControlRect(false, EditorStyleCache.CategoryHeaderHeight);
            rect.x += EditorStyleCache.Spacing16;
            rect.width -= EditorStyleCache.Spacing16 * 2f;
            EditorGUI.LabelField(rect, category.ToUpper(), EditorStyleCache.SectionLabel);
        }

        private void DrawListRow(ScriptableObject config, IVisibleConfig visibleConfig)
        {
            bool isSelected = m_SelectedConfig == config;
            Rect rect = EditorGUILayout.GetControlRect(false, EditorStyleCache.RowHeight);

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

            float labelLeft = rect.x + EditorStyleCache.AccentBarWidth + EditorStyleCache.Spacing12;
            Rect labelRect = new Rect(labelLeft, rect.y, rect.xMax - labelLeft - EditorStyleCache.Spacing8, rect.height);
            EditorGUI.LabelField(labelRect, visibleConfig.ConfigName, isSelected ? EditorStyleCache.ListItemSelected : EditorStyleCache.ListItem);

            if (evt.type == EventType.MouseDown && evt.button == 0 && isHover)
            {
                SelectConfig(config);
                evt.Use();
            }
            else if (evt.type == EventType.MouseMove && isHover)
            {
                Repaint();
            }
        }

        private static void DrawPanelDivider()
        {
            Rect rect = EditorGUILayout.GetControlRect(GUILayout.Width(EditorStyleCache.DividerThickness), GUILayout.ExpandHeight(true));
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

            if (m_SelectedConfig == null)
            {
                GUILayout.Space(EditorStyleCache.Spacing24);
                EditorGUILayout.HelpBox("Please select a configuration from the left panel.", MessageType.Info);
                EditorGUILayout.EndVertical();
                return;
            }

            IVisibleConfig visibleConfig = (IVisibleConfig)m_SelectedConfig;

            GUILayout.Space(EditorStyleCache.Spacing24);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(EditorStyleCache.Spacing24);
            EditorGUILayout.BeginVertical();

            EditorGUILayout.LabelField(visibleConfig.ConfigName, EditorStyleCache.TitleLarge);
            EditorGUILayout.LabelField($"Category · {visibleConfig.Category}", EditorStyleCache.CaptionLabel);
            GUILayout.Space(EditorStyleCache.Spacing16);
            DrawHorizontalDivider();
            GUILayout.Space(EditorStyleCache.Spacing16);

            m_RightScrollPos = EditorGUILayout.BeginScrollView(m_RightScrollPos);
            EnsureCachedEditor();
            if (m_CachedEditor != null)
            {
                float oldLabelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = k_DetailLabelWidth;
                m_CachedEditor.OnInspectorGUI();
                EditorGUIUtility.labelWidth = oldLabelWidth;
            }
            EditorGUILayout.EndScrollView();

            GUILayout.Space(EditorStyleCache.Spacing16);
            DrawHorizontalDivider();
            GUILayout.Space(EditorStyleCache.Spacing12);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Ping in Project", EditorStyleCache.FlatActionButton, GUILayout.Height(k_FooterButtonHeight)))
            {
                EditorGUIUtility.PingObject(m_SelectedConfig);
            }
            GUILayout.Space(EditorStyleCache.Spacing8);
            if (GUILayout.Button("Select in Project", EditorStyleCache.FlatActionButton, GUILayout.Height(k_FooterButtonHeight)))
            {
                Selection.activeObject = m_SelectedConfig;
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(EditorStyleCache.Spacing16);
            EditorGUILayout.EndVertical();
            GUILayout.Space(EditorStyleCache.Spacing24);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void EnsureCachedEditor()
        {
            if (m_CachedEditor != null && m_CachedEditor.target == m_SelectedConfig)
            {
                return;
            }

            DestroyCachedEditor();
            if (m_SelectedConfig != null)
            {
                m_CachedEditor = UnityEditor.Editor.CreateEditor(m_SelectedConfig);
            }
        }

        private void DestroyCachedEditor()
        {
            if (m_CachedEditor != null)
            {
                DestroyImmediate(m_CachedEditor);
                m_CachedEditor = null;
            }
        }

        private void SelectConfig(ScriptableObject config)
        {
            if (m_SelectedConfig == config)
            {
                return;
            }

            m_SelectedConfig = config;
            DestroyCachedEditor();
            Repaint();
        }
    }
}
