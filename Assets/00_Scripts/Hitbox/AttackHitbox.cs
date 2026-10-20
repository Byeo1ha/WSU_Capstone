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
        
        DeActive().Forget();
    }

    private void OnDisable()
    {
        cts?.Cancel();
        cts?.Dispose();
        cts = null;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Attack(collision);
    }

    // 외부에서 이 Hitbox가 저장되야할 풀을 참조시켜주는 함수
    public void SetPool(IObjectPool<AttackHitbox> pool)
    {
        this.pool = pool;
    }

    // 외부에서 이 Hitbox에 타격되면 발생시킬 피격 이펙트를 가진 풀을 참조시켜주는 함수
    public void SetHitEffectPool(HitEffectPool hitEffectPool)
    {
        this.hitEffectPool = hitEffectPool;
    }

    // 외부에서 이 객체의 좌우 반전 여부를 결정해주는 함수
    public void SetDirection(bool value)
    {
        spriteRenderer.flipX = value;
    }

    // 적을 감지하면 데미지를 주고 연출을 재생, 피격 이펙트를 참조 중이면 피격 이펙트도 재생하는 함수 
    private void Attack(Collider2D collision)
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

    // 스스로를 풀로 되돌리기 위한 함수
    private async UniTask DeActive()
    {
        await UniTask.Delay(
            TimeSpan.FromSeconds(0.2f), 
            cancellationToken: cts.Token);

        pool.Release(this);
    }

}
