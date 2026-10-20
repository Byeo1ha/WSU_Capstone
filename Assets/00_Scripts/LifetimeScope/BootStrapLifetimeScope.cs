using VContainer;
using VContainer.Unity;

public class BootStrapLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponentInHierarchy<BootStrap>();
    }
}
