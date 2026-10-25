using UnityEngine;

public class MidBossTotemPattern : MonoBehaviour
{
    [Header("패턴 실행기")]
    [SerializeField] private BossPatternRunner patternRunner;

    [Header("재사용할 토템")]
    [SerializeField] private MidBossTotem totem;

    [Header("테스트 배치 위기")]
    [SerializeField] private Transform spawnPoint;

    [Header("토템 발사기")]
    [SerializeField] private MidBossTotemShooter shooter;

    private bool hasTotemEnded;

    private bool ownsExecution;

    private void OnEnable()
    {
        if(patternRunner == null || totem == null || spawnPoint == null || shooter == null)
        {
            Debug.LogError("실행기,. 토템, 테스트 배치 위치를 연결해 주세요.", this);

            enabled = false;
            return;
        }

        patternRunner.StateChanged += HandleStateChanged;
        totem.Ended += HandleTotemEnded;
    }

    private void OnDisable()
    {
        if(patternRunner != null)
            patternRunner.StateChanged -= HandleStateChanged;

        if(totem != null)
            totem.Ended -= HandleTotemEnded;

        bool shouldStopRunner = ownsExecution;
        CancelExecution();

        if(shouldStopRunner && patternRunner != null)
            patternRunner.StopPattern();
    }

    private void HandleStateChanged(BossPatternState state)
    {
        switch (state)
        {
            case BossPatternState.Windup:
                if(!patternRunner.IsSelectedPattern(this))
                    return;

                CancelExecution();
                ownsExecution = true;
                hasTotemEnded = false;
                break;

            case BossPatternState.Active:
                if(ownsExecution)
                    ActivateTotem();
                break;

            case BossPatternState.Recovery:
            case BossPatternState.Idle:
                CancelExecution();
                break;
        }
    }

    private void ActivateTotem()
    {
        if(totem == null || spawnPoint == null || !totem.enabled || !totem.gameObject || totem.gameObject.activeSelf)
        {
            Debug.LogWarning("꺼진 상태의 토템과 배치 위치를 확인해 주세요.", this);

            AbortPattern();
            return;
        }

        totem.transform.position = spawnPoint.position;
        totem.gameObject.SetActive(true);
    }

    private void HandleTotemEnded()
    {
        if (!ownsExecution)
            return;

        hasTotemEnded = true;
        TryCompleteActive();
    }

    private void TryCompleteActive()
    {
        if(!ownsExecution || !hasTotemEnded || shooter == null || shooter.ActiveFireballCount > 0 || patternRunner.CurrentState != BossPatternState.Active)
            return;

        patternRunner.CompleteActive();
    }

    private void Update()
    {
        if(!ownsExecution || patternRunner == null || patternRunner.CurrentState != BossPatternState.Active)
            return;

        if(totem == null || shooter == null)
        {
            AbortPattern();
            return;
        }

        if(!hasTotemEnded && !totem.isActiveAndEnabled)
        {
            AbortPattern();
            return;
        }

        TryCompleteActive();
    }

    private void CancelExecution()
    {
        if(!ownsExecution)
            return;

        ownsExecution = false;
        hasTotemEnded = false;

        if(totem != null)
        {
            totem.EndTotem();

            if(totem.gameObject.activeSelf)
                totem.gameObject.SetActive(false);
        }

        if(shooter != null)
            shooter.CancelAndRetrunAll();
    }

    private void AbortPattern()
    {
        CancelExecution();

        if(patternRunner != null)
            patternRunner.StopPattern();
    }
}
