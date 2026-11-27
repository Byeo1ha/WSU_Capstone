using System.Collections;
using UnityEngine;


public class MidBossBombFieldPattern : BossPriorityPattern
{
    [Header("보스 연결")]
    [SerializeField] private BossHealth bossHealth;
    [SerializeField] private BossPatternRunner patternRunner;
    [SerializeField] private BossTargetFinder targetFinder;
    [SerializeField] private MidBossVisual bossVisual;
    [SerializeField] private MidBossBlinkController blinkController;

    [Header("BombField 연결")]
    [SerializeField] private MidBossBombFieldData data;
    [SerializeField] private MidBossBombFieldPool projectilePool;
    [SerializeField] private MidBossBombFieldFloor floor;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Collider2D groundCollider;

    private MidBossBombFieldProjectile activeProjectile;
    private Coroutine explosionWait;
    private Coroutine blinkWait;
    private bool ownsBlink;

    private readonly float[] healthRatios = {0.7f, 0.4f, 0.2f};

    private int nextThresholdIndex;
    private int pendingCount;
    private bool hasStarted;
    private bool hasStartedGrowth;
    private bool canStartFloor;
    private bool hasStartedFloor;

    private Vector2 explosionPosition;
    private float explosionRadius;

    public override bool IsPending => pendingCount > 0;
    public override bool IsRunning => isRunning;

    private bool isRunning;

    private void OnEnable()
    {
        if (bossHealth == null || patternRunner == null || targetFinder == null ||
            bossVisual == null || data == null || projectilePool == null ||
            floor == null || spawnPoint == null || groundCollider == null || blinkController == null)
        {
            Debug.LogError("BombField실행 참조를 모두 연결해 주세요.", this);
            enabled = false;
            return;
        }

        projectilePool.ExplosionFinished += HandleExplosionFinished;
        floor.Finished += HandleFloorFinished;
        patternRunner.StateChanged += HandleStateChanged;

        bossHealth.HealthChanged += HandleHealthChanged;

        if (hasStarted)
            CheckCurrentHealth();
    }

    private void OnDisable()
    {
        if (projectilePool != null)
            projectilePool.ExplosionFinished -= HandleExplosionFinished;

        if (floor != null)
            floor.Finished -= HandleFloorFinished;

        if (patternRunner != null)
            patternRunner.StateChanged -= HandleStateChanged;

        if (bossHealth != null)
            bossHealth.HealthChanged -= HandleHealthChanged;

        CancelPriority();
    }

    private void Start()
    {
        hasStarted = true;
        CheckCurrentHealth();
    }

    private void CheckCurrentHealth()
    {
        HandleHealthChanged(bossHealth.CurrentHealth, bossHealth.MaxHealth);
    }

    private void HandleHealthChanged(int currentHealth, int maxHealth)
    {
        if (currentHealth <= 0)
        {
            pendingCount = 0;

            if (isRunning)
                CancelPriority();

            return;
        }

        if (maxHealth <= 0)
            return;

        while (nextThresholdIndex < healthRatios.Length)
        {
            float threshold = maxHealth * healthRatios[nextThresholdIndex];

            if (currentHealth > threshold)
                break;

            nextThresholdIndex++;
            pendingCount++;
        }
    }

    private void Update()
    {
        if (bossHealth.IsDead)
        {
            pendingCount = 0;

            if (isRunning)
                CancelPriority();
            
            return;
        }

        if (isRunning && !patternRunner.IsStartAllowed)
            CancelPriority();
    } 

    public void RequestPattern()
    {
        if (!isActiveAndEnabled || bossHealth.IsDead)
            return;

        pendingCount++;
    }

    public override bool TryStartPriority()
    {
        if (!isActiveAndEnabled || pendingCount <= 0 || isRunning ||
            bossHealth.IsDead || !patternRunner.IsStartAllowed ||
            patternRunner.CurrentState != BossPatternState.Idle ||
            !projectilePool.isActiveAndEnabled || !floor.isActiveAndEnabled ||
            !blinkController.isActiveAndEnabled || blinkController.IsBlinking ||
            !patternRunner.TrySelectPattern(this))
        {
            return false;
        }

        isRunning = true;
        hasStartedGrowth = false;
        canStartFloor = false;
        hasStartedFloor = false;

        if (!blinkController.TryBeginBlink(false))
        {
            CancelPriority();
            return false;
        }

        ownsBlink = true;
        pendingCount--;

        blinkWait = StartCoroutine(WaitForBlink());
        return true;
    }

