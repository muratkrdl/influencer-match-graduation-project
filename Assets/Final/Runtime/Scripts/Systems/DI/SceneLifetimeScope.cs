using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Final.Systems.DI
{
    public sealed class SceneLifetimeScope : LifetimeScope
    {
        [SerializeField] private MonoInstaller[] m_Installers;

        protected override void Awake()
        {
            base.Awake();
            AutoInjectSceneObjects();
        }

        protected override void Configure(IContainerBuilder builder)
        {
            foreach (IInstaller installer in m_Installers)
            {
                installer.Install(builder);
            }
        }

        private void AutoInjectSceneObjects()
        {
            MonoBehaviour[] all = FindObjectsOfType<MonoBehaviour>(true);
            for (int i = 0; i < all.Length; i++)
            {
                Container.Inject(all[i]);
            }
        }
    }
}
