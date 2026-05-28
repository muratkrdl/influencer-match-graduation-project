using System;
using Final.InfluencerMatch.Detail;
using Final.Systems.EventBus.Pipes;
using VContainer;
using VContainer.Unity;

namespace Final.InfluencerMatch.EmptyState
{
    /// <summary>
    /// Drives the empty state panel, raising a back request when the user changes filters.
    /// </summary>
    public class EmptyStateController : IInitializable, IDisposable
    {
        [Inject] private EmptyStateView m_View;
        [Inject] private MainPipe m_Pipe;

        void IInitializable.Initialize()
        {
            m_View.ChangeFiltersRequested += HandleChangeFiltersRequested;
        }

        void IDisposable.Dispose()
        {
            m_View.ChangeFiltersRequested -= HandleChangeFiltersRequested;
        }

        private void HandleChangeFiltersRequested()
        {
            m_Pipe.Raise(new BackRequestedMessage());
        }
    }
}
