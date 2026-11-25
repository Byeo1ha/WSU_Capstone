using UnityEngine;

public class FinalBossComboPattern : BossAttackPattern
{
    [Header("실행 연결")]
    [SerializeField] private BossPatternRunner patternRunner;
    [SerializeField] private BossHealth bossHealth;

    [Header("공격 연결")]
    [SerializeField] private BossTargetFinder targetFinder;
    [SerializeField] private FinalBossComboData data;
    [SerializeField] private FinalBossSlash slash;

    [SerializeField] private FinalBossAnimation bossAnimation;
    [SerializeField] private SpriteRenderer comboRenderer;

    private readonly int idleState = Animator.StringToHash("Base Layer.FinalBoss_Idle");

    private int comboDirection;
    private int completedHits;
    private bool animationFinished;
    private bool ownsExecution;
    
    private void OnEnable()
    {
        if(patternRunner == null || bossHealth == null || targetFinder == null || data == null || slash == null || bossAnimation == null)
        {
            Debug.LogError("칼 연계에 필요한 참조를 모두 연결해 주세요.", this);

            enabled = false;
            return;
        }

        patternRunner.StateChanged += HandleStateChanged;
        bossHealth.HealthChanged += HandleHealthChanged;
    }

    private void OnDisable()
    {
        if(patternRunner != null)
            patternRunner.StateChanged -= HandleStateChanged;

        if(bossHealth != null)
            bossHealth.HealthChanged -= HandleHealthChanged;

        bool shouldStopRunner = ownsExecution;
        CancelExecution();

        if(shouldStopRunner && patternRunner != null)
            patternRunner.StopPattern();
    }

    private bool CanExecute()
    {
        return isActiveAndEnabled &&
               patternRunner.isActiveAndEnabled &&
               !bossHealth.IsDead &&
               targetFinder.isActiveAndEnabled &&
               slash.isActiveAndEnabled;
    }

    private void HandleStateChanged(BossPatternState state)
    {
        switch(state)
        {
            case BossPatternState.Windup:
                if (!patternRunner.IsSelectedPattern(this))
                    return;

                CancelExecution();
                ownsExecution = true;

                if(!CanExecute() ||!targetFinder.TryGetTargetPosition(out Vector2 targetPosition))
                {
                    AbortPattern();
                    return;
                }

                comboDirection = targetPosition.x < slash.transform.position.x ? -1 : 1;
                if (comboRenderer != null)
                    comboRenderer.flipX = comboDirection < 0;
                break;

            case BossPatternState.Active:
                if(ownsExecution)
                    BeginCombo();

                break;

            case BossPatternState.Recovery:
                CancelExecution(completedNormally: true);
                break;

            case BossPatternState.Idle:
                CancelExecution();
                break;
        }
    }

    private void BeginCombo()
    {
        if (!CanExecute() || bossAnimation == null || !bossAnimation.CanPlayCombo)
        {
            Debug.LogWarning("콤보 실행 상태와 Animator의 연결을 확인해주세요.", this);
            AbortPattern();
            return;
        }
        completedHits = 0;
        animationFinished = false;

        bossAnimation.StartComboAnim();
    }

    public void HandleComboHit(int hitNumber)
    {
        if (!ownsExecution || patternRunner.CurrentState != BossPatternState.Active)
        return;

        if (hitNumber != completedHits + 1 || hitNumber > data.SlashCount)
        {
            Debug.LogWarning($"콤보 타격 이벤트 순서를 확인해 주세요. 받은 값 {hitNumber}", this);
            AbortPattern();
            return;
        }

        bool isFinisher = hitNumber == data.SlashCount;
        int damage = isFinisher ? data.FinisherDamage : data.SlashDamage;
        float pushImpulse = isFinisher ? data.FinisherPushImpulse : 0f;

        if (!CanExecute() || !slash.BeginSlash(comboDirection, damage, pushImpulse))
        {
            Debug.LogWarning("콤보 타격을 시작하지 못했습니다. 타격 간격을 확인해 주세요,", this);
            AbortPattern();
            return;
        }

        completedHits = hitNumber;
    }

    public void HandleComboAnimationFinished()
    {
        if (ownsExecution && patternRunner.CurrentState == BossPatternState.Active)
            animationFinished = true;
    }

    private void Update()
    {
        if (!ownsExecution || patternRunner.CurrentState != BossPatternState.Active)
            return;

        if (!CanExecute() || bossAnimation == null || !bossAnimation.CanPlayCombo)
        {
            AbortPattern();
            return;
        }

        bool reachedClipEnd = bossAnimation.HasComboClipEnded;

        if ((!animationFinished && !reachedClipEnd) || slash.IsAttacking)
            return;

        if (completedHits != data.SlashCount)
            Debug.LogWarning("콤보 클립에 1~4타 이벤트가 모두 있는지 확인해 주세요.", this);

        patternRunner.CompleteActive();
    }   
    private void HandleHealthChanged(int currentHealth, int maxHealth)
    {
        if(currentHealth <= 0 && ownsExecution)
            AbortPattern();
    }

    private void CancelExecution(bool completedNormally = false)
    {
        bool wasExecuting = ownsExecution;
        ownsExecution = false;

        completedHits = 0;
        animationFinished = false;

        if (wasExecuting && slash != null)
            slash.StopSlash();

        if (!wasExecuting || bossAnimation == null)
            return;
        
        if (completedNormally)
            bossAnimation.FinishComboAnim();
        else
            bossAnimation.CancelComboAnim();
    }

    private void AbortPattern()
    {
        bool shouldStopRunner = ownsExecution;
        CancelExecution();

        if(shouldStopRunner && patternRunner != null)
            patternRunner.StopPattern();
    }

    public void StartCombo()
    {
        TryStartAttack();
    }

    public override bool TryStartAttack()
    {
        if (!CanExecute() || !patternRunner.TrySelectPattern(this))
            return false;

        patternRunner.StartPatternWithWindupDuration(data.WindupDuration);

        return ownsExecution && patternRunner.CurrentState == BossPatternState.Windup;   
    }
}
