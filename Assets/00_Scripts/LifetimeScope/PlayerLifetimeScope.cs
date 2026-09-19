using VContainer;
using VContainer.Unity;

public class PlayerLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponentInHierarchy<PlayerMove>();
        builder.RegisterComponentInHierarchy<PlayerJump>();
        builder.RegisterComponentInHierarchy<PlayerSpriteFlip>();
        builder.RegisterComponentInHierarchy<PlayerNormalAttack>();

        builder.RegisterComponentInHierarchy<NormalAttackPool>();
    }
}
