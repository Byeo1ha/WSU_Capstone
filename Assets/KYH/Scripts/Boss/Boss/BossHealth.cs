using System;
using UnityEngine;

public class BossHealth : MonoBehaviour, IDamageable
{
    [Header("보스 데이터")]
    [SerializeField] private BossData bossData;
    
    private HitFlashEffect hitFlashEffect;
    private int maxHealth;
    private int currentHealth;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    private int minimumHealth;
    private bool isInvincible;
    public bool IsDead => currentHealth <= 0;

    public event Action<int, int> HealthChanged;

    private void Awake()
    {
        if (bossData == null)
        {
            Debug.LogError("Boss Data를 연결해 주세요.", this);
            return;
        }

        maxHealth = bossData.MaxHealth;
        currentHealth = maxHealth;

        hitFlashEffect = GetComponent<HitFlashEffect>();
    }

    public void SetMinimumHealth(int value)
    {
        minimumHealth = Mathf.Clamp(value, 0, maxHealth);
    }

    public void SetInvincible(bool value)
    {
        isInvincible = value;
    }

    public bool ResetHealth()
    {
        if(maxHealth <= 0)
        {
            Debug.LogError("최대 체력이 초기화되지 않았습니다.", this);
            return false;
        }

        currentHealth = maxHealth;
        HealthChanged?.Invoke(currentHealth, maxHealth);

        return true;
    }

    public void TakeDamage(int damage)
    {
        if (damage <= 0 || IsDead || isInvincible)
            return;

        int nextHealth = Mathf.Max(minimumHealth, currentHealth - damage);
        currentHealth = Mathf.Min(currentHealth, nextHealth);

        HealthChanged?.Invoke(currentHealth, maxHealth);
        hitFlashEffect.PlayHitFlash();

        if (IsDead)
            Die();
    }

    private void Die()
    {
        Debug.Log("보스가 사망했습니다.", this);
    }

    [ContextMenu("테스트/피해 10 받기")]
    private void TestTakeDamage()
    {
        if (!Application.isPlaying)
            return;

        TakeDamage(10);
    }

    [ContextMenu("테스트/무적 켜기")]
    private void TestEnableInvincibility()
    {
        if (!Application.isPlaying)
            return;

        SetInvincible(true);
    }

    [ContextMenu("테스트/무적 끄기")]
    private void TestDisableInvincibility()
    {
        if (!Application.isPlaying)
            return;

        SetInvincible(false);
    }
}