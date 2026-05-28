using Final.Systems.EventBus.Pipes;
using VContainer;
using VContainer.Unity;

namespace Final.Systems.EventBus.Installers
{
    public class ProjectPipeInstaller : IInstaller
    {
        void IInstaller.Install(IContainerBuilder builder)
        {
            builder.Register<ProjectPipe>(Lifetime.Singleton);
        }
    }
}
