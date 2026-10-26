using UnityEngine;

public class BossDamageTargetTest : MonoBehaviour, IDamageable
{
    [SerializeField, Min(1)] private int maxHealth = 100;

    private int currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        if (damage <= 0 || currentHealth <= 0)
            return;

        currentHealth = Mathf.Max(0, currentHealth - damage);

        Debug.Log(
            $"테스트 표적: 피해 {damage}, 남은 체력 {currentHealth}",
            this);
    }
}