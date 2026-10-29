using UnityEngine;
using VContainer;
using VContainer.Unity;

public class UILifetimeScope : LifetimeScope
{
    [SerializeField] private UIHealth uiHealth;
    [SerializeField] private UIDie uiDie;

    protected override void Configure(IContainerBuilder builder)
    {
        
        builder.RegisterComponent(uiHealth);
        builder.RegisterComponent(uiDie);
    }
}
