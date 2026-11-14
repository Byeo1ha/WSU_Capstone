using UnityEngine;
using UnityEngine.InputSystem.Controls;

public class BossPhaseEventTest : MonoBehaviour
{
    [Header("테스트할 페이즈 관리")]
    [SerializeField] private FinalBossPhaseController phaseController;

    private void OnEnable()
    {
        phaseController.PhaseChanged += HandlePhaseChanged;
        phaseController.TransformationStarted += HandleTransformationStarted;
        phaseController.TransformationEnded += HandleTransformationEnded;
    }

    private void OnDisable()
    {
        phaseController.PhaseChanged -= HandlePhaseChanged;
        phaseController.TransformationStarted -= HandleTransformationStarted;
        phaseController.TransformationEnded -= HandleTransformationEnded;
    }

    private void HandlePhaseChanged(FinalBossPhase phase)
    {
        Debug.Log($"이벤트 수신: {phase}", this);
    }
    
    private void HandleTransformationStarted()
    {
        Debug.Log("이벤스 수신: 변신 시작", this);
    }

    private void HandleTransformationEnded()
    {
        Debug.Log("이벤트 수신: 변신 종료", this);
    }
}
