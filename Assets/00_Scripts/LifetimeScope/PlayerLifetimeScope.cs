using UnityEngine;
using VContainer;
using VContainer.Unity;

public enum PoolKey
{
    NormalAttack,
    FinalNormalAttack,
    HighAttack,
    AirAttack,
    LowAttack
}

public class PlayerLifetimeScope : LifetimeScope
{
    [Header("키로 등록할 객체들")]
    [SerializeField] private AttackHitboxPool normalAttack;
    [SerializeField] private AttackHitboxPool finalNormalAttack;
    [SerializeField] private AttackHitboxPool highAttack;
    [SerializeField] private AttackHitboxPool airAttack;
    [SerializeField] private AttackHitboxPool lowAttack;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponentInHierarchy<PlayerMove>();
        builder.RegisterComponentInHierarchy<PlayerJump>();
        builder.RegisterComponentInHierarchy<PlayerNormalAttack>();
        builder.RegisterComponentInHierarchy<PlayerHighAttack>();
        builder.RegisterComponentInHierarchy<PlayerAirAttack>();
        builder.RegisterComponentInHierarchy<PlayerLowAttack>();
        builder.RegisterComponentInHierarchy<PlayerDodge>();
        builder.RegisterComponentInHierarchy<PlayerAnimation>();
        builder.RegisterComponentInHierarchy<PlayerAttackFeedback>();

        builder.RegisterComponent(normalAttack).Keyed(PoolKey.NormalAttack);
        builder.RegisterComponent(finalNormalAttack).Keyed(PoolKey.FinalNormalAttack);
        builder.RegisterComponent(highAttack).Keyed(PoolKey.HighAttack);
        builder.RegisterComponent(airAttack).Keyed(PoolKey.AirAttack);
        builder.RegisterComponent(lowAttack).Keyed(PoolKey.LowAttack);
    }
}
