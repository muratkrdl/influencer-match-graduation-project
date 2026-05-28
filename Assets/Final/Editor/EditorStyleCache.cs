using UnityEditor;
using UnityEngine;

namespace Final.Editor
{
    /// <summary>
    /// Lazy-initialized shared GUIStyles, palette, and spacing constants used by Final's editor windows.
    /// </summary>
    public static class EditorStyleCache
    {
        public static readonly Color AccentColor = new Color(0.24f, 0.48f, 0.90f, 1f);
        public static readonly Color AccentSelectionTint = new Color(0.24f, 0.48f, 0.90f, 0.18f);
        public static readonly Color HoverRowColor = new Color(1f, 1f, 1f, 0.04f);
        public static readonly Color SeparatorColor = new Color(1f, 1f, 1f, 0.06f);
        public static readonly Color PanelDividerColor = new Color(0f, 0f, 0f, 0.35f);
        public static readonly Color HeaderBackgroundColor = new Color(0.18f, 0.18f, 0.18f, 1f);
        public static readonly Color AlternateRowColor = new Color(0f, 0f, 0f, 0.04f);
        public static readonly Color IconPlaceholderColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
        public static readonly Color MutedTextColor = new Color(0.65f, 0.65f, 0.65f);
        public static readonly Color SubtleTextColor = new Color(0.55f, 0.55f, 0.55f);
        public static readonly Color CategoryHeaderTextColor = new Color(0.5f, 0.5f, 0.5f);
        public static readonly Color SelectedTextColor = new Color(0.95f, 0.95f, 0.95f);

        public const float Spacing4 = 4f;
        public const float Spacing8 = 8f;
        public const float Spacing12 = 12f;
        public const float Spacing16 = 16f;
        public const float Spacing24 = 24f;

        public const float RowHeight = 30f;
        public const float CategoryHeaderHeight = 22f;
        public const float AccentBarWidth = 3f;
        public const float DividerThickness = 1f;

        private static GUIStyle s_TitleLarge;
        private static GUIStyle s_TitleMedium;
        private static GUIStyle s_SectionLabel;
        private static GUIStyle s_CaptionLabel;
        private static GUIStyle s_ListItem;
        private static GUIStyle s_ListItemSelected;
        private static GUIStyle s_MutedMiniLabel;
        private static GUIStyle s_RightAlignedBoldLabel;
        private static GUIStyle s_FlatActionButton;

        public static GUIStyle TitleLarge => s_TitleLarge ??= new GUIStyle(EditorStyles.label)
        {
            fontSize = 18,
            fontStyle = FontStyle.Bold,
            padding = new RectOffset(0, 0, 4, 2)
        };

        public static GUIStyle TitleMedium => s_TitleMedium ??= new GUIStyle(EditorStyles.label)
        {
            fontSize = 13,
            fontStyle = FontStyle.Bold,
            padding = new RectOffset(0, 0, 2, 2)
        };

        public static GUIStyle SectionLabel => s_SectionLabel ??= new GUIStyle(EditorStyles.miniLabel)
        {
            fontSize = 10,
            fontStyle = FontStyle.Bold,
            normal = { textColor = CategoryHeaderTextColor },
            padding = new RectOffset(0, 0, 0, 0)
        };

        public static GUIStyle CaptionLabel => s_CaptionLabel ??= new GUIStyle(EditorStyles.label)
        {
            fontSize = 11,
            normal = { textColor = MutedTextColor },
            padding = new RectOffset(0, 0, 0, 0)
        };

        public static GUIStyle ListItem => s_ListItem ??= new GUIStyle(EditorStyles.label)
        {
            fontSize = 12,
            alignment = TextAnchor.MiddleLeft,
            padding = new RectOffset(0, 8, 0, 0)
        };

        public static GUIStyle ListItemSelected => s_ListItemSelected ??= new GUIStyle(EditorStyles.label)
        {
            fontSize = 12,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft,
            normal = { textColor = SelectedTextColor },
            padding = new RectOffset(0, 8, 0, 0)
        };

        public static GUIStyle MutedMiniLabel => s_MutedMiniLabel ??= new GUIStyle(EditorStyles.miniLabel)
        {
            normal = { textColor = MutedTextColor }
        };

        public static GUIStyle RightAlignedBoldLabel => s_RightAlignedBoldLabel ??= new GUIStyle(EditorStyles.boldLabel)
        {
            alignment = TextAnchor.MiddleRight
        };

        public static GUIStyle FlatActionButton => s_FlatActionButton ??= new GUIStyle(EditorStyles.miniButton)
        {
            fontSize = 12,
            fontStyle = FontStyle.Bold,
            fixedHeight = 0f,
            padding = new RectOffset(12, 12, 6, 6)
        };

        public static void DrawSelectionHighlight(Rect rowRect)
        {
            EditorGUI.DrawRect(rowRect, AccentSelectionTint);
            Rect accentBar = new Rect(rowRect.x, rowRect.y, AccentBarWidth, rowRect.height);
            EditorGUI.DrawRect(accentBar, AccentColor);
        }

        public static void DrawHoverHighlight(Rect rowRect)
        {
            EditorGUI.DrawRect(rowRect, HoverRowColor);
        }

        [InitializeOnLoadMethod]
        private static void RegisterReloadHandlers()
        {
            AssemblyReloadEvents.beforeAssemblyReload += ClearCache;
        }

        private static void ClearCache()
        {
            s_TitleLarge = null;
            s_TitleMedium = null;
            s_SectionLabel = null;
            s_CaptionLabel = null;
            s_ListItem = null;
            s_ListItemSelected = null;
            s_MutedMiniLabel = null;
            s_RightAlignedBoldLabel = null;
            s_FlatActionButton = null;
        }
    }
}
