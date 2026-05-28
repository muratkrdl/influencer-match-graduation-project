using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Final.Systems.DI
{
    /// <summary>
    /// Root DI scope for the app; survives scene transitions and parents every scene scope.
    /// </summary>
    public sealed class BootLifetimeScope : LifetimeScope
    {
        [SerializeField] private MonoInstaller[] m_Installers;

        protected override void Awake()
        {
            DontDestroyOnLoad(gameObject);
            base.Awake();
        }

        protected override void Configure(IContainerBuilder builder)
        {
            foreach (IInstaller installer in m_Installers)
            {
                installer.Install(builder);
            }
        }
    }
}
