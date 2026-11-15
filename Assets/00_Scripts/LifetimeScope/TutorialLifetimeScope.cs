using VContainer;
using VContainer.Unity;

public class TutorialLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponentInHierarchy<TutorialEvent>();
    }
}
