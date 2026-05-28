using System;
using Cysharp.Threading.Tasks;
using Final.InfluencerMatch.Common;
using Final.InfluencerMatch.MainMenu;
using Final.InfluencerMatch.Splash;
using Final.InfluencerMatch.Transition;
using Final.Systems.DI;
using Final.Systems.EventBus.Pipes;
using VContainer;
using VContainer.Unity;

namespace Final.InfluencerMatch.Bootstrap
{
    /// <summary>
    /// App-level navigation controller; translates scene-completion signals into scene loads.
    /// </summary>
    public class AppNavigationController : IInitializable, IDisposable
    {
        [Inject] private ProjectPipe m_Pipe;
        [Inject] private IScreenFader m_ScreenFader;

        void IInitializable.Initialize()
        {
            m_Pipe.SubscribeTo<SplashCompletedMessage>(OnSplashCompleted);
            m_Pipe.SubscribeTo<MatchInfluencerRequestedMessage>(OnMatchInfluencerRequested);
        }

        void IDisposable.Dispose()
        {
            m_Pipe.UnsubscribeFrom<SplashCompletedMessage>(OnSplashCompleted);
            m_Pipe.UnsubscribeFrom<MatchInfluencerRequestedMessage>(OnMatchInfluencerRequested);
        }

        private void OnSplashCompleted(ref SplashCompletedMessage msg)
        {
            TransitionTo(SceneNames.MainMenu);
        }

        private void OnMatchInfluencerRequested(ref MatchInfluencerRequestedMessage msg)
        {
            TransitionTo(SceneNames.Main);
        }

        private void TransitionTo(string sceneName)
        {
            m_ScreenFader.FadeOut(() => LoadThenRevealAsync(sceneName).Forget());
        }

        private async UniTaskVoid LoadThenRevealAsync(string sceneName)
        {
            await SceneLoader.LoadAsync(sceneName);
            m_ScreenFader.FadeIn();
        }
    }
}
