using DG.Tweening;
using UnityEngine;

public class FinalBossDashPattern : MonoBehaviour
{
    [Header("실행 연결")]
    [SerializeField] private BossPatternRunner patternRunner;
    [SerializeField] private BossHealth bossHealth;
    [SerializeField] private FinalBossPhaseController phaseController;
    [SerializeField] private BossTargetFinder targetFinder;
    [SerializeField] private Rigidbody2D bossBody;

    [Header("돌진 이동 한계")]
    [SerializeField] private Transform leftDashLimit;
    [SerializeField] private Transform rightDashLimit; 

    [Header("돌진 이동 - 임시값")]
    [SerializeField, Min(0.1f)] private float dashDistance = 4f;
    [SerializeField, Min(0.05f)] private float dashDuration = 0.25f;

    private bool ownsExecution;
    private float destinationX;
    private Tween dashTween;

    private void Awake()
    {
        if(patternRunner != null && bossHealth != null && phaseController != null && bossBody != null)
            return;

        Debug.LogError("돌진 발도에 필요한 참조를 모두 연결해 주세요", this);
        enabled = false;
    } 

    private void OnEnable()
    {
        patternRunner.StateChanged += HandleStateChanged;
    }

    private void OnDisable()
    {
        if (patternRunner != null)
            patternRunner.StateChanged -= HandleStateChanged;

        if (ownsExecution)
            AbortPattern();
    }

    private bool CanExecute()
    {
        return isActiveAndEnabled &&
               patternRunner.IsStartAllowed &&
               !bossHealth.IsDead &&
               phaseController.isActiveAndEnabled &&
               !phaseController.IsTransforming &&
               phaseController.CurrentPhase == FinalBossPhase.Phase1 &&
               bossBody.simulated &&
               bossBody.bodyType != RigidbodyType2D.Static;
    }

    private void HandleStateChanged(BossPatternState state)
    {
        if (state == BossPatternState.Windup && patternRunner.IsSelectedPattern(this))
        {
            ownsExecution = true;

            if (!CanExecute() || !targetFinder.isActiveAndEnabled || !targetFinder.TryGetTargetPosition(out Vector2 targetPosition))
            {
                AbortPattern();
                return;
            }

            if (leftDashLimit == null || rightDashLimit == null ||
                leftDashLimit.position.x >= rightDashLimit.position.x)
            {
                Debug.LogError("돌진 한계를 왼쪽과 오른쪽 순서로 연결해 주세요.", this);
                AbortPattern();
                return;
            }

            float startX = bossBody.position.x;
            int direction = targetPosition.x < startX ? -1 : 1;
            float limitX = direction < 0 ? leftDashLimit.position.x : rightDashLimit.position.x;

            float availableDistance = Mathf.Max(0f, (limitX - startX) * direction);
            float travelDistance = Mathf.Min(dashDistance, availableDistance);

            destinationX = startX + direction * travelDistance;
            return;
        }

        if (!ownsExecution)
            return;

        if (state == BossPatternState.Active)
            BeginDash();
        else if (state == BossPatternState.Recovery || state == BossPatternState.Idle)
            CancelExecution();
    }

    private void BeginDash()
    {
        if (!CanExecute())
        {
            AbortPattern();
            return;
        }

        dashTween = bossBody.DOMoveX(destinationX, dashDuration)
            .SetEase(Ease.Linear)
            .SetUpdate(UpdateType.Fixed)
            .OnComplete(FinishDash);
    }

    private void Update()
    {
        if (ownsExecution && !CanExecute())
            AbortPattern();
    }

    private void FinishDash()
    {
        dashTween = null;

        if (ownsExecution && patternRunner.CurrentState == BossPatternState.Active)
            patternRunner.CompleteActive();
    }

    private void CancelExecution()
    {
        ownsExecution = false;
        dashTween?.Kill(false);
        dashTween = null;
    }

    private void AbortPattern()
    {
        CancelExecution();
        patternRunner.StopPattern();
    }

    [ContextMenu("테스트/돌진 발도 이동")]
    private void TestDash()
    {
        if (Application.isPlaying && CanExecute() &&
            patternRunner.TrySelectPattern(this))
            patternRunner.StartPattern();
    }
}
