using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;
using VContainer;

public enum ShakingType
{
    Normal,
    Power
}

[RequireComponent(typeof(SpriteRenderer))]
public class AttackHitbox : MonoBehaviour
{
    [Header("피해량")]
    [SerializeField] private int damage;
    [SerializeField] private ShakingType shakingType;

    private IObjectPool<AttackHitbox> pool;

    private SpriteRenderer spriteRenderer;
    private PlayerAttackFeedback playerAttackFeedback;
    private HitEffectPool hitEffectPool;

    private int targetLayer;

    private CancellationTokenSource cts;

    [Inject]
    public void Construct(PlayerAttackFeedback playerAttackFeedback)
    {
        this.playerAttackFeedback = playerAttackFeedback;
    }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        targetLayer = LayerMask.NameToLayer("Enemy");
    }

    private void OnEnable()
    {
        cts = new CancellationTokenSource();
        
        TestDeActive().Forget();
    }

    private void OnDisable()
    {
        cts?.Cancel();
        cts?.Dispose();
        cts = null;
    }

    public void SetPool(IObjectPool<AttackHitbox> pool)
    {
        this.pool = pool;
    }

    public void SetHitEffectPool(HitEffectPool hitEffectPool)
    {
        this.hitEffectPool = hitEffectPool;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<IDamageable>(out var damageable) &&
            collision.gameObject.layer == targetLayer)
        {
            Vector3 hitPosition = collision.transform.position;

            damageable.TakeDamage(damage);
            playerAttackFeedback.StartFeedback(shakingType);

            if (hitEffectPool != null)
                hitEffectPool.Spawn(hitPosition, spriteRenderer.flipX);
        }
    }

    public void SetDirection(bool value)
    {
        spriteRenderer.flipX = value;
    }

    private async UniTask TestDeActive()
    {
        await UniTask.Delay(
            TimeSpan.FromSeconds(0.2f), 
            cancellationToken: cts.Token);

        pool.Release(this);
    }

}
