using VContainer;
using VContainer.Unity;

public class StageLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponentInHierarchy<TutorialEvent>();
    }
}
