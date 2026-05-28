using System;
using DG.Tweening;
using Final.Systems.EventBus.Pipes;
using VContainer;
using VContainer.Unity;

namespace Final.InfluencerMatch.Splash
{
    /// <summary>
    /// Drives the splash lifecycle, raising a completion message after a fixed dwell.
    /// </summary>
    public class SplashController : IInitializable, IDisposable
    {
        [Inject] private SplashConfig m_Config;
        [Inject] private ProjectPipe m_Pipe;

        private Tween m_DwellTween;

        void IInitializable.Initialize()
        {
            m_DwellTween = DOVirtual.DelayedCall(m_Config.DurationSeconds, OnDwellCompleted, ignoreTimeScale: true);
        }

        void IDisposable.Dispose()
        {
            m_DwellTween?.Kill();
            m_DwellTween = null;
        }

        private void OnDwellCompleted()
        {
            m_DwellTween = null;
            m_Pipe.Raise(new SplashCompletedMessage());
        }
    }
}
