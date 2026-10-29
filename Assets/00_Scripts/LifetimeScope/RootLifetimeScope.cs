using UnityEngine;
using VContainer;
using VContainer.Unity;

public class RootLifetimeScope : LifetimeScope
{
    [SerializeField] private ScreenFader screenFader;
    [SerializeField] private PlayerAttributes playerAttributes;
    [SerializeField] private InputManager inputManager;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponent(screenFader);
        builder.Register<SceneFlowService>(Lifetime.Singleton);
        builder.RegisterComponent(playerAttributes);
        builder.RegisterComponent(inputManager);
    }
}
