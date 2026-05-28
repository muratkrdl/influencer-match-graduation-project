using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Final.InfluencerMatch.Common;
using Final.InfluencerMatch.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;

namespace Final.InfluencerMatch.Recommendation
{
    /// <summary>
    /// Recommendation list panel that renders a scrollable list of influencer cards from a ranked result set.
    /// </summary>
    public class RecommendationListView : UIPanelBase
    {
        private const int k_CardPoolDefaultCapacity = 8;
        private const int k_CardPoolMaxSize = 128;

        [Header("Navigation")]
        [SerializeField] private Button m_BackButton;

        [Header("Content")]
        [SerializeField] private Transform m_ResultsContainer;
        [SerializeField] private InfluencerCardView m_InfluencerCardPrefab;

        [Header("Texts")]
        [SerializeField] private TMP_Text m_TitleText;
        [SerializeField] private TMP_Text m_SubtitleText;
        [SerializeField] private GameObject m_EmptyStateGameObject;

        private readonly List<InfluencerCardView> m_SpawnedCards = new List<InfluencerCardView>();
        private CancellationTokenSource m_SpawnCts;
        private ObjectPool<InfluencerCardView> m_CardPool;

        [Inject] private RecommendationConfig m_Config;
        [Inject] private IObjectResolver m_Container;

        public event CardClickedHandler CardClicked;
        public event BackClickedHandler BackClicked;

        private void Awake()
        {
            m_CardPool = new ObjectPool<InfluencerCardView>(
                createFunc: CreatePooledCard,
                actionOnGet: card => card.gameObject.SetActive(true),
                actionOnRelease: card => card.gameObject.SetActive(false),
                actionOnDestroy: DestroyPooledCard,
                collectionCheck: false,
                defaultCapacity: k_CardPoolDefaultCapacity,
                maxSize: k_CardPoolMaxSize);

            m_BackButton.onClick.AddListener(HandleBackButtonClicked);
            m_EmptyStateGameObject.SetActive(false);
            m_TitleText.text = "Recommendations";
        }

        private void OnDisable()
        {
            CancelInFlightSpawn();
        }

        private void OnDestroy()
        {
            m_BackButton.onClick.RemoveListener(HandleBackButtonClicked);

            CancelInFlightSpawn();
            ClearSpawnedCards();
            m_CardPool?.Dispose();
        }

        public void DisplayResults(IReadOnlyList<ScoredInfluencer> ranked, CategoryConfig categoryConfig)
        {
            CancelInFlightSpawn();
            ClearSpawnedCards();

            int count = ranked?.Count ?? 0;

            m_EmptyStateGameObject.SetActive(count == 0);

            if (count == 0)
            {
                return;
            }

            m_SpawnCts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
            SpawnCardsStaggeredAsync(ranked, categoryConfig, m_SpawnCts.Token).Forget();
        }

        public void SetSubtitle(string subtitle)
        {
            m_SubtitleText.text = subtitle;
        }

        private async UniTaskVoid SpawnCardsStaggeredAsync(
            IReadOnlyList<ScoredInfluencer> ranked,
            CategoryConfig categoryConfig,
            CancellationToken token)
        {
            foreach (ScoredInfluencer scored in ranked)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }

                InfluencerCardView card = m_CardPool.Get();
                card.transform.SetAsLastSibling();
                card.Bind(scored, categoryConfig);
                m_SpawnedCards.Add(card);

                card.PlayEntranceFade();

                await UniTask.Delay(TimeSpan.FromSeconds(m_Config.StaggerDelay), cancellationToken: token);
            }
        }

        private void CancelInFlightSpawn()
        {
            m_SpawnCts?.Cancel();
            m_SpawnCts?.Dispose();
            m_SpawnCts = null;
        }

        private void ClearSpawnedCards()
        {
            foreach (InfluencerCardView card in m_SpawnedCards)
            {
                m_CardPool.Release(card);
            }
            m_SpawnedCards.Clear();
        }

        private InfluencerCardView CreatePooledCard()
        {
            InfluencerCardView card = m_Container.Instantiate(m_InfluencerCardPrefab, m_ResultsContainer);
            card.CardClicked += HandleCardClicked;
            return card;
        }

        private void DestroyPooledCard(InfluencerCardView card)
        {
            if (card == null)
            {
                return;
            }
            card.CardClicked -= HandleCardClicked;
            Destroy(card.gameObject);
        }

        private void HandleBackButtonClicked()
        {
            BackClicked?.Invoke();
        }

        private void HandleCardClicked(SerializableGuid influencerId)
        {
            CardClicked?.Invoke(influencerId);
        }

        public delegate void CardClickedHandler(SerializableGuid influencerId);
        public delegate void BackClickedHandler();
    }
}