    private IEnumerator WaitForBlink()
    {
        while (isRunning && blinkController.IsBlinking)
        {
            if (bossHealth.IsDead || !patternRunner.IsStartAllowed ||
                !blinkController.isActiveAndEnabled)
            {
                blinkWait = null;
                CancelPriority();
                yield break;
            }

            yield return null;
        }

        blinkWait = null;

        if (!isRunning)
            yield break;

        bool succeeded = blinkController.BlinkSucceeded;
        ownsBlink = false;

        if (!succeeded || bossHealth.IsDead || !patternRunner.IsStartAllowed ||
            patternRunner.CurrentState != BossPatternState.Idle || 
            !patternRunner.IsSelectedPattern(this))
        {
            CancelPriority();
            yield break;
        }

        patternRunner.StartPatternWaitingForActiveEvent();

        if (patternRunner.CurrentState != BossPatternState.Windup)
        {
            CancelPriority();
            yield break;
        }

        bossVisual.StartBombFieldAnim();
    }

    public void BeginOrbGrowth()
    {
        if (!isRunning || hasStartedGrowth || bossHealth.IsDead)
            return;

        hasStartedGrowth = true;

        activeProjectile = projectilePool.Rent(spawnPoint.position);

        if (activeProjectile == null)
        {
            CancelPriority();
            return;
        }

        activeProjectile.GrowthFinished += HandleGrowthFinished;
        activeProjectile.ExplosionRequested += HandleExplosionStarted;

        patternRunner.EnterActiveFromEvent(this);

        if (!activeProjectile.BeginGrowth())
            CancelPriority();
    }

    private void HandleGrowthFinished(MidBossBombFieldProjectile projectile)
    {
        projectile.GrowthFinished -= HandleGrowthFinished;

        if (!isRunning || projectile != activeProjectile)
            return;

        if (!targetFinder.TryGetTargetPosition(out Vector2 targetPosition))
        {
            CancelPriority();
            return;
        }

        Vector2 destination = new Vector2(targetPosition.x, groundCollider.bounds.max.y);

        if (!projectile.Launch(destination, data))
            CancelPriority();
    }

    private void HandleExplosionStarted(MidBossBombFieldProjectile projectile, Vector3 position)
    {
        if (!isRunning || projectile != activeProjectile || bossHealth.IsDead)
            return;

        explosionPosition = position;
        explosionRadius = projectile.ExplosionRadius;

        if (!floor.PrepareWarning(data, explosionPosition, explosionRadius))
            CancelPriority();
    }

    private void HandleExplosionFinished(Vector3 position, float radius)
    {
        if (!isRunning || activeProjectile == null)
            return;

        activeProjectile.GrowthFinished -= HandleGrowthFinished;
        activeProjectile.ExplosionRequested -= HandleExplosionStarted;
        activeProjectile = null;

        explosionPosition = position;
        explosionRadius = radius;

        explosionWait = StartCoroutine(WaitForFloor());
    }

    private IEnumerator WaitForFloor()
    {
        yield return new WaitForSeconds(Mathf.Max(0f, data.AfterExplosionDelay));

        explosionWait = null;

        if (!isRunning || bossHealth.IsDead)
            yield break;

        canStartFloor = true;
        bossVisual.ContinueBombFieldAnim();
    }

    public void BeginFloorFromAnimation()
    {
        if (!isRunning || !canStartFloor || hasStartedFloor || bossHealth.IsDead)
        {
            return;
        }

        hasStartedFloor = true;

        if (!floor.Begin(data, explosionPosition, explosionRadius))
            CancelPriority();
    }

    private void HandleFloorFinished()
    {
        if (!isRunning || !hasStartedFloor)
            return;

        isRunning = false;

        if (patternRunner.IsSelectedPattern(this))
            patternRunner.CompleteActive();
    }

    private void HandleStateChanged(BossPatternState state)
    {
        if (isRunning && state == BossPatternState.Idle)
            CancelPriority();
    }

    public override void CancelPriority()
    {
        bool wasRunning = isRunning;
        isRunning = false;

        if (blinkWait != null)
        {
            StopCoroutine(blinkWait);
            blinkWait = null;
        }

        if (ownsBlink)
        {
            ownsBlink = false;

            if (blinkController != null)
                blinkController.CancelBlink();
        }

        if (explosionWait != null)
        {
            StopCoroutine(explosionWait);
            explosionWait = null;
        }

        if (activeProjectile != null)
        {
            activeProjectile.GrowthFinished -= HandleGrowthFinished;
            activeProjectile.ExplosionRequested -= HandleExplosionStarted;
            activeProjectile = null;
        }

        if (!wasRunning)
            return;

        projectilePool.Return();
        floor.Stop();
        bossVisual.CancelBombFieldAnim();

        if (patternRunner.IsSelectedPattern(this))
            patternRunner.StopPattern();
    }     

    [ContextMenu("테스트/BombField 다음 패턴으로 예약")]
    private void TestRequest()
    {
        if (Application.isPlaying)
            RequestPattern();
    }


}
