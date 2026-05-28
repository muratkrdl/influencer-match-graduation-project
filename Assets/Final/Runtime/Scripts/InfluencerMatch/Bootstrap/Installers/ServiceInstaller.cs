using Final.InfluencerMatch.Budget;
using Final.InfluencerMatch.Common;
using VContainer;
using VContainer.Unity;

namespace Final.InfluencerMatch.Bootstrap
{
    /// <summary>
    /// Registers the app-wide services and session state that survive scene transitions.
    /// </summary>
    public class ServiceInstaller : IInstaller
    {
        void IInstaller.Install(IContainerBuilder builder)
        {
            builder.Register<IPricingService, PricingService>(Lifetime.Singleton);
            builder.Register<IMatchingService, MatchingService>(Lifetime.Singleton);
            builder.Register<AppState>(Lifetime.Singleton);
        }
    }
}
