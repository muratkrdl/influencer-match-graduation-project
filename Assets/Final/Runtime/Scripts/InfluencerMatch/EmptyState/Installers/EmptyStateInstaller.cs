using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Final.InfluencerMatch.EmptyState
{
    [Serializable]
    public class EmptyStateInstaller : IInstaller
    {
        [Header("Views")]
        [SerializeField] private EmptyStateView m_EmptyStateView;

        void IInstaller.Install(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<EmptyStateController>().AsSelf();
            builder.RegisterInstance(m_EmptyStateView).AsImplementedInterfaces().AsSelf();
        }
    }
}
