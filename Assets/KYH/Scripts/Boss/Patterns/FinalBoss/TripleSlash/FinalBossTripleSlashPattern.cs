using UnityEngine;

public class FinalBossTripleSlashPattern : BossAttackPattern
{
    [Header("전투 연결")]
    [SerializeField] private BossPatternRunner patternRunner;
    [SerializeField] private BossHealth bossHealth;
    [SerializeField] private BossTargetFinder targetFinder;

    [Header("참격 연결")]
    [SerializeField] private FinalBossTripleSlashData data;
    [SerializeField] private FinalBossTripleSlashPool projectilePool;
    [SerializeField] private Transform firePoint;

    [Header("보스 모션")]
    [SerializeField] private FinalBossAnimation bossAnimation;
    [SerializeField] private SpriteRenderer bossRenderer;

    private FinalBossTripleSlashProjectile thirdProjectile;
    private int attackDirection;
    private int firedCount;
    private bool ownsExecution;

    private void OnEnable()
    {
        if (patternRunner == null || bossHealth == null || targetFinder == null ||
            data == null || projectilePool == null || firePoint == null || bossAnimation == null || bossRenderer == null)
        {
            Debug.LogError("세번베기의 전투,참격,모션 참조를 연결해 주세요.", this);
            enabled = false;
            return;
        }

        patternRunner.StateChanged += HandleStateChanged;
        bossHealth.HealthChanged += HandleHealthChanged;
        projectilePool.ProjectileReturned += HandleProjectileReturned;
    }

    private void OnDisable()
    {
        if (patternRunner != null)
            patternRunner.StateChanged -= HandleStateChanged;

        if (bossHealth != null)
            bossHealth.HealthChanged -= HandleHealthChanged;

        if (projectilePool != null)
            projectilePool.ProjectileReturned -= HandleProjectileReturned;

        AbortPattern();
    }

    private bool CanExecute()
    {
        return isActiveAndEnabled &&
               patternRunner.IsStartAllowed &&
               !bossHealth.IsDead &&
               projectilePool.isActiveAndEnabled &&
               bossAnimation.CanPlayTripleSlash;
    }

    private void Update()
    {
        if (ownsExecution && !CanExecute())
            AbortPattern();
    }

    private void HandleStateChanged(BossPatternState state)
    {
        switch (state)
        {
            case BossPatternState.Windup:
                if (!patternRunner.IsSelectedPattern(this))
                    return;

                ownsExecution = true;
                firedCount = 0;
                thirdProjectile = null;

                if (!CanExecute() ||
                    !targetFinder.TryGetTargetPosition(out Vector2 targetPosition))
                {
                    AbortPattern();
                    return;
                }

                attackDirection = targetPosition.x < transform.position.x ? -1 : 1;
                bossRenderer.flipX = attackDirection < 0;
                bossAnimation.StartTripleSlashAnim();
                break;

            case BossPatternState.Recovery:
                EndExecution(completedNormally: true);
                break;

            case BossPatternState.Idle:
                EndExecution(completedNormally: false);
                break;
        }
    }

    public void OnTripleSlashFire(int hitNumber)
    {
        if (!ownsExecution)
            return;

        if (!CanExecute())
        {
            AbortPattern();
            return;
        }

        if (hitNumber != firedCount + 1 || hitNumber < 1 || hitNumber > 3)
        {
            Debug.LogWarning("세번베기 발사 이벤트의 정수 값을 확인해 주세요.", this);
            AbortPattern();
            return;
        }

        if (hitNumber == 1)
            patternRunner.EnterActiveFromEvent(this);

        if (patternRunner.CurrentState != BossPatternState.Active)
            return;

        Vector3 localPoint = firePoint.localPosition;
        localPoint.x *= attackDirection;

        Vector3 position = transform.TransformPoint(localPoint);

        FinalBossTripleSlashProjectile projectile = projectilePool.RentAndLaunch(
            position, attackDirection, data.MoveSpeed, data.GetSlash(hitNumber));

        if (projectile == null)
        {
            Debug.LogWarning("세번베기 참격을 발사하지 못했습니다.", this);
            AbortPattern();
            return;
        }

        firedCount = hitNumber;

        if (hitNumber == 3)
            thirdProjectile = projectile;
    }

    public void OnTripleSlashAnimationFinished()
    {
        if (ownsExecution)
            bossAnimation.FinishTripleSlashAnim();
    }

    private void HandleProjectileReturned(FinalBossTripleSlashProjectile projectile)
    {
        if (!ownsExecution || projectile != thirdProjectile)
            return;

        thirdProjectile = null;

        if (patternRunner.CurrentState == BossPatternState.Active)
            patternRunner.CompleteActive();
    }

    private void HandleHealthChanged(int currentHealth, int maxHealth)
    {
        if (currentHealth <= 0)
            AbortPattern();
    }

    private void EndExecution(bool completedNormally)
    {
        if (!ownsExecution)
            return;

        ownsExecution = false;
        thirdProjectile = null;
        firedCount = 0;

        projectilePool.ReturnAll();

        if (completedNormally)
            bossAnimation.FinishTripleSlashAnim();
        else
            bossAnimation.CancelTripleSlashAnim();
    }

    private void AbortPattern()
    {
        bool shouldStopRunner = ownsExecution;

        EndExecution(completedNormally: false);

        if (shouldStopRunner && patternRunner != null)
            patternRunner.StopPattern();
    }

    public override bool TryStartAttack()
    {
        if (!CanSelect || !CanExecute() || !patternRunner.TrySelectPattern(this))
            return false;

        patternRunner.StartPatternWaitingForActiveEvent();

        return ownsExecution && patternRunner.CurrentState == BossPatternState.Windup;
    }

    [ContextMenu("테스트/세번베기 시작")]
    private void TestStartTripleSlash()
    {
        if (Application.isPlaying)
            TryStartAttack();
    }
}
