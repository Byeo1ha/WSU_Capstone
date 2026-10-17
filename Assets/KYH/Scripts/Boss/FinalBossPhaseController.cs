using UnityEngine;

public class FinalBossPhaseController : MonoBehaviour
{
    [Header("보스 체력")]
    [SerializeField] private BossHealth bossHealth;
    [Header("현재 페이즈")]
    [SerializeField] private FinalBossPhase currentPhase = FinalBossPhase.Phase1;

    public FinalBossPhase CurrentPhase => currentPhase;

    private void OnEnable()
    {
        bossHealth.HealthChanged += HandleHealthChanged;        
    }

    private  void OnDisable()
    {
        bossHealth.HealthChanged -= HandleHealthChanged;
    }

    private void HandleHealthChanged(int currentHealth, int maxHealth)
    {
        float healthRatio = (float)currentHealth / maxHealth;

        Debug.Log($"페이즈 관리: 체력 {healthRatio:P0}", this);
    }
}
