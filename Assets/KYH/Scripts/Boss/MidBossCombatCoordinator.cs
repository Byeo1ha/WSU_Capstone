using UnityEngine;

public class MidBossCombatCoordinator : MonoBehaviour
{
    [Header("중간보스 체력")]
    [SerializeField] private BossHealth bossHealth;

    [Header("패턴 실행기")]
    [SerializeField] private BossPatternRunner patternRunner;

    private bool hasStarted;

    private void Awake()
    {
        if(bossHealth == null || patternRunner == null)
        {
            Debug.LogError("Boss Health와 Pattern Runner를 연결해 주세요.", this);
        
            enabled = false;
        }
    }

    private void OnEnable()
    {
        bossHealth.HealthChanged += HandleHealthChanged;

        if(hasStarted)
            CheckDeath();
    }

    private void Start()
    {
        hasStarted = true;
        CheckDeath();
    }

    private void OnDisable()
    {
        if(bossHealth != null)
            bossHealth.HealthChanged -= HandleHealthChanged;
    }

    private void HandleHealthChanged(int currentHealth, int maxHealth)
    {
        if(currentHealth <= 0)
            StopCombat();
    }

    private void CheckDeath()
    {
        if(bossHealth.MaxHealth <= 0 || bossHealth.IsDead)
            StopCombat();
    }

    private void StopCombat()
    {
        patternRunner.SetPatternStartAllowed(false);
        patternRunner.StopPattern();
    }
}

