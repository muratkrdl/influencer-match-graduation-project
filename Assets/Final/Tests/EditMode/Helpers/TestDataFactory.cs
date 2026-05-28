using System.Collections.Generic;
using Final.InfluencerMatch.Common;
using Final.InfluencerMatch.Recommendation;
using UnityEditor;
using UnityEngine;

namespace Final.Tests.Helpers
{
    /// <summary>
    /// Builds ScriptableObject instances for EditMode tests using SerializedObject so private serialized fields can be filled without modifying production setters.
    /// </summary>
    internal static class TestDataFactory
    {
        private static Sprite s_DummyAvatar;

        private static Sprite GetDummyAvatar()
        {
            if (s_DummyAvatar == null)
            {
                Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                texture.name = "TestDummyAvatarTexture";
                s_DummyAvatar = Sprite.Create(texture, new Rect(0f, 0f, 2f, 2f), new Vector2(0.5f, 0.5f));
                s_DummyAvatar.name = "TestDummyAvatar";
            }
            return s_DummyAvatar;
        }

        public static InfluencerData CreateInfluencer(
            string id,
            string displayName,
            int followers,
            float engagementRate,
            int basePrice,
            params (CategoryId category, int score)[] scores)
        {
            InfluencerData data = ScriptableObject.CreateInstance<InfluencerData>();
            data.name = id;

            // m_Id is auto-initialised to a fresh SerializableGuid via the field
            // initializer in InfluencerData; tests compare by reference, not by Id.
            SerializedObject so = new SerializedObject(data);
            so.FindProperty("m_DisplayName").stringValue = displayName;
            so.FindProperty("m_Handle").stringValue = "@" + id;
            so.FindProperty("m_Email").stringValue = id + "@test.com";
            so.FindProperty("m_ShortBio").stringValue = "test";
            so.FindProperty("m_Followers").intValue = followers;
            so.FindProperty("m_EngagementRate").floatValue = engagementRate;
            so.FindProperty("m_BasePrice").intValue = basePrice;
            so.FindProperty("m_Avatar").objectReferenceValue = GetDummyAvatar();

            SerializedProperty scoresProperty = so.FindProperty("m_CategoryScores");
            scoresProperty.arraySize = scores != null ? scores.Length : 0;
            if (scores != null)
            {
                for (int i = 0; i < scores.Length; i++)
                {
                    SerializedProperty element = scoresProperty.GetArrayElementAtIndex(i);
                    element.FindPropertyRelative("m_Category").enumValueIndex = (int)scores[i].category;
                    element.FindPropertyRelative("m_Score").intValue = scores[i].score;
                }
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            return data;
        }

        public static MatchingConfig CreateDefaultMatchingConfig()
        {
            MatchingConfig config = ScriptableObject.CreateInstance<MatchingConfig>();
            config.name = "TestMatchingConfig";

            SerializedObject so = new SerializedObject(config);
            so.FindProperty("m_CategoryWeight").floatValue = 0.5f;
            so.FindProperty("m_FollowersWeight").floatValue = 0.3f;
            so.FindProperty("m_EngagementWeight").floatValue = 0.2f;
            so.FindProperty("m_FollowerNormalizationMin").intValue = 1000;
            so.FindProperty("m_FollowerNormalizationMax").intValue = 10_000_000;
            so.FindProperty("m_OverBudgetPenalty").floatValue = 0.5f;
            so.FindProperty("m_PriceRoundingTL").intValue = 100;
            so.FindProperty("m_MinimumCategoryScoreToInclude").intValue = 1;

            SerializedProperty scoreMultipliers = so.FindProperty("m_CategoryScoreMultipliers");
            (int score, float multiplier)[] scoreEntries =
            {
                (1, 0.7f),
                (2, 0.85f),
                (3, 1.0f),
                (4, 1.2f),
                (5, 1.5f)
            };
            scoreMultipliers.arraySize = scoreEntries.Length;
            for (int i = 0; i < scoreEntries.Length; i++)
            {
                SerializedProperty element = scoreMultipliers.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("m_Score").intValue = scoreEntries[i].score;
                element.FindPropertyRelative("m_Multiplier").floatValue = scoreEntries[i].multiplier;
            }

            SerializedProperty tierMultipliers = so.FindProperty("m_FollowerTierMultipliers");
            (int min, int max, float multiplier, string label)[] tierEntries =
            {
                (0, 1_000, 0.5f, "nano"),
                (1_000, 10_000, 0.8f, "micro"),
                (10_000, 100_000, 1.0f, "medium"),
                (100_000, 1_000_000, 1.5f, "macro"),
                (1_000_000, 50_000_000, 2.0f, "mega")
            };
            tierMultipliers.arraySize = tierEntries.Length;
            for (int i = 0; i < tierEntries.Length; i++)
            {
                SerializedProperty element = tierMultipliers.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("m_MinFollowersInclusive").intValue = tierEntries[i].min;
                element.FindPropertyRelative("m_MaxFollowersExclusive").intValue = tierEntries[i].max;
                element.FindPropertyRelative("m_Multiplier").floatValue = tierEntries[i].multiplier;
                element.FindPropertyRelative("m_Label").stringValue = tierEntries[i].label;
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            return config;
        }

        public static List<InfluencerData> CreatePool(int count)
        {
            List<InfluencerData> pool = new List<InfluencerData>(count);
            for (int i = 0; i < count; i++)
            {
                string id = "inf_" + i.ToString("D3");
                InfluencerData data = CreateInfluencer(
                    id,
                    "Influencer " + i,
                    followers: 5_000 + (i * 1_000),
                    engagementRate: 0.05f,
                    basePrice: 5_000,
                    (CategoryId.Education, ((i % 5) + 1)),
                    (CategoryId.Technology, (((i + 2) % 5) + 1)),
                    (CategoryId.Sports, (((i + 3) % 5) + 1)));
                pool.Add(data);
            }
            return pool;
        }

        public static void DestroyAll(IList<InfluencerData> pool)
        {
            if (pool == null)
            {
                return;
            }

            for (int i = 0; i < pool.Count; i++)
            {
                if (pool[i] != null)
                {
                    Object.DestroyImmediate(pool[i]);
                }
            }
            pool.Clear();
        }
    }
}
