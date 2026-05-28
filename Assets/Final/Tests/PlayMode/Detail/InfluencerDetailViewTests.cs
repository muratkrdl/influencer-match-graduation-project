using System.Collections;
using System.Collections.Generic;
using Final.InfluencerMatch.Common;
using Final.InfluencerMatch.Detail;
using Final.InfluencerMatch.Recommendation;
using Final.Tests.PlayMode.Helpers;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using VContainer;

namespace Final.Tests.PlayMode.Detail
{
    /// <summary>
    /// PlayMode tests for <see cref="InfluencerDetailView.DisplayInfluencer"/>: text/avatar
    /// binding, bio section visibility, compatibility clamping, and one score-bar per
    /// non-None category from the supplied CategoryConfig.
    /// </summary>
    public sealed class InfluencerDetailViewTests
    {
        private readonly List<UnityEngine.Object> m_Owned = new List<UnityEngine.Object>();
        private GameObject m_Object;
        private GameObject m_ScoreBarPrefab;
        private GameObject m_PlatformIconPrefab;
        private InfluencerDetailView m_View;
        private Image m_AvatarImage;
        private TMP_Text m_NameText;
        private TMP_Text m_HandleText;
        private TMP_Text m_EmailText;
        private TMP_Text m_FollowersText;
        private TMP_Text m_EngagementText;
        private TMP_Text m_CompatibilityText;
        private TMP_Text m_PriceValueText;
        private GameObject m_BioSection;
        private TMP_Text m_BioText;
        private Transform m_ScoreBarsContainer;
        private Transform m_PlatformsContainer;

        [TearDown]
        public void TearDown()
        {
            if (m_Object != null)
            {
                Object.Destroy(m_Object);
                m_Object = null;
            }
            if (m_ScoreBarPrefab != null)
            {
                Object.Destroy(m_ScoreBarPrefab);
                m_ScoreBarPrefab = null;
            }
            if (m_PlatformIconPrefab != null)
            {
                Object.Destroy(m_PlatformIconPrefab);
                m_PlatformIconPrefab = null;
            }
            for (int i = 0; i < m_Owned.Count; i++)
            {
                if (m_Owned[i] != null)
                {
                    Object.DestroyImmediate(m_Owned[i]);
                }
            }
            m_Owned.Clear();
        }

        [UnityTest]
        public IEnumerator DisplayInfluencer_BindsProfileTextsAndPrice()
        {
            BuildView(scoreBarConfig: NewScoreBarConfig(), out CategoryConfig categoryConfig);
            SeedCategoryConfig(categoryConfig, CategoryId.Education);
            InfluencerData influencer = NewInfluencer(displayName: "Ada", handle: "@ada", email: "ada@test.com", followers: 12_345, bio: "Hello");
            m_View.DisplayInfluencer(influencer, compatibilityPercent: 42, engagementCount: 0, finalPrice: 7_500, EmptyPlatforms(), new[] { CategoryId.Education }, categoryConfig);
            yield return null;

            Assert.AreEqual("Ada", m_NameText.text);
            Assert.AreEqual("@ada", m_HandleText.text);
            Assert.AreEqual("ada@test.com", m_EmailText.text);
            Assert.AreEqual("$7,500", m_PriceValueText.text);
            StringAssert.Contains("12.3K", m_FollowersText.text);
        }

        [UnityTest]
        public IEnumerator DisplayInfluencer_NonEmptyBio_ActivatesBioSection()
        {
            BuildView(scoreBarConfig: NewScoreBarConfig(), out CategoryConfig categoryConfig);
            InfluencerData influencer = NewInfluencer(bio: "About me.");

            m_View.DisplayInfluencer(influencer, 50, 0, 0, EmptyPlatforms(), new List<CategoryId>(), categoryConfig);
            yield return null;

            Assert.IsTrue(m_BioSection.activeSelf);
            Assert.AreEqual("About me.", m_BioText.text);
        }

        [UnityTest]
        public IEnumerator DisplayInfluencer_EmptyBio_HidesBioSection()
        {
            BuildView(scoreBarConfig: NewScoreBarConfig(), out CategoryConfig categoryConfig);
            InfluencerData influencer = NewInfluencer(bio: "");

            m_View.DisplayInfluencer(influencer, 50, 0, 0, EmptyPlatforms(), new List<CategoryId>(), categoryConfig);
            yield return null;

            Assert.IsFalse(m_BioSection.activeSelf);
        }

