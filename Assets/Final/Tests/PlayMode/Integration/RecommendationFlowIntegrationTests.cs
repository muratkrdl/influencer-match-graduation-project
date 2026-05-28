using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Final.InfluencerMatch.Budget;
using Final.InfluencerMatch.Common;
using Final.InfluencerMatch.Detail;
using Final.InfluencerMatch.EmptyState;
using Final.InfluencerMatch.Inputs;
using Final.InfluencerMatch.Recommendation;
using Final.InfluencerMatch.UI;
using Final.Systems.EventBus.Pipes;
using Final.Tests.PlayMode.Helpers;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;

namespace Final.Tests.PlayMode.Integration
{
    /// <summary>
    /// End-to-end signal flow: real MatchingService + PricingService + Presenter +
    /// Controller + PanelNavigationController + UIManager + View + Pipe wired programmatically.
    /// Asserts BudgetCommittedMessage drives matched-vs-empty branching and that
    /// BackRequestedMessage drives panel pop.
    /// </summary>
    public sealed class RecommendationFlowIntegrationTests
    {
        private readonly List<UnityEngine.Object> m_Owned = new List<UnityEngine.Object>();
        private GameObject m_RecommendationViewObject;
        private GameObject m_EmptyStateViewObject;
        private GameObject m_CardPrefab;
        private MainPipe m_Pipe;
        private RecommendationListController m_Controller;
        private PanelNavigationController m_PanelNav;
        private UIManager m_UIManager;
        private Transform m_ResultsContainer;
        private RecommendationConfig m_RecommendationConfig;

        [TearDown]
        public void TearDown()
        {
            if (m_Controller != null)
            {
                ((IDisposable)m_Controller).Dispose();
                m_Controller = null;
            }
            if (m_PanelNav != null)
            {
                ((IDisposable)m_PanelNav).Dispose();
                m_PanelNav = null;
            }
            if (m_UIManager != null)
            {
                ((IDisposable)m_UIManager).Dispose();
                m_UIManager = null;
            }
            DestroyIfNotNull(ref m_RecommendationViewObject);
            DestroyIfNotNull(ref m_EmptyStateViewObject);
            DestroyIfNotNull(ref m_CardPrefab);
            for (int i = 0; i < m_Owned.Count; i++)
            {
                if (m_Owned[i] != null)
                {
                    UnityEngine.Object.DestroyImmediate(m_Owned[i]);
                }
            }
            m_Owned.Clear();
            m_Pipe = null;
            m_ResultsContainer = null;
        }

        [UnityTest]
        public IEnumerator BudgetCommitted_ThreeMatchingInfluencers_SpawnsThreeCards()
        {
            const int influencerCount = 3;
            BuildEnvironment(influencerCount, scoreForEducation: 5);

            m_Pipe.Raise(new BudgetCommittedMessage());

            float waitSeconds = (m_RecommendationConfig.StaggerDelay * influencerCount) + m_RecommendationConfig.CardFadeInDuration + 0.5f;
            yield return new WaitForSecondsRealtime(waitSeconds);

            Assert.AreEqual(influencerCount, CountActiveChildren(m_ResultsContainer),
                "Controller should have driven the view to spawn one card per matched influencer.");
        }

        [UnityTest]
        public IEnumerator BudgetCommitted_NoMatchingInfluencers_ShowsEmptyState()
        {
            // All influencers score 0 → filtered by MinimumCategoryScoreToInclude=1.
            // Controller raises EmptyStateRequestedMessage → PanelNav shows EmptyStateView.
            BuildEnvironment(influencerCount: 3, scoreForEducation: 0);

            m_Pipe.Raise(new BudgetCommittedMessage());
            yield return null;

            Assert.IsTrue(m_EmptyStateViewObject.activeSelf, "EmptyStateView should be active after a zero-match commit.");
        }

        [UnityTest]
        public IEnumerator BackRequested_AfterPanelStackPushed_PopsToPreviousPanel()
        {
            BuildEnvironment(influencerCount: 3, scoreForEducation: 5);

            // Manually populate history: show EmptyStateView first, then RecommendationListView
            // (which pushes EmptyStateView onto the back stack).
            m_UIManager.Show<EmptyStateView>();
            yield return null;
            m_UIManager.Show<RecommendationListView>();
            yield return null;
            Assume.That(m_RecommendationViewObject.activeSelf, Is.True);
            Assume.That(m_EmptyStateViewObject.activeSelf, Is.False);

            m_Pipe.Raise(new BackRequestedMessage());
            yield return null;

            Assert.IsTrue(m_EmptyStateViewObject.activeSelf, "GoBack should re-activate the previous panel.");
            Assert.IsFalse(m_RecommendationViewObject.activeSelf, "Popped panel should be hidden.");
        }

