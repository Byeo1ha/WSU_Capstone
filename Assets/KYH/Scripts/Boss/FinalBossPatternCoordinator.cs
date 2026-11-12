using UnityEngine;

public class FinalBossPatternCoordinator : MonoBehaviour
{
    [Header("최종보스 페이즈 관리")]
    [SerializeField] private FinalBossPhaseController phaseController;

    [Header("패턴 실행기")]
    [SerializeField] private BossPatternRunner patternRunner;

    [Header("근접 공격 접근")]
    [SerializeField] private FinalBossApproach approach;
    [SerializeField] private FinalBossComboPattern comboPattern;

    [Header("1페이즈 중거리 패턴")]
    [UnityEngine.Serialization.FormerlySerializedAs("rangedDashPattern")]
    [SerializeField] private FinalBossDashPattern midRangeDashPattern;
    [SerializeField, Range(0f, 1f)] private float meleeSelectionChance = 0.7f;

    [Header("1페이즈 자동 공격")]
    [SerializeField] private bool autoStartPhase1Patterns;
    [SerializeField] private BossHealth bossHealth;

    private float nextAutoAttempTime;
    private bool awaitingApproach;
    private void Awake()
    {
        if(phaseController == null || patternRunner == null)
        {
            Debug.LogError("Phase Controller와 Pattern Runner를 모두 연결해 주세요", this);

            enabled = false;
        }
    }

    private void Update()
    {
        if (!autoStartPhase1Patterns || bossHealth == null || bossHealth.IsDead)
            return;

        if (awaitingApproach || (approach != null && approach.IsApproaching))
            return;

        if (!patternRunner.IsStartAllowed ||
            patternRunner.CurrentState != BossPatternState.Idle ||
            !phaseController.isActiveAndEnabled ||
            phaseController.IsTransforming ||
            phaseController.CurrentPhase != FinalBossPhase.Phase1)
                return;

        if (Time.time < nextAutoAttempTime)
            return;

        bool started = Random.value < 0.4f ? TryStartMeleeApproach() : TryStartRangedPattern();

        if (!started)
            nextAutoAttempTime = Time.time + 0.25f;
    }

    private void OnEnable()
    {
        phaseController.TransformationStarted += HandleTransformationStarted;
        phaseController.TransformationEnded += HanldeTransformationEnded;

        if(approach != null)
            approach.Finished += HandleApproachFinished;

        if(phaseController.IsTransforming)
            HandleTransformationStarted();
        else
            HanldeTransformationEnded();
    }

    private void OnDisable()
    {
        if(phaseController != null)
        {
        phaseController.TransformationStarted -= HandleTransformationStarted;
        phaseController.TransformationEnded -= HanldeTransformationEnded;
        }

        if(approach != null)
            approach.Finished -= HandleApproachFinished;

        awaitingApproach = false;

        if(approach != null)
            approach.CancelApproach();
    }

    private void HandleTransformationStarted()
    {
        patternRunner.SetPatternStartAllowed(false);

        awaitingApproach = false;

        if(approach != null)
            approach.CancelApproach();
        patternRunner.StopPattern();
    }

    private void HanldeTransformationEnded()
    {
        patternRunner.SetPatternStartAllowed(true);
    }

    public bool TryStartMeleeApproach()
    {
        if(!isActiveAndEnabled || awaitingApproach)
            return false;
        if(approach == null || comboPattern == null)
        {
            Debug.LogError("접근 컴포넌트와 칼 연계 패턴을 연결해 주세요.", this);

            return false;
        }

        if(!patternRunner.IsStartAllowed ||
           patternRunner.CurrentState != BossPatternState.Idle ||
           phaseController.IsTransforming ||
           phaseController.CurrentPhase != FinalBossPhase.Phase1 ||
           !comboPattern.isActiveAndEnabled ||
           approach.IsApproaching)
            return false;

        awaitingApproach = true;

        if (!approach.BeginApproach())
        {
            awaitingApproach = false;
            return false;
        }

        return true;
    }

    private void HandleApproachFinished(FinalBossApproach.ApproachResult result)
    {
        if(!awaitingApproach)
            return;

        awaitingApproach =false;

        if (!isActiveAndEnabled ||
            !patternRunner.IsStartAllowed ||
            patternRunner.CurrentState != BossPatternState.Idle ||
            phaseController.IsTransforming ||
            phaseController.CurrentPhase != FinalBossPhase.Phase1)
            return;

        switch(result)
        {
            case FinalBossApproach.ApproachResult.ReadyForMelee:
                if(comboPattern != null && comboPattern.isActiveAndEnabled)
                    comboPattern.StartCombo();
                break;

            case FinalBossApproach.ApproachResult.ReadyForMidRange:
                if (midRangeDashPattern == null || !midRangeDashPattern.TryStartDash())
                    nextAutoAttempTime = Time.time + 0.25f;
                break;

            case FinalBossApproach.ApproachResult.NeedsRanged:
                nextAutoAttempTime = Time.time + 0.25f;
                break;

            case FinalBossApproach.ApproachResult.Cancelled:
                Debug.Log("근접 공격 접근이 취소됐습니다.", this);
                break;
        }
    }

    public bool TryStartRangedPattern()
    {
        if (!isActiveAndEnabled || awaitingApproach || approach == null || approach.IsApproaching ||
            midRangeDashPattern == null || !midRangeDashPattern.isActiveAndEnabled || !patternRunner.IsStartAllowed ||
            patternRunner.CurrentState != BossPatternState.Idle || phaseController.IsTransforming || phaseController.CurrentPhase != FinalBossPhase.Phase1)
            return false;

        awaitingApproach = approach.BeginMidRangeApproach(midRangeDashPattern.ApproachStartRange);

        return awaitingApproach;
    }

    [ContextMenu("테스트/제자리에서 원거리 패턴 시작")]
    private void TestRangedPattern()
    {
        if (Application.isPlaying)
            TryStartRangedPattern();
    }

    [ContextMenu("테스트/접근 후 칼 권총 연계")]
    private void TestApproachAndCombo()
    {
        if(Application.isPlaying)
            TryStartMeleeApproach();
    }
}
