using System;
using Final.InfluencerMatch.Budget;
using Final.InfluencerMatch.Common;
using Final.InfluencerMatch.Detail;
using Final.InfluencerMatch.Recommendation;
using Final.Systems.DI;
using VContainer;
using VContainer.Unity;

namespace Final.InfluencerMatch.Bootstrap
{
    /// <summary>
    /// Loads every Main-scene-scoped authored asset (influencer database, category / matching / budget / recommendation / score-bar configs) through Addressables and registers them on the Main scope alongside an AddressableHandleRegistry that releases the handles on scope dispose.
    /// </summary>
    [Serializable]
    public class MainDataInstaller : IInstaller
    {
        private const string k_InfluencerDatabaseAddress = "Data/InfluencerDatabase";
        private const string k_CategoryConfigAddress = "Data/CategoryConfig";
        private const string k_MatchingConfigAddress = "Data/MatchingConfig";
        private const string k_BudgetConfigAddress = "Data/BudgetConfig";
        private const string k_RecommendationConfigAddress = "Data/RecommendationConfig";
        private const string k_ScoreBarConfigAddress = "Data/ScoreBarConfig";
        private const string k_PlatformConfigAddress = "Data/PlatformConfig";

        void IInstaller.Install(IContainerBuilder builder)
        {
            AddressableHandleRegistry registry = new AddressableHandleRegistry();
            builder.RegisterInstance(registry).AsImplementedInterfaces();

            registry.LoadAndRegister<InfluencerDatabase>(builder, k_InfluencerDatabaseAddress);
            registry.LoadAndRegister<CategoryConfig>(builder, k_CategoryConfigAddress);
            registry.LoadAndRegister<MatchingConfig>(builder, k_MatchingConfigAddress);
            registry.LoadAndRegister<BudgetConfig>(builder, k_BudgetConfigAddress);
            registry.LoadAndRegister<RecommendationConfig>(builder, k_RecommendationConfigAddress);
            registry.LoadAndRegister<ScoreBarConfig>(builder, k_ScoreBarConfigAddress);
            registry.LoadAndRegister<PlatformConfig>(builder, k_PlatformConfigAddress);
        }
    }
}
