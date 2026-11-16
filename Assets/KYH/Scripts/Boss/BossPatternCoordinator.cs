using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class BossPatternCoordinator : MonoBehaviour
{

    [Header("패턴 실행기")]
    [SerializeField] private BossPatternRunner patternRunner;

    [Header("근접 공격 접근")]
    [SerializeField] private BossApproach approach;

    [Header("1페이즈 중거리 패턴")]
    
    [Header("거리별 공격 목록")]
    [SerializeField] private BossAttackPattern[] meleePatterns;
    [SerializeField] private BossAttackPattern[] midRangePatterns;
    [SerializeField] private BossAttackPattern[] rangePatterns;

    [Header("조건부 우선 패턴")]
    [SerializeField] private List<BossPriorityPattern> priorityPatterns = new();

    private BossAttackPattern pendingPattern;
    private BossPriorityPattern runningPriorityPattern;

    [Header("최종보스 자동 공격")]
    [UnityEngine.Serialization.FormerlySerializedAs("autoStartPhase1Patterns")]
    [SerializeField] private bool autoStartPatterns;
    [SerializeField, Range(0f, 1f)]
    private float immediateAttackChance = 0.5f; 
    [SerializeField, Min(0f)] private float initialActionDelay = 2f;
    [SerializeField] private bool excludeLastPattern;

    private BossAttackPattern lastPattern;

    [Header("보스 체력")]
    [SerializeField] private BossHealth bossHealth;

    [Header("보스 방향")]
    [SerializeField] private BossTargetFinder facingTargetFinder;
    [SerializeField] private SpriteRenderer bossRenderer;

    private float nextAutoAttempTime;
    private bool awaitingApproach;

    private enum AttackRange
    {
        Melee,
        MidRange,
        Ranged
    }

    private readonly List<(BossAttackPattern pattern, AttackRange range)>
        candidates = new();

    private readonly HashSet<BossAttackPattern> registeredCandidates = new();
    private void Awake()
    {
        if(patternRunner == null || bossHealth == null)
        {
            Debug.LogError("패턴 실행기와 보스 체력을 연결해 주세요", this);

            enabled = false;
        }
    }

    private void Start()
    {
        nextAutoAttempTime = Time.time + initialActionDelay;
    }

    private void Update()
    {
        if (runningPriorityPattern != null)
        {
            if (!patternRunner.IsStartAllowed || bossHealth == null || bossHealth.IsDead)
            {
                CancelRunningPriorityPattern();
                return;
            }

            if (runningPriorityPattern.IsRunning)
                return;

            runningPriorityPattern = null;
        }

        if (!autoStartPatterns || bossHealth == null || bossHealth.IsDead)
            return;

        if (awaitingApproach || (approach != null && approach.IsApproaching))
            return;

        if (!patternRunner.IsStartAllowed || patternRunner.CurrentState != BossPatternState.Idle)
            return;

        if (Time.time < nextAutoAttempTime)
            return;

        BossPriorityPattern priority = FindPendingPriorityPattern();

        if (priority != null && priority.TryStartPriority())
        {
            runningPriorityPattern = priority;
            return;
        }

        if (!TryStartRandomPattern())
            nextAutoAttempTime = Time.time + 0.25f; 
    }


    private void CancelRunningPriorityPattern()
    {
        BossPriorityPattern running = runningPriorityPattern;
        runningPriorityPattern = null;

        if (running != null)
            running.CancelPriority();
    }

    private void LateUpdate()
    {
        if (bossHealth == null || bossHealth.IsDead || patternRunner == null ||
            facingTargetFinder == null || bossRenderer == null)
            return;

        BossPatternState state = patternRunner.CurrentState;
        if (state == BossPatternState.Windup || state == BossPatternState.Active)
            return;

        if (!facingTargetFinder.TryGetTargetPosition(out Vector2 targetPosition))
            return;

        float horizontalDifference = targetPosition.x - bossRenderer.transform.position.x;

        if (Mathf.Abs(horizontalDifference) > 0.01f)
            bossRenderer.flipX = horizontalDifference < 0f;
    }

    private void OnEnable()
    {

        if(approach != null)
            approach.Finished += HandleApproachFinished;
        
        bossHealth.HealthChanged += HandleHealthChanged;
    }

    private void OnDisable()
    {
        CancelRunningPriorityPattern();

        if(approach != null)
            approach.Finished -= HandleApproachFinished;

        if (bossHealth != null)
            bossHealth.HealthChanged -= HandleHealthChanged;

        awaitingApproach = false;

        if(approach != null)
            approach.CancelApproach();
    }

    private void HandleHealthChanged(int currentHealth, int maxHealth)
    {
        if (currentHealth > 0)
            return;
        
        CancelRunningPriorityPattern();

        awaitingApproach = false;
        patternRunner.SetPatternStartAllowed(false);

        if (approach != null)
            approach.CancelApproach();
        
        patternRunner.StopPattern();
    }

    public bool TryStartMeleeApproach()
    {
        return TryBeginApproach(meleePatterns, false);
    }

    public bool TryStartRangedPattern()
    {
        return TryBeginApproach(midRangePatterns, true);
    }

    private bool TryBeginApproach(BossAttackPattern[] patterns, bool isMidRange)
    {
        if (!isActiveAndEnabled || bossHealth.IsDead || awaitingApproach || approach == null ||
            approach.IsApproaching || !patternRunner.IsStartAllowed || patternRunner.CurrentState != BossPatternState.Idle ||
            patterns == null)
            return false;

        BossAttackPattern selected = null;
        int availableCount = 0;

        foreach (BossAttackPattern pattern in patterns)
        {
            if (pattern == null || !pattern.isActiveAndEnabled)
                continue;

            if (isMidRange && !(pattern is IBossApproachRange))
                continue;

            availableCount++;

            if (Random.Range(0, availableCount) == 0)
                selected = pattern;
        }    
        return TryBeginApproach(selected, isMidRange);
    }

    private bool TryBeginApproach(BossAttackPattern selected, bool isMidRange)
    {
        if (selected == null || approach == null || approach.IsApproaching)
            return false;

        pendingPattern = selected;
        awaitingApproach = true;

        float startDistance = approach.MeleeStartDistance;

        if (isMidRange)
            startDistance = ((IBossApproachRange)selected). ApproachStartRange.x;

        bool started = approach.BeginApproach(startDistance);

        if (!started)
        {
            awaitingApproach = false;
            pendingPattern = null;
        }

        return started;
    }
    private void HandleApproachFinished(BossApproach.ApproachResult result)
    {
        if(!awaitingApproach)
            return;

        BossAttackPattern selected = pendingPattern;
        awaitingApproach =false;
        pendingPattern = null;

        if (!isActiveAndEnabled || bossHealth.IsDead ||
            !patternRunner.IsStartAllowed ||
            patternRunner.CurrentState != BossPatternState.Idle)
            return;

            bool ready = result == BossApproach.ApproachResult.ReachedRange ||
                         result == BossApproach.ApproachResult.TimedOut;

            if (ready && selected != null &&
                selected.isActiveAndEnabled && TryStartAndRemember(selected))
                return;

            nextAutoAttempTime = Time.time + 0.25f;
    }

    private bool TryStartRandomPattern()
    {
        CollectCandidates(excludeLastPattern);

        if (candidates.Count == 0 && excludeLastPattern)
            CollectCandidates(false);

        if (candidates.Count == 0)
            return false;

        var selected = candidates[Random.Range(0, candidates.Count)];

        if (approach == null || selected.range == AttackRange.Ranged ||
            Random.value < immediateAttackChance)
            return TryStartAndRemember(selected.pattern);

        return TryBeginApproach(selected.pattern, selected.range == AttackRange.MidRange);

    }

    private BossPriorityPattern FindPendingPriorityPattern()
    {
        foreach (BossPriorityPattern pattern in priorityPatterns)
        {
            if (pattern == null || !pattern.isActiveAndEnabled)
                continue;
            
            if (pattern.IsPending)
                return pattern;
        }

        return null;
    }

    private void CollectCandidates(bool excludeLast)
    {
        candidates.Clear();
        registeredCandidates.Clear();

        AddCandidates(meleePatterns, AttackRange.Melee, excludeLast);
        AddCandidates(midRangePatterns, AttackRange.MidRange, excludeLast);
        AddCandidates(rangePatterns, AttackRange.Ranged, excludeLast);
    }

    private void AddCandidates(BossAttackPattern[] patterns, AttackRange range, bool excludeLast)
    {
        if (patterns == null)
            return;

        foreach (BossAttackPattern pattern in patterns)
        {
            if (pattern == null || !pattern.isActiveAndEnabled)
                continue;

            if (excludeLast && pattern == lastPattern)
                continue;

            if (approach != null && range == AttackRange.MidRange && !(pattern is IBossApproachRange))
                continue;

            if (registeredCandidates.Add(pattern))
                candidates.Add((pattern, range));
        }
    }

    private bool TryStartAndRemember(BossAttackPattern pattern)
    {
        if (!pattern.TryStartAttack())
            return false;

        lastPattern = pattern;
        return true;
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