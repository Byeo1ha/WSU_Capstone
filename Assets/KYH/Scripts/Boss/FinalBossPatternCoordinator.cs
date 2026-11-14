using UnityEngine;
using System.Collections.Generic;

public class FinalBossPatternCoordinator : MonoBehaviour
{

    [Header("패턴 실행기")]
    [SerializeField] private BossPatternRunner patternRunner;

    [Header("근접 공격 접근")]
    [SerializeField] private FinalBossApproach approach;

    [Header("1페이즈 중거리 패턴")]
    
    [Header("거리별 공격 목록")]
    [SerializeField] private BossAttackPattern[] meleePatterns;
    [SerializeField] private BossAttackPattern[] midRangePatterns;
    [SerializeField] private BossAttackPattern[] rangePatterns;

    private BossAttackPattern pendingPattern;

    [Header("최종보스 자동 공격")]
    [UnityEngine.Serialization.FormerlySerializedAs("autoStartPhase1Patterns")]
    [SerializeField] private bool autoStartPatterns;
    [SerializeField, Range(0f, 1f)]
    private float immediateAttackChance = 0.5f; 

    [Header("보스 체력")]
    [SerializeField] private BossHealth bossHealth;

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

    private void Update()
    {
        if (!autoStartPatterns || bossHealth == null || bossHealth.IsDead)
            return;

        if (awaitingApproach || (approach != null && approach.IsApproaching))
            return;

        if (!patternRunner.IsStartAllowed ||
            patternRunner.CurrentState != BossPatternState.Idle)
                return;

        if (Time.time < nextAutoAttempTime)
            return;

        bool started = TryStartRandomPattern();

        if (!started)
            nextAutoAttempTime = Time.time + 0.25f;
    }

    private void OnEnable()
    {

        if(approach != null)
            approach.Finished += HandleApproachFinished;
        
        bossHealth.HealthChanged += HandleHealthChanged;
    }

    private void OnDisable()
    {

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

        bool started = isMidRange ? approach.BeginMidRangeApproach(((IBossApproachRange)selected).ApproachStartRange) : approach.BeginApproach();

        if (!started)
        {
            awaitingApproach = false;
            pendingPattern = null;
        }

        return started;
    }
    private void HandleApproachFinished(FinalBossApproach.ApproachResult result)
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

            bool ready = result == FinalBossApproach.ApproachResult.ReadyForMelee ||
                         result == FinalBossApproach.ApproachResult.ReadyForMidRange;

            if (ready && selected != null &&
                selected.isActiveAndEnabled && selected.TryStartAttack())
                return;

            nextAutoAttempTime = Time.time + 0.25f;
    }

    private bool TryStartRandomPattern()
    {
        candidates.Clear();
        registeredCandidates.Clear();

        AddCandidates(meleePatterns, AttackRange.Melee);
        AddCandidates(midRangePatterns, AttackRange.MidRange);
        AddCandidates(rangePatterns, AttackRange.Ranged);

        if (candidates.Count == 0)
            return false;

        var selected = candidates[Random.Range(0, candidates.Count)];

        if (selected.range == AttackRange.Ranged || Random.value < immediateAttackChance)
            return selected.pattern.TryStartAttack();

        return TryBeginApproach(selected.pattern, selected.range == AttackRange.MidRange);
    }

    private void AddCandidates(BossAttackPattern[] patterns, AttackRange range)
    {
        if (patterns == null)
            return;

        foreach (BossAttackPattern pattern in patterns)
        {
            if (pattern == null || !pattern.isActiveAndEnabled)
                continue;

            if (range == AttackRange.MidRange && !(pattern is IBossApproachRange))
                continue;

            if (registeredCandidates.Add(pattern))
                candidates.Add((pattern, range));
        }
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