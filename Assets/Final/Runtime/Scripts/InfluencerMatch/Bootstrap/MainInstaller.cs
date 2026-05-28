using System.Collections.Generic;
using Final.InfluencerMatch.Budget;
using Final.InfluencerMatch.Detail;
using Final.InfluencerMatch.EmptyState;
using Final.InfluencerMatch.Recommendation;
using Final.InfluencerMatch.UI;
using Final.Systems.DI;
using Final.Systems.EventBus.Installers;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Final.InfluencerMatch.Bootstrap
{
    /// <summary>
    /// Main scene installer; registers the UIManager, feature installers, and panel navigation.
    /// </summary>
    public class MainInstaller : MonoInstaller
    {
        [Header("Feature Installers")]
        [SerializeField] private BudgetInstaller m_BudgetInstaller;
        [SerializeField] private RecommendationInstaller m_RecommendationInstaller;
        [SerializeField] private DetailInstaller m_DetailInstaller;
        [SerializeField] private EmptyStateInstaller m_EmptyStateInstaller;

        protected override void InstallBindings(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<UIManager>().AsSelf();

            foreach (IInstaller installer in FetchInstallers())
            {
                installer.Install(builder);
            }

            builder.RegisterEntryPoint<PanelNavigationController>().AsSelf();
        }

        private IEnumerable<IInstaller> FetchInstallers()
        {
            yield return new MainPipeInstaller();
            yield return new MainDataInstaller();
            yield return m_BudgetInstaller;
            yield return m_RecommendationInstaller;
            yield return m_DetailInstaller;
            yield return m_EmptyStateInstaller;
        }
    }
}
