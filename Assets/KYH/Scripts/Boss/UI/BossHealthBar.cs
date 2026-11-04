using System;
using UnityEngine;
using UnityEngine.UI;

public class BossHealthBar : MonoBehaviour
{
    [Header("표시할 보스 체력")]
    [SerializeField] private BossHealth bossHealth;

    [Header("체력 표시 이미지")]
    [SerializeField] private Image healthFill;

    private bool hasStarted;

    private void Awake()
    {
        if (bossHealth == null || healthFill == null)
        {
            Debug.LogError("BossHealthBar: BossHealth 또는 HealthFill이 할당되지 않았습니다.", this);

            enabled = false;
        }
    }

    private void OnEnable()
    {
        bossHealth.HealthChanged += UpdateHealthBar;

        if (hasStarted)
            UpdateHealthBar(bossHealth.CurrentHealth, bossHealth.MaxHealth);
    }

    private void Start()
    {
        hasStarted = true;
        UpdateHealthBar(bossHealth.CurrentHealth, bossHealth.MaxHealth);
    }

    private void OnDisable()
    {
        if(bossHealth != null)
        {
        bossHealth.HealthChanged -= UpdateHealthBar;
        }
    }

    private void UpdateHealthBar(int currentHealth, int maxHealth)
    {
        healthFill.fillAmount = maxHealth > 0 ? Mathf.Clamp01((float)currentHealth / maxHealth) : 0f;
    } 
}
