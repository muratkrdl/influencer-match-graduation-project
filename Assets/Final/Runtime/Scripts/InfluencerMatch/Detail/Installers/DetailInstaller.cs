using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Final.InfluencerMatch.Detail
{
    [Serializable]
    public class DetailInstaller : IInstaller
    {
        [Header("Views")]
        [SerializeField] private InfluencerDetailView m_InfluencerDetailView;

        void IInstaller.Install(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<InfluencerDetailController>().AsSelf();
            builder.Register<InfluencerDetailPresenter>(Lifetime.Singleton);
            builder.RegisterInstance(m_InfluencerDetailView).AsImplementedInterfaces().AsSelf();
        }
    }
}
