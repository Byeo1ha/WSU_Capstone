using UnityEngine;
using UnityEngine.AI;

public class MidBossBlinkController : MonoBehaviour
{
    [Header("중간보스 체력")]
    [SerializeField] private BossHealth bossHealth;
    private readonly float[] healthRations = {0.75f, 0.5f, 0.25f};

    private int nextThresholdIndex;
    private int pendingBlinkCount;
    private bool hasStarted;

    public int PendingBlinkCount => pendingBlinkCount;

    private void Awake()
    {
        if(bossHealth == null)
        {
            Debug.LogError("중간보스 체력을 연결해 주세요.", this);
            enabled = false;
        }   
    }

    private void OnEnable()
    {
        if(bossHealth == null)
            return;

        bossHealth.HealthChanged += HandleHealthChanged;

        if(hasStarted)
            CheckCurrentHealth();
    }

    private void Start()
    {
        hasStarted = true;
        CheckCurrentHealth();
    }

    private void OnDisable()
    {
        if(bossHealth != null)
            bossHealth.HealthChanged -= HandleHealthChanged;
    }

    private void CheckCurrentHealth()
    {
        HandleHealthChanged(bossHealth.CurrentHealth, bossHealth.MaxHealth);
    }

    private void HandleHealthChanged(int currentHealth, int maxHealth)
    {
        if(maxHealth <= 0 || currentHealth <= 0)
        {
            pendingBlinkCount = 0;
            return;
        }

        while(nextThresholdIndex < healthRations.Length)
        {
            int threshold = Mathf.CeilToInt(maxHealth * healthRations[nextThresholdIndex]);

            if(currentHealth > threshold)
                break;

            nextThresholdIndex++;
            pendingBlinkCount++;

            Debug.Log($"블링크 예약: 기준체력{threshold}, 남은 예약 {pendingBlinkCount}", this);
        }
    }

    public bool TryConsumeBlinkRequest()
    {
        if(!isActiveAndEnabled || bossHealth == null || bossHealth.IsDead || pendingBlinkCount <= 0)
            return false;

        pendingBlinkCount--;
        return true;
    }
}
