using System.Collections.Generic;
using Final.Systems.DI;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Final.InfluencerMatch.MainMenu
{
    public class MainMenuSceneInstaller : MonoInstaller
    {
        [Header("Feature Installers")]
        [SerializeField] private MainMenuInstaller m_MainMenuInstaller;

        protected override void InstallBindings(IContainerBuilder builder)
        {
            foreach (IInstaller installer in FetchInstallers())
            {
                installer.Install(builder);
            }
        }

        private IEnumerable<IInstaller> FetchInstallers()
        {
            yield return m_MainMenuInstaller;
        }
    }
}
