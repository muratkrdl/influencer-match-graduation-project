using System;

namespace Final.InfluencerMatch.Transition
{
    /// <summary>
    /// Full-screen transition overlay that covers and reveals the screen during transitions.
    /// </summary>
    public interface IScreenFader
    {
        void FadeOut(Action onComplete = null);
        void FadeIn(Action onComplete = null);
    }
}
