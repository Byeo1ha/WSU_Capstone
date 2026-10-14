using UnityEngine;

[RequireComponent(typeof(HitFlashEffect))]
public class EnemyTest : MonoBehaviour, IDamageable
{
    private HitFlashEffect hitFlashEffect;

    private void Awake()
    {
        hitFlashEffect = GetComponent<HitFlashEffect>();
    }

    public void TakeDamage(int damage)
    {
        Debug.Log($"{damage} 만큼의 피해를 입었습니다.");
        hitFlashEffect.PlayHitFlash();
    }
}
