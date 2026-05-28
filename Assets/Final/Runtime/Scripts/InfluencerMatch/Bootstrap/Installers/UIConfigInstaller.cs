using System;
using Final.InfluencerMatch.Common;
using Final.InfluencerMatch.Splash;
using Final.InfluencerMatch.Transition;
using Final.Systems.DI;
using VContainer;
using VContainer.Unity;

namespace Final.InfluencerMatch.Bootstrap
{
    /// <summary>
    /// Loads the cross-scene UI configuration assets (splash + framework-shared + screen fader) through Addressables and registers them on the BootLifetimeScope alongside an AddressableHandleRegistry that releases the handles on app shutdown.
    /// </summary>
    [Serializable]
    public class UIConfigInstaller : IInstaller
    {
        private const string k_SplashConfigAddress = "Data/SplashConfig";
        private const string k_UISharedConfigAddress = "Data/UISharedConfig";
        private const string k_ScreenFaderConfigAddress = "Data/ScreenFaderConfig";

        void IInstaller.Install(IContainerBuilder builder)
        {
            AddressableHandleRegistry registry = new AddressableHandleRegistry();
            builder.RegisterInstance(registry).AsImplementedInterfaces();

            registry.LoadAndRegister<SplashConfig>(builder, k_SplashConfigAddress);
            registry.LoadAndRegister<UISharedConfig>(builder, k_UISharedConfigAddress);
            registry.LoadAndRegister<ScreenFaderConfig>(builder, k_ScreenFaderConfigAddress);
        }
    }
}
