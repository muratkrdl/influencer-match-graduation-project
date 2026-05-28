using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Final.InfluencerMatch.Recommendation
{
    [Serializable]
    public class RecommendationInstaller : IInstaller
    {
        [Header("Views")]
        [SerializeField] private RecommendationListView m_RecommendationListView;

        void IInstaller.Install(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<RecommendationListController>().AsSelf();
            builder.Register<RecommendationListPresenter>(Lifetime.Singleton);
            builder.RegisterInstance(m_RecommendationListView).AsImplementedInterfaces().AsSelf();
        }
    }
}
