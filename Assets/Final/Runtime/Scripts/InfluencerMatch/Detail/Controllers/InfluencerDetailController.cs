using System;
using Final.InfluencerMatch.Budget;
using Final.InfluencerMatch.Common;
using Final.InfluencerMatch.Recommendation;
using Final.InfluencerMatch.UI;
using Final.Systems.EventBus.Pipes;
using VContainer;
using VContainer.Unity;

namespace Final.InfluencerMatch.Detail
{
    /// <summary>
    /// Drives the influencer detail panel: dispatches CardSelected signals to the presenter, then either renders the resolved view model or surfaces an error.
    /// </summary>
    public class InfluencerDetailController : IInitializable, IDisposable
    {
        [Inject] private InfluencerDetailView m_View;
        [Inject] private AppState m_AppState;
        [Inject] private CategoryConfig m_CategoryConfig;
        [Inject] private InfluencerDetailPresenter m_Presenter;
        [Inject] private UIManager m_UIManager;
        [Inject] private MainPipe m_Pipe;

        void IInitializable.Initialize()
        {
            m_View.BackClicked += HandleBackClicked;
            m_Pipe.SubscribeTo<CardSelectedMessage>(OnCardSelected);
        }

        void IDisposable.Dispose()
        {
            m_View.BackClicked -= HandleBackClicked;
            m_Pipe.UnsubscribeFrom<CardSelectedMessage>(OnCardSelected);
        }

        private void OnCardSelected(ref CardSelectedMessage msg)
        {
            InfluencerDetailViewModel vm = m_Presenter.Build(msg.InfluencerId, m_AppState.SelectedCategories, m_AppState.Budget);

            m_UIManager.Show<InfluencerDetailView>();
            m_View.DisplayInfluencer(vm.Influencer, vm.CompatibilityPercent, vm.EngagementCount, vm.FinalPrice, vm.ActivePlatforms, m_AppState.SelectedCategories, m_CategoryConfig);
        }

        private void HandleBackClicked()
        {
            NavigateBack();
        }

        private void NavigateBack()
        {
            m_Pipe.Raise(new BackRequestedMessage());
        }
    }
}
