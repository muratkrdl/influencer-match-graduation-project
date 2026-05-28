using System;
using System.Collections.Generic;
using System.Reflection;
using Final.InfluencerMatch.Common;
using Final.InfluencerMatch.Recommendation;
using NUnit.Framework;
using UnityEngine;

namespace Final.Tests.PlayMode.Helpers
{
    /// <summary>
    /// Reflection helpers for PlayMode tests that need to fill [SerializeField] / [Inject]
    /// private fields before the Unity message loop fires Awake / OnEnable on the host
    /// GameObject. Walks the type hierarchy so inherited private fields (e.g. UIPanelBase's
    /// m_CanvasGroup / m_SharedConfig) are reachable from concrete subclasses.
    /// </summary>
    internal static class ReflectionHelpers
    {
        public static void SetPrivateField(object target, string fieldName, object value)
        {
            Type type = target.GetType();
            while (type != null)
            {
                FieldInfo field = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
                if (field != null)
                {
                    field.SetValue(target, value);
                    return;
                }
                type = type.BaseType;
            }
            Assert.Fail($"Field '{fieldName}' not found on {target.GetType().Name} or any base type.");
        }

        public static CategoryDefinition CreateCategoryDefinition(CategoryId id, string displayName, Sprite icon = null, Color? accentColor = null)
        {
            CategoryDefinition def = new CategoryDefinition();
            SetPrivateField(def, "m_Id", id);
            SetPrivateField(def, "m_DisplayName", displayName);
            if (icon != null)
            {
                SetPrivateField(def, "m_Icon", icon);
            }
            if (accentColor.HasValue)
            {
                SetPrivateField(def, "m_AccentColor", accentColor.Value);
            }
            return def;
        }

        public static void AddCategoryScore(InfluencerData influencer, CategoryId category, int score)
        {
            FieldInfo field = typeof(InfluencerData).GetField("m_CategoryScores", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(field, "m_CategoryScores field not found on InfluencerData.");
            List<CategoryScoreEntry> list = (List<CategoryScoreEntry>)field.GetValue(influencer);
            list.Add(new CategoryScoreEntry(category, score));
        }
    }
}
