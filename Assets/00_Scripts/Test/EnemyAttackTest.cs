using UnityEngine;

public class EnemyAttackTest : MonoBehaviour
{
    [Header("테스트용 피해량")]
    private int damage = 5;

    private int targetLayer;

    private void Awake()
    {
        targetLayer = LayerMask.NameToLayer("Player");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<IDamageable>(out var damageable) &&
            collision.gameObject.layer == targetLayer)
        {
            damageable.TakeDamage(damage);
        }
    }
}
