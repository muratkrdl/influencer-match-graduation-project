using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Final.InfluencerMatch.Splash
{
    [Serializable]
    public class SplashInstaller : IInstaller
    {
        [Header("Views")]
        [SerializeField] private SplashView m_SplashView;

        void IInstaller.Install(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<SplashController>().AsSelf();
            builder.RegisterInstance(m_SplashView).AsImplementedInterfaces().AsSelf();
        }
    }
}
