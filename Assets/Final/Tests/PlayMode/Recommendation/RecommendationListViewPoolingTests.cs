using System.Collections;
using System.Collections.Generic;
using Final.InfluencerMatch.Budget;
using Final.InfluencerMatch.Common;
using Final.InfluencerMatch.Recommendation;
using Final.InfluencerMatch.UI;
using Final.Tests.PlayMode.Helpers;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;

namespace Final.Tests.PlayMode.Recommendation
{
    /// <summary>
    /// PlayMode regression guard for <see cref="RecommendationListView"/>'s card pool.
    /// Calls DisplayResults twice with the same ranked set and asserts the second pass
    /// reuses the InstanceIDs from the first — if anyone reverts the pool to
    /// Destroy/Instantiate, this fails.
    /// </summary>
    public sealed class RecommendationListViewPoolingTests
    {
        private const float k_PostSpawnBufferSeconds = 0.5f;

        private readonly List<Object> m_OwnedObjects = new List<Object>();
        private GameObject m_ViewObject;
        private GameObject m_CardPrefab;
        private Transform m_ResultsContainer;
        private IObjectResolver m_Container;
        private RecommendationConfig m_RecommendationConfig;
        private UISharedConfig m_SharedConfig;
        private CategoryConfig m_CategoryConfig;

        [TearDown]
        public void TearDown()
        {
            if (m_ViewObject != null) Object.Destroy(m_ViewObject);
            if (m_CardPrefab != null) Object.Destroy(m_CardPrefab);
            m_ViewObject = null;
            m_CardPrefab = null;
            m_ResultsContainer = null;

            for (int i = 0; i < m_OwnedObjects.Count; i++)
            {
                if (m_OwnedObjects[i] != null)
                {
                    Object.DestroyImmediate(m_OwnedObjects[i]);
                }
            }
            m_OwnedObjects.Clear();

            m_Container = null;
        }

        [UnityTest]
        public IEnumerator DisplayResults_TwoCalls_PoolReusesCardInstances()
        {
            BuildSceneGraph();
            RecommendationListView view = m_ViewObject.GetComponent<RecommendationListView>();
            List<ScoredInfluencer> ranked = BuildRanked(count: 3);

            view.DisplayResults(ranked, m_CategoryConfig);
            yield return new WaitForSecondsRealtime(StaggerTotalSeconds(ranked.Count));
            HashSet<int> firstIds = CaptureCardInstanceIds();
            Assume.That(firstIds.Count, Is.EqualTo(ranked.Count), "First pass should spawn one card per ranked entry.");

            view.DisplayResults(ranked, m_CategoryConfig);
            yield return new WaitForSecondsRealtime(StaggerTotalSeconds(ranked.Count));
            HashSet<int> secondIds = CaptureCardInstanceIds();

            Assert.AreEqual(ranked.Count, secondIds.Count, "Second pass should also spawn one card per ranked entry.");
            secondIds.IntersectWith(firstIds);
            Assert.AreEqual(ranked.Count, secondIds.Count, "Pool must reuse the same InstanceIDs across DisplayResults calls.");
        }

        private float StaggerTotalSeconds(int count)
        {
            return (m_RecommendationConfig.StaggerDelay * count) + m_RecommendationConfig.CardFadeInDuration + k_PostSpawnBufferSeconds;
        }

        private HashSet<int> CaptureCardInstanceIds()
        {
            HashSet<int> ids = new HashSet<int>();
            for (int i = 0; i < m_ResultsContainer.childCount; i++)
            {
                Transform child = m_ResultsContainer.GetChild(i);
                if (child.gameObject.activeSelf)
                {
                    ids.Add(child.gameObject.GetInstanceID());
                }
            }
            return ids;
        }

        private List<ScoredInfluencer> BuildRanked(int count)
        {
            List<ScoredInfluencer> list = new List<ScoredInfluencer>(count);
            for (int i = 0; i < count; i++)
            {
                InfluencerData data = ScriptableObject.CreateInstance<InfluencerData>();
                data.name = "TestInfluencer_" + i;
                m_OwnedObjects.Add(data);
                list.Add(new ScoredInfluencer(data, 0.5f, false));
            }
            return list;
        }

