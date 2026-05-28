using System;
using Final.InfluencerMatch.Transition;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Final.InfluencerMatch.Bootstrap
{
    /// <summary>
    /// Registers the persistent screen-fade overlay (<see cref="ScreenFaderView"/>) so the AppNavigationController can drive fade-out/fade-in across scene loads.
    /// </summary>
    [Serializable]
    public class TransitionInstaller : IInstaller
    {
        [SerializeField] private ScreenFaderView m_ScreenFader;

        void IInstaller.Install(IContainerBuilder builder)
        {
            builder.RegisterComponent(m_ScreenFader).AsImplementedInterfaces();
        }
    }
}
