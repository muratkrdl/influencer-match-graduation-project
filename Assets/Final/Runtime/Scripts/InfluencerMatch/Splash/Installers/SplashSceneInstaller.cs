using System.Collections.Generic;
using Final.Systems.DI;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Final.InfluencerMatch.Splash
{
    /// <summary>
    /// Scene installer for the Splash scope that composes the splash feature installer.
    /// </summary>
    public class SplashSceneInstaller : MonoInstaller
    {
        [Header("Feature Installers")]
        [SerializeField] private SplashInstaller m_SplashInstaller;

        protected override void InstallBindings(IContainerBuilder builder)
        {
            foreach (IInstaller installer in FetchInstallers())
            {
                installer.Install(builder);
            }
        }

        private IEnumerable<IInstaller> FetchInstallers()
        {
            yield return m_SplashInstaller;
        }
    }
}
