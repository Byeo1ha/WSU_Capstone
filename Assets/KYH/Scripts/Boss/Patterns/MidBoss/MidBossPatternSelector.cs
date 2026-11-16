using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MidBossPatternSelector : MonoBehaviour
{
    [Header("패턴 실행기")]
    [SerializeField] private BossPatternRunner patternRunner;

    [Header("거리별 공격 목록")]
    [SerializeField] private BossAttackPattern[] meleePatterns;
    [SerializeField] private BossAttackPattern[] midRangePatterns;

    [UnityEngine.Serialization.FormerlySerializedAs("patterns")]
    [SerializeField] private BossAttackPattern[] rangedPatterns;
    [Header("블링크 직후 고정 패턴")]
    [SerializeField] private MidBossFireballPattern fireballPattern;

    [Header("첫 패턴 시작 대기")]
    [SerializeField, Min(0f)] private float firstPatternDelay = 1.5f;
    [Header("예약된 블링크")]
    [SerializeField] private MidBossBlinkController blinkController;
    [Header("블링크 직후 준비시간 배율")]
    [SerializeField, Range(0.01f, 1f)]
    private float windupScaleAfterBlink = 0.3f;

    public bool CurrentPatternFollowsBlink {get; private set;}

    private readonly List<BossAttackPattern> candidates = new List<BossAttackPattern>();

    private float timeRemaining;
    private bool waitingForBlink;
    private BossAttackPattern lastPattern;

    private void OnEnable()
    {
        if (!ValidateReferences())
        {
            enabled = false;
            return;
        }

        lastPattern = null;
        waitingForBlink = false;
        CurrentPatternFollowsBlink = false;
        timeRemaining = Mathf.Max(0f, firstPatternDelay);
    }

    private void OnDisable()
    {
        if (waitingForBlink && blinkController != null)
            blinkController.CancelBlink();

        waitingForBlink = false;
    }

    private bool ValidateReferences()
    {
        if(fireballPattern == null)
        {
            Debug.LogError("블링크 직후 사용할 파이어볼 패턴을 연결해 주세요.", this);
            return false;
        }
        if(blinkController == null)
        {
            Debug.LogError("블링크 관리기를 연결해 주세요.", this);
            return false;
        }
        if(patternRunner == null )
        {
            Debug.LogError("패턴 실행기를 연결해 주세요.", this);
            return false;
        }

        if (patternRunner == null)
        {
            Debug.LogError("패턴 실행기를 연결해 주세요.", this);
            return false;
        }
        
       if (!HasAssignedPattern(meleePatterns) && !HasAssignedPattern(midRangePatterns) &&
            !HasAssignedPattern(rangedPatterns))
        {
            Debug.LogError("중간보스 패턴을 하나 이상 연결해 주세요.", this);
            return false;
        }

        return true;
    }

    private bool HasAssignedPattern(BossAttackPattern[] patterns)
    {
        if (patterns == null)
            return false;

        foreach (BossAttackPattern pattern in patterns)
        {
            if (pattern != null)
                return true;
        }
        
        return false;
    }

    private void CollectCandidates(bool excludeLast)
    {
        candidates.Clear();

        AddCandidates(meleePatterns, excludeLast);
        AddCandidates(midRangePatterns, excludeLast);
        AddCandidates(rangedPatterns, excludeLast);
    }

    private void AddCandidates(BossAttackPattern[] patterns, bool excludeLast)
    {
        if (patterns == null)
            return;

        foreach (BossAttackPattern pattern in patterns)
        {
            if (pattern == null || !pattern.isActiveAndEnabled ||
                (excludeLast && pattern == lastPattern) ||
                candidates.Contains(pattern))
                continue;

            candidates.Add(pattern); 
        }
    }

    private void Update()
{
    if (patternRunner == null ||
        !patternRunner.IsStartAllowed ||
        patternRunner.CurrentState != BossPatternState.Idle)
    {
        if (waitingForBlink)
        {
            blinkController.CancelBlink();
            waitingForBlink = false;
        }

        return;
    }

    if (waitingForBlink)
    {
        if (blinkController.IsBlinking)
            return;

        waitingForBlink = false;

        if (!blinkController.BlinkSucceeded)
        {
            Debug.LogWarning("블링크가 중단되거나 이동에 실패했습니다. 연결을 확인해 주세요.", this);
            enabled = false;
            return;
        }

        StartNextPattern(true);
        return;
    }

    timeRemaining = Mathf.Max(
        0f, timeRemaining - Time.deltaTime);

    if (timeRemaining > 0f)
        return;

    StartNextPattern();
}

    private void StartNextPattern(bool followsBlink = false)
{
    if (!followsBlink && blinkController.PendingBlinkCount > 0)
    {
        if (!fireballPattern.isActiveAndEnabled)
        {
            Debug.LogWarning(
                "블링크 후 사용할 파이어볼 패턴을 활성화해 주세요.",
                this);
            enabled = false;
            return;
        }

        if (!blinkController.TryBeginBlink())
        {
            Debug.LogWarning(
                "블링크 모션을 시작하지 못했습니다. 연결을 확인해 주세요.",
                this);
            enabled = false;
            return;
        }

        waitingForBlink = true;
        return;
    }

    BossAttackPattern selected;

    if (followsBlink)
    {
        selected = fireballPattern;
    }
    else
    {
        CollectCandidates(excludeLast: true);

        if (candidates.Count == 0)
            CollectCandidates(excludeLast: false);

        selected = candidates[Random.Range(0, candidates.Count)];
    }

    CurrentPatternFollowsBlink = followsBlink;

    bool started = followsBlink ? fireballPattern.TryStartAfterBlink(windupScaleAfterBlink) : selected.TryStartAttack();

    if (!started)
        {
            CurrentPatternFollowsBlink = false;
            Debug.LogWarning("패턴 시작에 실패했습니다. 연결과 설정을 확인해 주세요.", this);
            enabled = false;
            return;
        }

    if (patternRunner.CurrentState != BossPatternState.Windup)
    {
        Debug.LogWarning("패턴 시작에 실패했습니다. 연결과 설정을 확인해 주세요.", this);
        enabled = false;
        return;
    }

    lastPattern = selected;

    Debug.Log($"중간보스 패턴 선택: {selected.GetType().Name}", this);
}
}
