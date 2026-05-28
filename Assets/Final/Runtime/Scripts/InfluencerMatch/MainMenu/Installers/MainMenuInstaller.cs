using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Final.InfluencerMatch.MainMenu
{
    [Serializable]
    public class MainMenuInstaller : IInstaller
    {
        [Header("Views")]
        [SerializeField] private MainMenuView m_MainMenuView;

        void IInstaller.Install(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<MainMenuController>().AsSelf();
            builder.RegisterInstance(m_MainMenuView).AsSelf();
        }
    }
}