        [UnityTest]
        public IEnumerator DisplayInfluencer_CompatibilityAbove100_ClampsToHundred()
        {
            BuildView(scoreBarConfig: NewScoreBarConfig(), out CategoryConfig categoryConfig);
            InfluencerData influencer = NewInfluencer();

            m_View.DisplayInfluencer(influencer, compatibilityPercent: 150, engagementCount: 0, finalPrice: 0, EmptyPlatforms(), new List<CategoryId>(), categoryConfig);
            yield return null;

            Assert.AreEqual("%100", m_CompatibilityText.text);
        }

        [UnityTest]
        public IEnumerator DisplayInfluencer_CompatibilityBelowZero_ClampsToZero()
        {
            BuildView(scoreBarConfig: NewScoreBarConfig(), out CategoryConfig categoryConfig);
            InfluencerData influencer = NewInfluencer();

            m_View.DisplayInfluencer(influencer, compatibilityPercent: -25, engagementCount: 0, finalPrice: 0, EmptyPlatforms(), new List<CategoryId>(), categoryConfig);
            yield return null;

            Assert.AreEqual("%0", m_CompatibilityText.text);
        }

        [UnityTest]
        public IEnumerator DisplayInfluencer_SpawnsOneScoreBar_PerNonNoneCategory()
        {
            ScoreBarConfig scoreBarConfig = NewScoreBarConfig();
            BuildView(scoreBarConfig, out CategoryConfig categoryConfig);
            SeedCategoryConfig(categoryConfig, CategoryId.Education, CategoryId.Sports, CategoryId.Technology);
            InfluencerData influencer = NewInfluencer();

            m_View.DisplayInfluencer(influencer, 50, 0, 0, EmptyPlatforms(), new[] { CategoryId.Education }, categoryConfig);
            yield return null;

            int spawnedBars = 0;
            for (int i = 0; i < m_ScoreBarsContainer.childCount; i++)
            {
                if (m_ScoreBarsContainer.GetChild(i).GetComponent<CategoryScoreBarView>() != null)
                {
                    spawnedBars++;
                }
            }
            Assert.AreEqual(3, spawnedBars);
        }

        [UnityTest]
        public IEnumerator DisplayInfluencer_SpawnsOnePlatformIcon_PerActivePlatform()
        {
            BuildView(scoreBarConfig: NewScoreBarConfig(), out CategoryConfig categoryConfig);
            InfluencerData influencer = NewInfluencer();
            PlatformDefinition[] platforms = new[]
            {
                NewPlatformDefinition(PlatformId.Instagram, "Instagram"),
                NewPlatformDefinition(PlatformId.YouTube, "YouTube"),
                NewPlatformDefinition(PlatformId.TikTok, "TikTok"),
            };

            m_View.DisplayInfluencer(influencer, 50, 0, 0, platforms, new List<CategoryId>(), categoryConfig);
            yield return null;

            int spawnedIcons = 0;
            for (int i = 0; i < m_PlatformsContainer.childCount; i++)
            {
                if (m_PlatformsContainer.GetChild(i).GetComponent<PlatformIconView>() != null)
                {
                    spawnedIcons++;
                }
            }
            Assert.AreEqual(3, spawnedIcons);
        }

        [UnityTest]
        public IEnumerator DisplayInfluencer_RebindClearsPreviousPlatforms()
        {
            BuildView(scoreBarConfig: NewScoreBarConfig(), out CategoryConfig categoryConfig);
            InfluencerData influencer = NewInfluencer();
            PlatformDefinition[] first = new[]
            {
                NewPlatformDefinition(PlatformId.Instagram, "Instagram"),
                NewPlatformDefinition(PlatformId.YouTube, "YouTube"),
            };
            PlatformDefinition[] second = new[]
            {
                NewPlatformDefinition(PlatformId.TikTok, "TikTok"),
            };

            m_View.DisplayInfluencer(influencer, 50, 0, 0, first, new List<CategoryId>(), categoryConfig);
            yield return null;
            m_View.DisplayInfluencer(influencer, 50, 0, 0, second, new List<CategoryId>(), categoryConfig);
            yield return null;

            int activeIcons = 0;
            for (int i = 0; i < m_PlatformsContainer.childCount; i++)
            {
                Transform child = m_PlatformsContainer.GetChild(i);
                if (child.GetComponent<PlatformIconView>() != null && child.gameObject.activeSelf)
                {
                    activeIcons++;
                }
            }
            Assert.AreEqual(1, activeIcons);
        }

