using System;
using Final.InfluencerMatch.Budget;
using Final.InfluencerMatch.Detail;
using Final.InfluencerMatch.EmptyState;
using Final.Systems.EventBus.Pipes;
using VContainer;
using VContainer.Unity;

namespace Final.InfluencerMatch.UI
{
    /// <summary>
    /// Drives intra-Main panel transitions in response to typed signals on the project pipe.
    /// </summary>
    public class PanelNavigationController : IInitializable, IStartable, IDisposable
    {
        [Inject] private UIManager m_UIManager;
        [Inject] private MainPipe m_Pipe;

        void IInitializable.Initialize()
        {
            m_Pipe.SubscribeTo<BackRequestedMessage>(OnBackRequested);
            m_Pipe.SubscribeTo<EmptyStateRequestedMessage>(OnEmptyStateRequested);
        }

        void IStartable.Start()
        {
            m_UIManager.Show<BudgetCategoryInputView>();
        }

        void IDisposable.Dispose()
        {
            m_Pipe.UnsubscribeFrom<BackRequestedMessage>(OnBackRequested);
            m_Pipe.UnsubscribeFrom<EmptyStateRequestedMessage>(OnEmptyStateRequested);
        }

        private void OnBackRequested(ref BackRequestedMessage msg)
        {
            m_UIManager.GoBack();
        }

        private void OnEmptyStateRequested(ref EmptyStateRequestedMessage msg)
        {
            m_UIManager.Show<EmptyStateView>();
        }
    }
}