        private static int CountActiveChildren(Transform parent)
        {
            int count = 0;
            for (int i = 0; i < parent.childCount; i++)
            {
                if (parent.GetChild(i).gameObject.activeSelf)
                {
                    count++;
                }
            }
            return count;
        }

        private void BuildEnvironment(int influencerCount, int scoreForEducation)
        {
            MatchingConfig matchingConfig = ScriptableObject.CreateInstance<MatchingConfig>();
            CategoryConfig categoryConfig = ScriptableObject.CreateInstance<CategoryConfig>();
            UISharedConfig sharedConfig = ScriptableObject.CreateInstance<UISharedConfig>();
            m_RecommendationConfig = ScriptableObject.CreateInstance<RecommendationConfig>();
            m_Owned.Add(matchingConfig);
            m_Owned.Add(categoryConfig);
            m_Owned.Add(sharedConfig);
            m_Owned.Add(m_RecommendationConfig);

            AppState appState = new AppState();
            appState.SetSelectedCategories(new[] { CategoryId.Education });
            appState.Budget = 100_000m;

            InfluencerDatabase database = ScriptableObject.CreateInstance<InfluencerDatabase>();
            m_Owned.Add(database);
            List<InfluencerData> dbList = new List<InfluencerData>(influencerCount);
            for (int i = 0; i < influencerCount; i++)
            {
                InfluencerData inf = ScriptableObject.CreateInstance<InfluencerData>();
                inf.name = "TestInfluencer_" + i;
                m_Owned.Add(inf);
                ReflectionHelpers.AddCategoryScore(inf, CategoryId.Education, scoreForEducation);
                dbList.Add(inf);
            }
            FieldInfo dbInfluencersField = typeof(InfluencerDatabase).GetField("m_Influencers", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(dbInfluencersField, "m_Influencers field not found on InfluencerDatabase.");
            dbInfluencersField.SetValue(database, dbList);

            m_CardPrefab = BuildCardPrefab();
            InfluencerCardView cardPrefabComponent = m_CardPrefab.GetComponent<InfluencerCardView>();

            ContainerBuilder containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterInstance(m_RecommendationConfig).AsSelf();
            containerBuilder.RegisterInstance(sharedConfig).AsSelf();
            IObjectResolver container = containerBuilder.Build();

            m_RecommendationViewObject = BuildRecommendationViewObject(cardPrefabComponent, sharedConfig, m_RecommendationConfig, container, out m_ResultsContainer);
            RecommendationListView recommendationView = m_RecommendationViewObject.GetComponent<RecommendationListView>();
            m_EmptyStateViewObject = BuildEmptyStateViewObject(sharedConfig);
            EmptyStateView emptyStateView = m_EmptyStateViewObject.GetComponent<EmptyStateView>();

            m_Pipe = new MainPipe();

            m_UIManager = new UIManager();
            IUIPanel[] panels = { recommendationView, emptyStateView };
            ReflectionHelpers.SetPrivateField(m_UIManager, "m_RegisteredPanels", panels);
            ReflectionHelpers.SetPrivateField(m_UIManager, "m_InputService", new FakeInputService());
            ((IInitializable)m_UIManager).Initialize();

            // PanelNavigationController owns the EmptyStateRequested and BackRequested signal
            // handlers — wire it so the test can exercise both routes end-to-end.
            m_PanelNav = new PanelNavigationController();
            ReflectionHelpers.SetPrivateField(m_PanelNav, "m_UIManager", m_UIManager);
            ReflectionHelpers.SetPrivateField(m_PanelNav, "m_Pipe", m_Pipe);
            ((IInitializable)m_PanelNav).Initialize();

            RecommendationListPresenter presenter = new RecommendationListPresenter(new MatchingService(), new PricingService(), matchingConfig);
            m_Controller = new RecommendationListController();
            ReflectionHelpers.SetPrivateField(m_Controller, "m_View", recommendationView);
            ReflectionHelpers.SetPrivateField(m_Controller, "m_Presenter", presenter);
            ReflectionHelpers.SetPrivateField(m_Controller, "m_AppState", appState);
            ReflectionHelpers.SetPrivateField(m_Controller, "m_Database", database);
            ReflectionHelpers.SetPrivateField(m_Controller, "m_CategoryConfig", categoryConfig);
            ReflectionHelpers.SetPrivateField(m_Controller, "m_UIManager", m_UIManager);
            ReflectionHelpers.SetPrivateField(m_Controller, "m_Pipe", m_Pipe);

            ((IInitializable)m_Controller).Initialize();
        }

        private GameObject BuildRecommendationViewObject(InfluencerCardView cardPrefab, UISharedConfig sharedConfig, RecommendationConfig recommendationConfig, IObjectResolver container, out Transform resultsContainer)
        {
            GameObject obj = new GameObject("RecommendationListView");
            obj.SetActive(false);

            CanvasGroup canvasGroup = obj.AddComponent<CanvasGroup>();
            Button backButton = NewChildWithComponent<Button>(obj, "BackButton");
            GameObject resultsContainerObj = NewChild(obj, "ResultsContainer");
            resultsContainer = resultsContainerObj.transform;
            TMP_Text titleText = NewChildWithComponent<TextMeshProUGUI>(obj, "Title");
            TMP_Text subtitleText = NewChildWithComponent<TextMeshProUGUI>(obj, "Subtitle");
            GameObject emptyStateObj = NewChild(obj, "EmptyStatePlaceholder");

            RecommendationListView view = obj.AddComponent<RecommendationListView>();
            ReflectionHelpers.SetPrivateField(view, "m_BackButton", backButton);
            ReflectionHelpers.SetPrivateField(view, "m_ResultsContainer", resultsContainerObj.transform);
            ReflectionHelpers.SetPrivateField(view, "m_InfluencerCardPrefab", cardPrefab);
            ReflectionHelpers.SetPrivateField(view, "m_TitleText", titleText);
            ReflectionHelpers.SetPrivateField(view, "m_SubtitleText", subtitleText);
            ReflectionHelpers.SetPrivateField(view, "m_EmptyStateGameObject", emptyStateObj);
            ReflectionHelpers.SetPrivateField(view, "m_CanvasGroup", canvasGroup);
            ReflectionHelpers.SetPrivateField(view, "m_SharedConfig", sharedConfig);
            ReflectionHelpers.SetPrivateField(view, "m_Config", recommendationConfig);
            ReflectionHelpers.SetPrivateField(view, "m_Container", container);

            obj.SetActive(true);
            return obj;
        }

        private GameObject BuildEmptyStateViewObject(UISharedConfig sharedConfig)
        {
            GameObject obj = new GameObject("EmptyStateView");
            obj.SetActive(false);

            CanvasGroup canvasGroup = obj.AddComponent<CanvasGroup>();
            TMP_Text titleText = NewChildWithComponent<TextMeshProUGUI>(obj, "Title");
            TMP_Text descriptionText = NewChildWithComponent<TextMeshProUGUI>(obj, "Description");
            Button changeFiltersButton = NewChildWithComponent<Button>(obj, "ChangeFilters");

            EmptyStateView view = obj.AddComponent<EmptyStateView>();
            ReflectionHelpers.SetPrivateField(view, "m_TitleText", titleText);
            ReflectionHelpers.SetPrivateField(view, "m_DescriptionText", descriptionText);
            ReflectionHelpers.SetPrivateField(view, "m_ChangeFiltersButton", changeFiltersButton);
            ReflectionHelpers.SetPrivateField(view, "m_CanvasGroup", canvasGroup);
            ReflectionHelpers.SetPrivateField(view, "m_SharedConfig", sharedConfig);

            obj.SetActive(true);
            return obj;
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
            CategoryChipView chipPrefab = chipPrefabObj.AddComponent<CategoryChipView>();
            m_Owned.Add(chipPrefabObj);

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

        private static void DestroyIfNotNull(ref GameObject obj)
        {
            if (obj != null)
            {
                UnityEngine.Object.Destroy(obj);
                obj = null;
            }
        }

        private sealed class FakeInputService : IInputService
        {
#pragma warning disable CS0067
            public event CancelRequestedHandler CancelRequested;
#pragma warning restore CS0067
        }
    }
}
