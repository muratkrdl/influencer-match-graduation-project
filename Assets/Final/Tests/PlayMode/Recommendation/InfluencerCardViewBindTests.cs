using System.Collections;
using System.Collections.Generic;
using Final.InfluencerMatch.Budget;
using Final.InfluencerMatch.Common;
using Final.InfluencerMatch.Recommendation;
using Final.Tests.PlayMode.Helpers;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Final.Tests.PlayMode.Recommendation
{
    /// <summary>
    /// PlayMode tests for <see cref="InfluencerCardView"/>'s Bind pipeline: over-budget
    /// alpha tinting, normal alpha pass-through, and chip pool reuse across re-binds.
    /// Companion to InfluencerCardViewTapTests which covers the tap → tween → event path.
    /// </summary>
    public sealed class InfluencerCardViewBindTests
    {
        private readonly List<Object> m_OwnedObjects = new List<Object>();
        private GameObject m_Object;
        private InfluencerCardView m_Card;
        private RecommendationConfig m_Config;
        private CategoryConfig m_CategoryConfig;

        // Card serialized refs (kept for assertions).
        private Image m_BackgroundImage;
        private Image m_AvatarImage;
        private TMP_Text m_NameText;
        private TMP_Text m_EmailText;
        private TMP_Text m_FollowersText;
        private TMP_Text m_CompatibilityText;
        private Transform m_ChipsContainer;

        [TearDown]
        public void TearDown()
        {
            if (m_Object != null)
            {
                Object.Destroy(m_Object);
                m_Object = null;
            }
            for (int i = 0; i < m_OwnedObjects.Count; i++)
            {
                if (m_OwnedObjects[i] != null)
                {
                    Object.DestroyImmediate(m_OwnedObjects[i]);
                }
            }
            m_OwnedObjects.Clear();
        }

        [UnityTest]
        public IEnumerator Bind_OverBudget_AppliesDisabledAlphaToAllGraphics()
        {
            BuildCard();
            InfluencerData influencer = NewInfluencer();
            ScoredInfluencer scored = new ScoredInfluencer(influencer, 0.5f, isOverBudget: true);

            m_Card.Bind(scored, m_CategoryConfig);
            yield return null;

            AssertAlphaMatchesAll(m_Config.DisabledAlpha);
        }

        [UnityTest]
        public IEnumerator Bind_NotOverBudget_AppliesNormalAlphaToAllGraphics()
        {
            BuildCard();
            InfluencerData influencer = NewInfluencer();
            ScoredInfluencer scored = new ScoredInfluencer(influencer, 0.5f, isOverBudget: false);

            m_Card.Bind(scored, m_CategoryConfig);
            yield return null;

            AssertAlphaMatchesAll(m_Config.NormalAlpha);
        }

        [UnityTest]
        public IEnumerator Bind_TwiceWithSameCategoryScores_ChipPoolReusesChipInstances()
        {
            // Two CategoryScores => two chips spawned. Re-Bind with the same data should
            // reuse the previously released chip GameObjects via the per-card chip pool.
            BuildCard();
            SeedCategoryConfig(CategoryId.Education, CategoryId.Sports);
            InfluencerData influencer = NewInfluencer();
            ReflectionHelpers.AddCategoryScore(influencer, CategoryId.Education, 5);
            ReflectionHelpers.AddCategoryScore(influencer, CategoryId.Sports, 4);
            ScoredInfluencer scored = new ScoredInfluencer(influencer, 0.5f, isOverBudget: false);

            m_Card.Bind(scored, m_CategoryConfig);
            yield return null;
            HashSet<int> firstChipIds = CaptureChipInstanceIds();
            Assume.That(firstChipIds.Count, Is.EqualTo(2), "First Bind should spawn one chip per scored category.");

            m_Card.Bind(scored, m_CategoryConfig);
            yield return null;
            HashSet<int> secondChipIds = CaptureChipInstanceIds();

            Assert.AreEqual(2, secondChipIds.Count, "Second Bind should still surface two chips.");
            secondChipIds.IntersectWith(firstChipIds);
            Assert.AreEqual(2, secondChipIds.Count, "Chip pool must reuse the same InstanceIDs across re-binds.");
        }

        private void AssertAlphaMatchesAll(float expectedAlpha)
        {
            Assert.AreEqual(expectedAlpha, m_BackgroundImage.color.a, 0.001f, "BackgroundImage alpha mismatch.");
            Assert.AreEqual(expectedAlpha, m_AvatarImage.color.a, 0.001f, "AvatarImage alpha mismatch.");
            Assert.AreEqual(expectedAlpha, m_NameText.color.a, 0.001f, "NameText alpha mismatch.");
            Assert.AreEqual(expectedAlpha, m_EmailText.color.a, 0.001f, "EmailText alpha mismatch.");
            Assert.AreEqual(expectedAlpha, m_FollowersText.color.a, 0.001f, "FollowersText alpha mismatch.");
            Assert.AreEqual(expectedAlpha, m_CompatibilityText.color.a, 0.001f, "CompatibilityText alpha mismatch.");
        }

        private HashSet<int> CaptureChipInstanceIds()
        {
            HashSet<int> ids = new HashSet<int>();
            for (int i = 0; i < m_ChipsContainer.childCount; i++)
            {
                Transform child = m_ChipsContainer.GetChild(i);
                if (child.gameObject.activeSelf)
                {
                    ids.Add(child.gameObject.GetInstanceID());
                }
            }
            return ids;
        }

        private InfluencerData NewInfluencer()
        {
            InfluencerData data = ScriptableObject.CreateInstance<InfluencerData>();
            m_OwnedObjects.Add(data);
            return data;
        }

        private void SeedCategoryConfig(params CategoryId[] ids)
        {
            FieldInfoBackedCategoryList list = new FieldInfoBackedCategoryList(m_CategoryConfig);
            foreach (CategoryId id in ids)
            {
                list.Add(ReflectionHelpers.CreateCategoryDefinition(id, id.ToString()));
            }
        }

        private void BuildCard()
        {
            m_Object = new GameObject(nameof(InfluencerCardViewBindTests));
            m_Object.SetActive(false);

            m_BackgroundImage = NewChildWithComponent<Image>("Background");
            m_AvatarImage = NewChildWithComponent<Image>("Avatar");
            m_NameText = NewChildWithComponent<TextMeshProUGUI>("Name");
            m_EmailText = NewChildWithComponent<TextMeshProUGUI>("Email");
            m_FollowersText = NewChildWithComponent<TextMeshProUGUI>("Followers");
            m_CompatibilityText = NewChildWithComponent<TextMeshProUGUI>("Compatibility");
            m_ChipsContainer = NewChild("ChipsContainer").transform;
            Button clickButton = NewChildWithComponent<Button>("ClickButton");
            CanvasGroup canvasGroup = m_Object.AddComponent<CanvasGroup>();

            // Chip prefab — RebuildChips Instantiate's this when CategoryScores are non-empty.
            GameObject chipPrefabObj = new GameObject("ChipPrefab");
            chipPrefabObj.SetActive(false);
            Image chipBg = AddChildImage(chipPrefabObj, "Bg");
            TMP_Text chipLabel = AddChildTMP(chipPrefabObj, "Label");
            Image chipIcon = AddChildImage(chipPrefabObj, "Icon");
            CategoryChipView chipPrefab = chipPrefabObj.AddComponent<CategoryChipView>();
            ReflectionHelpers.SetPrivateField(chipPrefab, "m_Background", chipBg);
            ReflectionHelpers.SetPrivateField(chipPrefab, "m_LabelText", chipLabel);
            ReflectionHelpers.SetPrivateField(chipPrefab, "m_IconImage", chipIcon);
            m_OwnedObjects.Add(chipPrefabObj);

            m_Card = m_Object.AddComponent<InfluencerCardView>();
            m_Config = ScriptableObject.CreateInstance<RecommendationConfig>();
            m_CategoryConfig = ScriptableObject.CreateInstance<CategoryConfig>();
            m_OwnedObjects.Add(m_Config);
            m_OwnedObjects.Add(m_CategoryConfig);

            ReflectionHelpers.SetPrivateField(m_Card, "m_BackgroundImage", m_BackgroundImage);
            ReflectionHelpers.SetPrivateField(m_Card, "m_AvatarImage", m_AvatarImage);
            ReflectionHelpers.SetPrivateField(m_Card, "m_NameText", m_NameText);
            ReflectionHelpers.SetPrivateField(m_Card, "m_EmailText", m_EmailText);
            ReflectionHelpers.SetPrivateField(m_Card, "m_FollowersText", m_FollowersText);
            ReflectionHelpers.SetPrivateField(m_Card, "m_CompatibilityText", m_CompatibilityText);
            ReflectionHelpers.SetPrivateField(m_Card, "m_ChipsContainer", m_ChipsContainer);
            ReflectionHelpers.SetPrivateField(m_Card, "m_CategoryChipPrefab", chipPrefab);
            ReflectionHelpers.SetPrivateField(m_Card, "m_ClickButton", clickButton);
            ReflectionHelpers.SetPrivateField(m_Card, "m_CanvasGroup", canvasGroup);
            ReflectionHelpers.SetPrivateField(m_Card, "m_Config", m_Config);

            m_Object.SetActive(true);
        }

        private GameObject NewChild(string childName)
        {
            GameObject child = new GameObject(childName);
            child.transform.SetParent(m_Object.transform);
            return child;
        }

        private T NewChildWithComponent<T>(string childName) where T : Component
        {
            return NewChild(childName).AddComponent<T>();
        }

        private static Image AddChildImage(GameObject parent, string name)
        {
            GameObject child = new GameObject(name);
            child.transform.SetParent(parent.transform);
            return child.AddComponent<Image>();
        }

        private static TMP_Text AddChildTMP(GameObject parent, string name)
        {
            GameObject child = new GameObject(name);
            child.transform.SetParent(parent.transform);
            return child.AddComponent<TextMeshProUGUI>();
        }

        // Adds CategoryDefinitions to a CategoryConfig instance via reflection on its private list.
        private readonly struct FieldInfoBackedCategoryList
        {
            private readonly List<CategoryDefinition> m_List;

            public FieldInfoBackedCategoryList(CategoryConfig config)
            {
                System.Reflection.FieldInfo field = typeof(CategoryConfig).GetField("m_Categories", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                Assert.IsNotNull(field, "m_Categories field not found on CategoryConfig.");
                m_List = (List<CategoryDefinition>)field.GetValue(config);
            }

            public void Add(CategoryDefinition definition)
            {
                m_List.Add(definition);
            }
        }
    }
}
