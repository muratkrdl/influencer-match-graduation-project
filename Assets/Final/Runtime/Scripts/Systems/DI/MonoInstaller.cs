using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Final.Systems.DI
{
    public abstract class MonoInstaller : MonoBehaviour, IInstaller
    {
        void IInstaller.Install(IContainerBuilder builder) => InstallBindings(builder);

        protected abstract void InstallBindings(IContainerBuilder builder);
    }
}
