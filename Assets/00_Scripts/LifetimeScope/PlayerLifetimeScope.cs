using UnityEngine;
using VContainer;
using VContainer.Unity;

public enum PoolKey
{
    NormalAttack,
    HighAttack,
    AirAttack
}

public class PlayerLifetimeScope : LifetimeScope
{
    [Header("키로 등록할 객체들")]
    [SerializeField] private AttackHitboxPool normalAttack;
    [SerializeField] private AttackHitboxPool highAttack;
    [SerializeField] private AttackHitboxPool airAttack;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponentInHierarchy<PlayerMove>();
        builder.RegisterComponentInHierarchy<PlayerJump>();
        builder.RegisterComponentInHierarchy<PlayerSpriteFlip>();
        builder.RegisterComponentInHierarchy<PlayerNormalAttack>();
        builder.RegisterComponentInHierarchy<PlayerHighAttack>();
        builder.RegisterComponentInHierarchy<PlayerAirAttack>();

        builder.RegisterComponent(normalAttack).Keyed(PoolKey.NormalAttack);
        builder.RegisterComponent(highAttack).Keyed(PoolKey.HighAttack);
        builder.RegisterComponent(airAttack).Keyed(PoolKey.AirAttack);
    }
}
