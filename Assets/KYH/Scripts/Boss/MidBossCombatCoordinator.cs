using UnityEngine;

public class MidBossCombatCoordinator : MonoBehaviour
{
    [Header("중간보스 체력")]
    [SerializeField] private BossHealth bossHealth;

    [Header("패턴 실행기")]
    [SerializeField] private BossPatternRunner patternRunner;

    [Header("사망 연출")]
    [SerializeField] private MidBossVisual bossVisual;
    [SerializeField] private Rigidbody2D bossBody;

    private bool hasDied;
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
            BeginDeath();
    }

    private void CheckDeath()
    {
        if(bossHealth.MaxHealth <= 0)
        {   
            StopCombat();
            return;
        }

        if (bossHealth.IsDead)
            BeginDeath();
    }

    private void BeginDeath()
    {
        if (hasDied)
            return;

        hasDied = true;

        patternRunner.SetPatternStartAllowed(false);

        if (bossBody != null)
        {
            bossBody.linearVelocity = Vector2.zero;
            bossBody.angularVelocity = 0f;

            // 떨림 연출 중 물리 처리와 위치 변경이 충돌하지 않게 한다.
            bossBody.simulated = false;
        }

        // Idle 상태 알림이 오기 전에 시각 처리를 잠근다.
        if (bossVisual != null)
            bossVisual.BeginDeath();
        else
            Debug.LogError("사망 연출용 MidBossVisual을 연결해 주세요.", this);

        StopCombat();
    }

    private void StopCombat()
    {
        patternRunner.SetPatternStartAllowed(false);
        patternRunner.StopPattern();
    }
}

