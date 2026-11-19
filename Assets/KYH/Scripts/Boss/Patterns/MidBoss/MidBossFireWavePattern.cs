using UnityEngine;
using System.Collections.Generic;
using NUnit.Framework;


public class MidBossFireWavePattern : BossAttackPattern
{
    [Header("실행 연결")]
    [SerializeField] private BossPatternRunner patternRunner;
    [SerializeField] private BossHealth bossHealth;
    [SerializeField] private BossTargetFinder targetFinder;
    [SerializeField] private MidBossVisual bossBossVisual;

    [Header("불꽃 파동")]
    [SerializeField] private MidBossFireWaveData data;
    [SerializeField] private MidBossFireWavePool pillarPool;
    [SerializeField] private MidBossBlinkArea waveArea;


    private bool ownsExecution;
    private int waveDirection;
    private float waveMinX;
    private float waveMaxX;

    private readonly HashSet<MidBossFireWavePillar> activePillars = new();
    private MidBossFireWavePillar leadingPillar;
    private bool hasReachedEnd;
    private void OnEnable()
    {
        if (patternRunner == null || bossHealth == null || targetFinder == null ||
            bossBossVisual == null || data == null || pillarPool == null || waveArea == null)
        {
            Debug.LogError("불꽃파동의 실행 참조와 데이터를 연결해 주세요.", this);
            enabled = false;
            return;
        }

        patternRunner.StateChanged += HandleStateChanged;
        pillarPool.PillarReturned += HandlePillarReturned;
        pillarPool.EffectStarted += HandleEffectStarted;
    }

    private void OnDisable()
    {
        if (patternRunner != null)
            patternRunner.StateChanged -= HandleStateChanged;

        if(pillarPool != null)
        {
            pillarPool.PillarReturned -= HandlePillarReturned;
            pillarPool.EffectStarted -= HandleEffectStarted;
        }
        AbortPattern();
    }

    private void Update()
    {
        if (ownsExecution && bossHealth.IsDead)
            AbortPattern();
    }

    private void HandleStateChanged(BossPatternState state)
    {
        if (state == BossPatternState.Windup && patternRunner.IsSelectedPattern(this))
        {
            ownsExecution = true;

            if (!targetFinder.TryGetTargetPosition(out Vector2 targetPosition))
            {
                AbortPattern();
                return;
            }

            waveDirection = targetPosition.x < transform.position.x ? -1 : 1;
            if (!waveArea.TryGetBounds(out waveMinX, out waveMaxX))
            {
                Debug.LogWarning("불꽃파동에 사용할 좌우 범위를 확인해 주세요.", this);
                AbortPattern();
                return;
            }
            bossBossVisual.StartFireWaveAnim();
            return;
        }

        if (!ownsExecution)
            return;

        if (state == BossPatternState.Active)
        {
            SpawnFirstPillar();
        }
        else if (state == BossPatternState.Recovery)
        {
            StopExecution(completedNormally: true);
        }
        else if (state == BossPatternState.Idle)
        {
            StopExecution();
        }
    }

    public void OnFireWaveCastFrame()
    {
        if (!isActiveAndEnabled || !ownsExecution || bossHealth.IsDead)
            return;

        patternRunner.EnterActiveFromEvent(this);
    }


    private void SpawnFirstPillar()
    {
        if (waveMaxX - waveMinX < data.PillarWidth)
        {
            Debug.LogWarning("불꽃 파동 범위가 기둥 하나으 폭보다 좁습니다.", this);
            AbortPattern();
            return;
        }

        hasReachedEnd = false;

        Vector3 position = transform.position;
        position.x += waveDirection * data.StartOffsetX;
        position.y += data.SpawnOffsetY;

        SpawnPillar(position);

    }

    private void SpawnPillar(Vector3 position)
    {
        float halfWidth = data.PillarWidth * 0.5f;
        float minCenterX = waveMinX + halfWidth;
        float maxCenterX = waveMaxX - halfWidth;

        position.x = Mathf.Clamp(position.x, minCenterX, maxCenterX);

        hasReachedEnd = waveDirection > 0 ? position.x >= maxCenterX - 0.001f
            : position.x <= minCenterX + 0.001f;

        MidBossFireWavePillar pillar = pillarPool.Rent(position);

        if (pillar == null)
        {
            AbortPattern();
            return;
        }

        activePillars.Add(pillar);
        leadingPillar = pillar;

        pillar.SetWidth(data.PillarWidth);
        pillar.ConfigureDamage(data.DamageSize, data.DamageOffset);
        pillar.BeginWarning(data.WarningDuration);
    }

    private void HandlePillarReturned(MidBossFireWavePillar pillar)
    {
        if (!ownsExecution || !activePillars.Remove(pillar))
            return;

        if (hasReachedEnd && activePillars.Count == 0 && patternRunner.CurrentState == BossPatternState.Active)
        {
            patternRunner.CompleteActive();
        }
    }

    private void HandleEffectStarted(MidBossFireWavePillar pillar)
    {
        if (!ownsExecution || patternRunner.CurrentState != BossPatternState.Active ||
            pillar != leadingPillar || !activePillars.Contains(pillar) || hasReachedEnd)
            return;

        Vector3 nextPosition = pillar.transform.position;
        nextPosition.x += waveDirection * Mathf.Max(0.01f, data.PillarSpacing);

        SpawnPillar(nextPosition);
    }

    private void StopExecution(bool completedNormally = false)
    {
        if (!ownsExecution)
            return;

            ownsExecution = false;
            leadingPillar = null;
            hasReachedEnd = false;
            activePillars.Clear();

            pillarPool.ReturnAll();

            if (completedNormally)
                bossBossVisual.FinishFireWaveAnim();
            else
                bossBossVisual.CancelFireWaveAnim();
    }

    private void AbortPattern()
    {
        bool shouldStopRunner = ownsExecution;
        StopExecution();

        if (shouldStopRunner && patternRunner != null)
            patternRunner.StopPattern();
    }

    public override bool TryStartAttack()
    {
        if (!isActiveAndEnabled || bossHealth == null || bossHealth.IsDead ||
            patternRunner == null || !patternRunner.IsStartAllowed ||
            !pillarPool.isActiveAndEnabled || !patternRunner.TrySelectPattern(this))
            return false;

        patternRunner.StartPatternWaitingForActiveEvent();

        return ownsExecution && patternRunner.CurrentState == BossPatternState.Windup;
    }

    [ContextMenu("테스트/불꽃파동 실행")]
    private void TestStartFireWave()
    {
        if (Application.isPlaying)
            TryStartAttack();
    }
}
