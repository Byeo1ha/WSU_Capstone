using UnityEngine;
using UnityEngine.UI;

public class BossHealthBar : MonoBehaviour
{
    [Header("표시할 보스 체력")]
    [SerializeField] private BossHealth bossHealth;

    [Header("체력 표시 이미지")]
    [SerializeField] private Image healthFill;

    private bool hasStarted;

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
        bossHealth.HealthChanged -= UpdateHealthBar;
    }

    private void UpdateHealthBar(int currentHealth, int MaxHealth)
    {
        healthFill.fillAmount = (float)currentHealth / MaxHealth;
    } 
}
