using System;
using Final.InfluencerMatch.Budget;
using Final.InfluencerMatch.Common;
using Final.InfluencerMatch.Detail;
using Final.InfluencerMatch.EmptyState;
using Final.InfluencerMatch.UI;
using Final.Systems.EventBus.Pipes;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Final.InfluencerMatch.Recommendation
{
    /// <summary>
    /// Drives the recommendation list panel: orchestrates the refresh on budget commit, pushes the <see cref="RecommendationListPresenter"/> output into the view, and surfaces card / back interactions as signals on the pipe.
    /// </summary>
    public class RecommendationListController : IInitializable, IDisposable
    {
        [Inject] private RecommendationListView m_View;
        [Inject] private RecommendationListPresenter m_Presenter;
        [Inject] private AppState m_AppState;
        [Inject] private InfluencerDatabase m_Database;
        [Inject] private CategoryConfig m_CategoryConfig;
        [Inject] private UIManager m_UIManager;
        [Inject] private MainPipe m_Pipe;

        void IInitializable.Initialize()
        {
            m_View.CardClicked += HandleCardClicked;
            m_View.BackClicked += HandleBackClicked;
            m_Pipe.SubscribeTo<BudgetCommittedMessage>(OnBudgetCommitted);
        }

        void IDisposable.Dispose()
        {
            m_View.CardClicked -= HandleCardClicked;
            m_View.BackClicked -= HandleBackClicked;
            m_Pipe.UnsubscribeFrom<BudgetCommittedMessage>(OnBudgetCommitted);
        }

        private void OnBudgetCommitted(ref BudgetCommittedMessage msg)
        {
            RecommendationListViewModel viewModel = m_Presenter.Build(
                m_AppState.SelectedCategories,
                m_AppState.Budget,
                m_Database.Influencers);

            if (viewModel.ResultCount == 0)
            {
                m_Pipe.Raise(new EmptyStateRequestedMessage());
                return;
            }

            m_UIManager.Show<RecommendationListView>();
            m_View.SetSubtitle(viewModel.Subtitle);
            m_View.DisplayResults(viewModel.Ranked, m_CategoryConfig);
        }

        private void HandleCardClicked(SerializableGuid influencerId)
        {
            m_Pipe.Raise(new CardSelectedMessage(influencerId));
        }

        private void HandleBackClicked()
        {
            m_Pipe.Raise(new BackRequestedMessage());
        }
    }
}
