using UnityEngine;
using System;

public class BossPatternRunner : MonoBehaviour
{
    private bool canStartPattern = true;

    [Header("단계 타이머")]
    [SerializeField] private float stateTimeRemaining;
    [Header("패턴 데이터")]
    [SerializeField] private BossPatternData patternData;
    [Header("현재 패턴 단계")]
    [SerializeField] private BossPatternState currentState = BossPatternState.Idle;

    public BossPatternState CurrentState => currentState;

    public event Action<BossPatternState> StateChanged;

    private void ChangeState(BossPatternState nextState)
    {
        if(currentState == nextState)
            return;

        currentState = nextState;

        StateChanged?.Invoke(currentState);

        Debug.Log($"패턴 단계 변경: {currentState}", this);
    }

    private void Awake()
    {
        currentState = BossPatternState.Idle;
        stateTimeRemaining = 0f;
    }

    private void OnDisable()
    {
        StopPattern();
    }

    private void Update()
    {
        if(currentState == BossPatternState.Idle ||
           currentState == BossPatternState.Active)
            return;

        stateTimeRemaining = Mathf.Max(0f, stateTimeRemaining - Time.deltaTime);

        if(stateTimeRemaining > 0f)
            return;

        switch (currentState)
        {
            case BossPatternState.Windup:
                stateTimeRemaining = 0f;
                ChangeState(BossPatternState.Active);
                break;
            
            case BossPatternState.Recovery:
                ChangeState(BossPatternState.Idle);
                break;
        }
    }

    public void StartPattern()
    {
        if(!isActiveAndEnabled)
            return;
        
        if (!canStartPattern)
            return;

        if(currentState != BossPatternState.Idle)
            return;

        if(patternData == null)
        {
            Debug.LogError("Pattern Data를 연결해 주세요.", this);
            return;
        }

        stateTimeRemaining = patternData.WindupDuration;
        ChangeState(BossPatternState.Windup);
    }

    public void CompleteActive()
    {
        if(!isActiveAndEnabled)
            return;

        if(currentState != BossPatternState.Active)
            return;

        stateTimeRemaining = patternData.RecoveryDuration;
        ChangeState(BossPatternState.Recovery);
    }

    public void StopPattern()
    {
        stateTimeRemaining = 0f;
        ChangeState(BossPatternState.Idle);
    }

    [ContextMenu("테스트/ 패턴 중단")]
    private void TestStopPattern()
    {
        if(!Application.isPlaying)
            return;

        StopPattern();
    }

    [ContextMenu("테스트/패턴 시작")]
    private void TestStartPattern()
    {
        if (!Application.isPlaying)
            return;

        StartPattern();
    }

    public void SetPatternStartAllowed(bool value)
    {
        canStartPattern = value;
    }
}
