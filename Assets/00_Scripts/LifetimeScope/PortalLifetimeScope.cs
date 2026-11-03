using UnityEngine;
using VContainer;
using VContainer.Unity;

public class PortalLifetimeScope : LifetimeScope
{
    [SerializeField] private Portal portal;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponent(portal);
    }
}
