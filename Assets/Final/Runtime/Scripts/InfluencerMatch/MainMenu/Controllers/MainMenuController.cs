using System;
using Final.Systems.EventBus.Pipes;
using VContainer;
using VContainer.Unity;

namespace Final.InfluencerMatch.MainMenu
{
    /// <summary>
    /// Forwards the main menu's "Match Influencer" button intent onto the project pipe.
    /// </summary>
    public class MainMenuController : IInitializable, IDisposable
    {
        [Inject] private MainMenuView m_View;
        [Inject] private ProjectPipe m_Pipe;

        void IInitializable.Initialize()
        {
            m_View.MatchInfluencerClicked += HandleMatchInfluencerClicked;
        }

        void IDisposable.Dispose()
        {
            m_View.MatchInfluencerClicked -= HandleMatchInfluencerClicked;
        }

        private void HandleMatchInfluencerClicked()
        {
            m_Pipe.Raise(new MatchInfluencerRequestedMessage());
        }
    }
}
