using System.Collections.Generic;
using Final.InfluencerMatch.Inputs;
using Final.Systems.DI;
using Final.Systems.EventBus.Installers;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Final.InfluencerMatch.Bootstrap
{
    /// <summary>
    /// App root installer; composes the boot installers and app-wide entry points.
    /// </summary>
    public class BootInstaller : MonoInstaller
    {
        [Header("Installers")]
        [SerializeField] private UIConfigInstaller m_UIConfigInstaller;
        [SerializeField] private TransitionInstaller m_TransitionInstaller;

        protected override void InstallBindings(IContainerBuilder builder)
        {
            ApplyOrientationLock();
            ApplyTargetFrameRate();

            foreach (IInstaller installer in FetchInstallers())
            {
                installer.Install(builder);
            }

            builder.RegisterEntryPoint<InputService>().As<IInputService>();
            builder.RegisterEntryPoint<AppNavigationController>().AsSelf();
        }

        private IEnumerable<IInstaller> FetchInstallers()
        {
            yield return new ProjectPipeInstaller();
            yield return new ServiceInstaller();
            yield return m_UIConfigInstaller;
            yield return m_TransitionInstaller;
        }

        private static void ApplyOrientationLock()
        {
            Screen.orientation = ScreenOrientation.Portrait;
            Screen.autorotateToPortrait = true;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeLeft = false;
            Screen.autorotateToLandscapeRight = false;
        }

        private static void ApplyTargetFrameRate()
        {
            Application.targetFrameRate = Mathf.RoundToInt((float)Screen.currentResolution.refreshRateRatio.value);
        }
    }
}
