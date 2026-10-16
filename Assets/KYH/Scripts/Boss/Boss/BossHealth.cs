using UnityEngine;
using System;

public class BossHealth : MonoBehaviour
{
    [Header("체력 설정")]
    [SerializeField, Min(1)] private int maxHealth = 600;

    private int currentHealth;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public bool IsDead => currentHealth <= 0;
    public event Action<int, int> HealthChanged;
    
    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        if (damage <= 0 || IsDead)
            return;

        currentHealth = Mathf.Max(0, currentHealth - damage);
        HealthChanged?.Invoke(currentHealth, maxHealth);

        if (IsDead)
            Die();
    }

    private void Die()
    {
        Debug.Log("보스가 사망했습니다.", this);
    } 

    [ContextMenu("테스트/피해 10 받기")]
    private void TestTakeDamage(){
        if (!Application.isPlaying)
            return;

            TakeDamage(10);
    }
    
}
