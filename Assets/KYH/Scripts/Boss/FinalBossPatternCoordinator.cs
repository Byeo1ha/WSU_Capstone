using UnityEngine;

public class FinalBossPatternCoordinator : MonoBehaviour
{
    [Header("최종보스 페이즈 관리")]
    [SerializeField] private FinalBossPhaseController phaseController;

    [Header("패턴 실행기")]
    [SerializeField] private BossPatternRunner patternRunner;

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

        if(phaseController.IsTransforming)
            HandleTransformationStarted();
        else
            HanldeTransformationEnded();
    }

    private void OnDisable()
    {
        if(phaseController == null)
            return;
        phaseController.TransformationStarted -= HandleTransformationStarted;
        phaseController.TransformationEnded -= HanldeTransformationEnded;
    }

    private void HandleTransformationStarted()
    {
        patternRunner.SetPatternStartAllowed(false);
        patternRunner.StopPattern();
    }

    private void HanldeTransformationEnded()
    {
        patternRunner.SetPatternStartAllowed(true);
    }
}
