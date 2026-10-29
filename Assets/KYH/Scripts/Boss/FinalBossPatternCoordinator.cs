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

    private bool awaitingApproach;
    private void Awake()
    {
        if(phaseController == null || patternRunner == null)
        {
            Debug.LogError("Phase Controller와 Pattern Runner를 모두 연결해 주세요", this);

            enabled = false;
        }
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
                if(comboPattern == null || !comboPattern.isActiveAndEnabled)
                    return;

                if(!patternRunner.TrySelectPattern(comboPattern))
                    return;

                patternRunner.StartPattern();
                break;

            case FinalBossApproach.ApproachResult.NeedsRanged:
                Debug.Log("원거리 패턴 전환 필요:" + "전환할 패턴은 다음 단계에서 연결합니다.", this);
                break;
            
            case FinalBossApproach.ApproachResult.Cancelled:
                Debug.Log("근접 공격 접근이 취소됐습니다.", this);
                break;
        }
    }

    [ContextMenu("테스트/접근 후 칼 권총 연계")]
    private void TestApproachAndCombo()
    {
        if(Application.isPlaying)
            TryStartMeleeApproach();
    }
}