        private void BuildSceneGraph()
        {
            m_CardPrefab = BuildCardPrefab();
            m_CardPrefab.SetActive(false);

            m_RecommendationConfig = ScriptableObject.CreateInstance<RecommendationConfig>();
            m_SharedConfig = ScriptableObject.CreateInstance<UISharedConfig>();
            m_CategoryConfig = ScriptableObject.CreateInstance<CategoryConfig>();
            m_OwnedObjects.Add(m_RecommendationConfig);
            m_OwnedObjects.Add(m_SharedConfig);
            m_OwnedObjects.Add(m_CategoryConfig);

            ContainerBuilder builder = new ContainerBuilder();
            builder.RegisterInstance(m_RecommendationConfig).AsSelf();
            builder.RegisterInstance(m_SharedConfig).AsSelf();
            m_Container = builder.Build();

            m_ViewObject = new GameObject("RecommendationListView");
            m_ViewObject.SetActive(false);

            CanvasGroup canvasGroup = m_ViewObject.AddComponent<CanvasGroup>();
            Button backButton = NewChildWithComponent<Button>(m_ViewObject, "BackButton");
            GameObject resultsContainerObj = NewChild(m_ViewObject, "ResultsContainer");
            m_ResultsContainer = resultsContainerObj.transform;
            TMP_Text titleText = NewChildWithComponent<TextMeshProUGUI>(m_ViewObject, "Title");
            TMP_Text subtitleText = NewChildWithComponent<TextMeshProUGUI>(m_ViewObject, "Subtitle");
            GameObject emptyStateObj = NewChild(m_ViewObject, "EmptyState");

            RecommendationListView view = m_ViewObject.AddComponent<RecommendationListView>();
            InfluencerCardView cardPrefabComponent = m_CardPrefab.GetComponent<InfluencerCardView>();

            ReflectionHelpers.SetPrivateField(view, "m_BackButton", backButton);
            ReflectionHelpers.SetPrivateField(view, "m_ResultsContainer", m_ResultsContainer);
            ReflectionHelpers.SetPrivateField(view, "m_InfluencerCardPrefab", cardPrefabComponent);
            ReflectionHelpers.SetPrivateField(view, "m_TitleText", titleText);
            ReflectionHelpers.SetPrivateField(view, "m_SubtitleText", subtitleText);
            ReflectionHelpers.SetPrivateField(view, "m_EmptyStateGameObject", emptyStateObj);
            ReflectionHelpers.SetPrivateField(view, "m_CanvasGroup", canvasGroup);
            ReflectionHelpers.SetPrivateField(view, "m_SharedConfig", m_SharedConfig);
            ReflectionHelpers.SetPrivateField(view, "m_Config", m_RecommendationConfig);
            ReflectionHelpers.SetPrivateField(view, "m_Container", m_Container);

            m_ViewObject.SetActive(true);
        }

        private GameObject BuildCardPrefab()
        {
            GameObject root = new GameObject("CardPrefab");
            root.SetActive(false);

            CanvasGroup cg = root.AddComponent<CanvasGroup>();
            Image background = NewChildWithComponent<Image>(root, "Background");
            Image avatar = NewChildWithComponent<Image>(root, "Avatar");
            TMP_Text nameText = NewChildWithComponent<TextMeshProUGUI>(root, "Name");
            TMP_Text emailText = NewChildWithComponent<TextMeshProUGUI>(root, "Email");
            TMP_Text followersText = NewChildWithComponent<TextMeshProUGUI>(root, "Followers");
            TMP_Text compatibilityText = NewChildWithComponent<TextMeshProUGUI>(root, "Compatibility");
            GameObject chipsContainer = NewChild(root, "ChipsContainer");
            Button clickButton = NewChildWithComponent<Button>(root, "ClickButton");
            GameObject chipPrefabObj = new GameObject("ChipPrefab");
            chipPrefabObj.SetActive(false);
            // CategoryChipView is unused in this test because ranked influencers have empty
            // CategoryScores → RebuildChips's loop never runs → chip pool's createFunc is never invoked.
            CategoryChipView chipPrefab = chipPrefabObj.AddComponent<CategoryChipView>();
            m_OwnedObjects.Add(chipPrefabObj);

            InfluencerCardView card = root.AddComponent<InfluencerCardView>();

            ReflectionHelpers.SetPrivateField(card, "m_BackgroundImage", background);
            ReflectionHelpers.SetPrivateField(card, "m_AvatarImage", avatar);
            ReflectionHelpers.SetPrivateField(card, "m_NameText", nameText);
            ReflectionHelpers.SetPrivateField(card, "m_EmailText", emailText);
            ReflectionHelpers.SetPrivateField(card, "m_FollowersText", followersText);
            ReflectionHelpers.SetPrivateField(card, "m_CompatibilityText", compatibilityText);
            ReflectionHelpers.SetPrivateField(card, "m_ChipsContainer", chipsContainer.transform);
            ReflectionHelpers.SetPrivateField(card, "m_CategoryChipPrefab", chipPrefab);
            ReflectionHelpers.SetPrivateField(card, "m_ClickButton", clickButton);
            ReflectionHelpers.SetPrivateField(card, "m_CanvasGroup", cg);

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
