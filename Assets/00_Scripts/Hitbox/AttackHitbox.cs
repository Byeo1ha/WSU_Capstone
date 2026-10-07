using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;

[RequireComponent(typeof(SpriteRenderer))]
public class AttackHitbox : MonoBehaviour
{
    [Header("피해량")]
    [SerializeField] private int damage;

    private IObjectPool<AttackHitbox> pool;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        TestDeActive().Forget();
    }

    public void SetPool(IObjectPool<AttackHitbox> pool)
    {
        this.pool = pool;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<IDamageable>(out var damageable))
        {
            damageable.TakeDamage(damage);
        }
    }

    public void SetDirection(bool value)
    {
        spriteRenderer.flipX = value;
    }

    private async UniTask TestDeActive()
    {
        await UniTask.Delay(
            TimeSpan.FromSeconds(0.4f), 
            cancellationToken: this.GetCancellationTokenOnDestroy());

        pool.Release(this);
    }

}
