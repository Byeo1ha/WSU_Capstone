using UnityEngine;

public class MidBossBlinkController : MonoBehaviour
{
    [Header("중간보스 체력")]
    [SerializeField] private BossHealth bossHealth;
    [Header("블링크 실행 연결")]
    [SerializeField] private BossPatternRunner patternRunner;
    [SerializeField] private BossTargetFinder targetFinder;
    [SerializeField] private MidBossBlinkArea blinkArea;
    [SerializeField] private Rigidbody2D bossBody;
    [Header("블링크 모션")]
    [SerializeField] private MidBossVisual bossVisual;

    public bool IsBlinking {get; private set;}
    public bool BlinkSucceeded {get; private set; }
    private bool hasMovedDuringBlink;
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

        CancelBlink();
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
            CancelBlink();
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

    public bool TryExecuteBlink()
    {
        if(!isActiveAndEnabled || bossHealth == null || bossHealth.IsDead || pendingBlinkCount <= 0)
            return false;

        if(patternRunner == null || targetFinder == null || blinkArea == null || bossBody == null)
        {
            Debug.LogError("블링크 실행기, 대상 탐색기, 범위, 보스몸체를 연결해 주세요.", this);

            return false;
        }

        if(!patternRunner.IsStartAllowed || patternRunner.CurrentState != BossPatternState.Idle || !bossBody.simulated || !bossBody.gameObject.activeInHierarchy)
            return false;

        if(!targetFinder.TryGetTargetPosition(out Vector2 targetPosition))
            return false;

        Vector2 currentPosition = bossBody.position;

        if(!blinkArea.TryGetRandomX(currentPosition.x, targetPosition.x, out float destinationX))
            return false;

        if(!TryConsumeBlinkRequest())
            return false;

        Vector2 destination = new Vector2(destinationX, currentPosition.y);

        bossBody.linearVelocity = Vector2.zero;
        bossBody.angularVelocity = 0f;
        bossBody.position = destination;

        Vector3 worldPosition = bossBody.transform.position;
        worldPosition.x = destination.x;
        worldPosition.y = destination.y;
        bossBody.transform.position = worldPosition;

        Debug.Log($"블링크 이동: {currentPosition.x:F2} -> {destinationX:F2}," + $"남은 예약: {pendingBlinkCount}", this);

        return true;
    }

    public bool TryConsumeBlinkRequest()
    {
        if(!isActiveAndEnabled || bossHealth == null || bossHealth.IsDead || pendingBlinkCount <= 0)
            return false;

        pendingBlinkCount--;
        return true;
    }

    public bool TryBeginBlink()
    {
        if (!isActiveAndEnabled ||
            IsBlinking ||
            bossHealth == null ||
            bossHealth.IsDead ||
            pendingBlinkCount <= 0)
            return false;

        if (patternRunner == null ||
            !patternRunner.IsStartAllowed ||
            patternRunner.CurrentState != BossPatternState.Idle ||
            bossVisual == null)
            return false;

        BlinkSucceeded = false;
        hasMovedDuringBlink = false;
        IsBlinking = true;

        if (!bossVisual.TryPlayBlink())
        {
            CancelBlink();
            return false;
        }

        return true;
    }

    public void ExecuteBlinkMovement()
    {
        if (!IsBlinking || hasMovedDuringBlink)
            return;

        hasMovedDuringBlink = TryExecuteBlink();

        if (!hasMovedDuringBlink)
            CancelBlink();
    }

    public void FinishBlink()
    {
        if (!IsBlinking)
            return;

        BlinkSucceeded = hasMovedDuringBlink;
        IsBlinking = false;
    }

    public void CancelBlink()
    {
        IsBlinking = false;
        BlinkSucceeded = false;
        hasMovedDuringBlink = false;
    }
}
