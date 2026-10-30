using UnityEngine;

public class MidBossFlamePillarPattern : MonoBehaviour
{
    [Header("실행 연결")]
    [SerializeField] private BossPatternRunner patternRunner;
    [SerializeField] private BossTargetFinder targetFinder;
    [SerializeField] private MidBossFlamePillar pillar;

    [Header("바닥 탐색")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField, Min(0.1f)] private float groundSearchDistance = 30f;

    private bool ownsExecution;

    private void OnEnable()
    {
        if (patternRunner == null || targetFinder == null || pillar == null)
        {
            Debug.LogError("실행기, 대상탐색기, 화염기둥을 연결해주세요.", this);
            enabled = false;
            return;
        }

        patternRunner.StateChanged += HandleStateChanged;
        pillar.Finished += HandleFinished;
    }

    private void OnDisable()
    {
        if(patternRunner != null)
            patternRunner.StateChanged -= HandleStateChanged;

        if(pillar != null)
            pillar.Finished -= HandleFinished;

        bool shouldStopRunner = ownsExecution;
        CancelExecution();

        if(shouldStopRunner && patternRunner != null)
            patternRunner.StopPattern();
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

                BeginWarning();
                break;

            case BossPatternState.Active:
                if(!ownsExecution)
                    return;

                if(!pillar.Activate())
                    AbortPattern("화염기둥 공격을 시작하지 못했습니다.");

                break;

            case BossPatternState.Recovery:
            case BossPatternState.Idle:
                CancelExecution();
                break;
        }
    }

    private void BeginWarning()
    {
        if(!pillar.isActiveAndEnabled)
        {
            AbortPattern("화염기둥 오브젝트와 컴포넌트를 켜 주세요.");
            return;
        }

        if(!targetFinder.TryGetTargetPosition(out Vector2 targetPosition))
        {
            AbortPattern("화염기둥의 대상을 찾지 못했습니다.");
            return;
        }

        RaycastHit2D hit = Physics2D.Raycast(targetPosition, Vector2.down, groundSearchDistance, groundLayer);

        if(hit.collider == null || !hit.collider.CompareTag("Ground"))
        {
            AbortPattern("대상 아래에서 바닥을 찾지 못했습니다.");
            return;
        }

        pillar.transform.position = new Vector3(hit.point.x, hit.point.y, 0f);

        if(!pillar.PrepareWarning(patternRunner.CurrentWindupDuration))
            AbortPattern("화염기둥 경고를 시작하지 못했습니다.");
    }

    private void HandleFinished()
    {
        if(!ownsExecution || !isActiveAndEnabled || patternRunner.CurrentState != BossPatternState.Active)
            return;

        patternRunner.CompleteActive();
    }

    private void AbortPattern(string message)
    {
        Debug.LogWarning(message, this);

        CancelExecution();
        patternRunner.StopPattern();
    }

    private void CancelExecution()
    {
        if(!ownsExecution)
            return;

        ownsExecution = false;

        if(pillar != null)
            pillar.Stop();
    }
}