        private InfluencerData NewInfluencer(string displayName = "A", string handle = "@a", string email = "a@test.com", int followers = 1_000, string bio = "")
        {
            InfluencerData data = ScriptableObject.CreateInstance<InfluencerData>();
            data.name = displayName;
            m_Owned.Add(data);
            ReflectionHelpers.SetPrivateField(data, "m_DisplayName", displayName);
            ReflectionHelpers.SetPrivateField(data, "m_Handle", handle);
            ReflectionHelpers.SetPrivateField(data, "m_Email", email);
            ReflectionHelpers.SetPrivateField(data, "m_Followers", followers);
            ReflectionHelpers.SetPrivateField(data, "m_ShortBio", bio);
            return data;
        }

        private void SeedCategoryConfig(CategoryConfig config, params CategoryId[] ids)
        {
            System.Reflection.FieldInfo field = typeof(CategoryConfig).GetField("m_Categories", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.IsNotNull(field, "m_Categories field not found on CategoryConfig.");
            List<CategoryDefinition> list = (List<CategoryDefinition>)field.GetValue(config);
            foreach (CategoryId id in ids)
            {
                list.Add(ReflectionHelpers.CreateCategoryDefinition(id, id.ToString()));
            }
        }

        private ScoreBarConfig NewScoreBarConfig()
        {
            ScoreBarConfig config = ScriptableObject.CreateInstance<ScoreBarConfig>();
            m_Owned.Add(config);
            return config;
        }

        private void BuildView(ScoreBarConfig scoreBarConfig, out CategoryConfig categoryConfig)
        {
            m_ScoreBarPrefab = BuildScoreBarPrefab();
            CategoryScoreBarView scoreBarPrefabComponent = m_ScoreBarPrefab.GetComponent<CategoryScoreBarView>();
            m_PlatformIconPrefab = BuildPlatformIconPrefab();
            PlatformIconView platformIconPrefabComponent = m_PlatformIconPrefab.GetComponent<PlatformIconView>();

            categoryConfig = ScriptableObject.CreateInstance<CategoryConfig>();
            m_Owned.Add(categoryConfig);
            UISharedConfig sharedConfig = ScriptableObject.CreateInstance<UISharedConfig>();
            m_Owned.Add(sharedConfig);

            ContainerBuilder containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterInstance(scoreBarConfig).AsSelf();
            IObjectResolver container = containerBuilder.Build();

            m_Object = new GameObject(nameof(InfluencerDetailViewTests));
            m_Object.SetActive(false);

            CanvasGroup canvasGroup = m_Object.AddComponent<CanvasGroup>();
            m_AvatarImage = NewChildWithComponent<Image>(m_Object, "Avatar");
            m_NameText = NewChildWithComponent<TextMeshProUGUI>(m_Object, "Name");
            m_HandleText = NewChildWithComponent<TextMeshProUGUI>(m_Object, "Handle");
            m_EmailText = NewChildWithComponent<TextMeshProUGUI>(m_Object, "Email");
            m_FollowersText = NewChildWithComponent<TextMeshProUGUI>(m_Object, "Followers");
            m_EngagementText = NewChildWithComponent<TextMeshProUGUI>(m_Object, "Engagement");
            m_CompatibilityText = NewChildWithComponent<TextMeshProUGUI>(m_Object, "Compatibility");
            m_PriceValueText = NewChildWithComponent<TextMeshProUGUI>(m_Object, "PriceValue");
            m_ScoreBarsContainer = NewChild(m_Object, "ScoreBars").transform;
            m_PlatformsContainer = NewChild(m_Object, "Platforms").transform;
            m_BioSection = NewChild(m_Object, "BioSection");
            m_BioText = NewChildWithComponent<TextMeshProUGUI>(m_BioSection, "BioText");
            Button backButton = NewChildWithComponent<Button>(m_Object, "Back");
            TMP_Text titleText = NewChildWithComponent<TextMeshProUGUI>(m_Object, "Title");

            m_View = m_Object.AddComponent<InfluencerDetailView>();
            ReflectionHelpers.SetPrivateField(m_View, "m_AvatarImage", m_AvatarImage);
            ReflectionHelpers.SetPrivateField(m_View, "m_NameText", m_NameText);
            ReflectionHelpers.SetPrivateField(m_View, "m_HandleText", m_HandleText);
            ReflectionHelpers.SetPrivateField(m_View, "m_EmailText", m_EmailText);
            ReflectionHelpers.SetPrivateField(m_View, "m_FollowersText", m_FollowersText);
            ReflectionHelpers.SetPrivateField(m_View, "m_EngagementText", m_EngagementText);
            ReflectionHelpers.SetPrivateField(m_View, "m_CompatibilityPercentText", m_CompatibilityText);
            ReflectionHelpers.SetPrivateField(m_View, "m_PriceValueText", m_PriceValueText);
            ReflectionHelpers.SetPrivateField(m_View, "m_ScoreBarsContainer", m_ScoreBarsContainer);
            ReflectionHelpers.SetPrivateField(m_View, "m_ScoreBarPrefab", scoreBarPrefabComponent);
            ReflectionHelpers.SetPrivateField(m_View, "m_PlatformsContainer", m_PlatformsContainer);
            ReflectionHelpers.SetPrivateField(m_View, "m_PlatformIconPrefab", platformIconPrefabComponent);
            ReflectionHelpers.SetPrivateField(m_View, "m_BioSection", m_BioSection);
            ReflectionHelpers.SetPrivateField(m_View, "m_BioText", m_BioText);
            ReflectionHelpers.SetPrivateField(m_View, "m_BackButton", backButton);
            ReflectionHelpers.SetPrivateField(m_View, "m_TitleText", titleText);
            ReflectionHelpers.SetPrivateField(m_View, "m_CanvasGroup", canvasGroup);
            ReflectionHelpers.SetPrivateField(m_View, "m_SharedConfig", sharedConfig);
            ReflectionHelpers.SetPrivateField(m_View, "m_Container", container);

            m_Object.SetActive(true);
        }

        private GameObject BuildPlatformIconPrefab()
        {
            GameObject root = new GameObject("PlatformIconPrefab");
            Image icon = root.AddComponent<Image>();
            PlatformIconView view = root.AddComponent<PlatformIconView>();
            ReflectionHelpers.SetPrivateField(view, "m_IconImage", icon);
            return root;
        }

        private static IReadOnlyList<PlatformDefinition> EmptyPlatforms()
        {
            return System.Array.Empty<PlatformDefinition>();
        }

        private static PlatformDefinition NewPlatformDefinition(PlatformId id, string displayName)
        {
            PlatformDefinition def = new PlatformDefinition();
            ReflectionHelpers.SetPrivateField(def, "m_Id", id);
            ReflectionHelpers.SetPrivateField(def, "m_DisplayName", displayName);
            return def;
        }

        private GameObject BuildScoreBarPrefab()
        {
            GameObject root = new GameObject("ScoreBarPrefab");
            root.SetActive(false);

            Image icon = NewChildWithComponent<Image>(root, "Icon");
            TMP_Text label = NewChildWithComponent<TextMeshProUGUI>(root, "Label");
            Image highlight = NewChildWithComponent<Image>(root, "Highlight");
            // Dots array — Bind iterates it; we provide a non-null array of size = DotCount default (10).
            Image[] dots = new Image[10];
            for (int i = 0; i < dots.Length; i++)
            {
                dots[i] = NewChildWithComponent<Image>(root, "Dot_" + i);
            }

            CategoryScoreBarView bar = root.AddComponent<CategoryScoreBarView>();
            ReflectionHelpers.SetPrivateField(bar, "m_IconImage", icon);
            ReflectionHelpers.SetPrivateField(bar, "m_LabelText", label);
            ReflectionHelpers.SetPrivateField(bar, "m_HighlightBackground", highlight);
            ReflectionHelpers.SetPrivateField(bar, "m_ScoreDots", dots);

            return root;
        }

        private static GameObject NewChild(GameObject parent, string name)
        {
            GameObject child = new GameObject(name);
            child.transform.SetParent(parent.transform);
            return child;
        }

        private static T NewChildWithComponent<T>(GameObject parent, string name) where T : Component
        {
            return NewChild(parent, name).AddComponent<T>();
        }
    }
}
