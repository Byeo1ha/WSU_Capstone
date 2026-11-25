using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MidBossFireFieldPattern : BossAttackPattern
{
    [Header("실행 연결")]
    [SerializeField] private BossPatternRunner patternRunner;
    [SerializeField] private BossHealth bossHealth;
    [SerializeField] private BossTargetFinder targetFinder;
    [SerializeField] private MidBossVisual bossVisual;

    [Header("화염장판")]
    [SerializeField] private MidBossFireFieldData data;
    [SerializeField] private MidBossFireFieldPool fieldPool;

    [Header("바닥 탐색")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField, Min(0.1f)] private float groundSearchDisstance = 30f;

    private readonly HashSet<MidBossFireFieldPillar> activeFields = new();

    private Coroutine spawnCoroutine;
    private bool ownsExecution;
    private bool hasFinishedSpawning;

    private void OnEnable()
    {
        if (patternRunner == null || bossHealth == null ||
            targetFinder == null || bossVisual == null ||
            data == null || fieldPool == null)
        {
            Debug.LogError("화염장판 실행 참조와 데이터를 연결해 주세요.", this);
            enabled = false;
            return;
        }

        patternRunner.StateChanged += HandleStateChanged;
        fieldPool.FieldReturned += HandleFieldReturned;
    }

    private void OnDisable()
    {
        if (patternRunner != null)
            patternRunner.StateChanged -= HandleStateChanged;
            
        if (fieldPool != null)
            fieldPool.FieldReturned -= HandleFieldReturned;

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
            hasFinishedSpawning = false;
            bossVisual.StartFireFieldAnim();
            return;
        }

        if (!ownsExecution)
            return;

        if (state == BossPatternState.Active)
            spawnCoroutine = StartCoroutine(SpawnSequence());
        else if (state == BossPatternState.Recovery)
            StopExecution(completedNormally: true);
        else if (state == BossPatternState.Idle)
            StopExecution();
    }

    public void OnFieldCastFrame()
    {
        if (!isActiveAndEnabled || !ownsExecution || bossHealth.IsDead)
        return;

        patternRunner.EnterActiveFromEvent(this);
    }

    private IEnumerator SpawnSequence()
    {
        for (int i = 0; i < data.SpawnCount; i++)
        {
            if (!TrySpawnField())
            {
                spawnCoroutine = null;
                AbortPattern();
                yield break;
            }

            if (i < data.SpawnCount - 1)
                yield return new WaitForSeconds(data.SpawnInterval);
        }

        spawnCoroutine = null;
        hasFinishedSpawning = true;
        TryCompleteActive();
    }

    private bool TrySpawnField()
    {
        if (!targetFinder.TryGetTargetPosition(out Vector2 targetPosition))
        {
            Debug.LogWarning("화염장판의 플레이어 위치를 찾지 못했습니다.", this);
            return false;
        }

        RaycastHit2D hit = Physics2D.Raycast(
            targetPosition,
            Vector2.down,
            groundSearchDisstance,
            groundLayer);

            if (hit.collider == null || !hit.collider.CompareTag("Ground"))
            {
                Debug.LogWarning("플레이어 아래에서 Ground 바닥을 찾지 못했습니다.", this);
                return false;
            }

            Vector3 groundPoint = new Vector3(hit.point.x, hit.point.y, 0f);
            MidBossFireFieldPillar field = fieldPool.Rent(groundPoint);

            if (field == null)
                return false;

            activeFields.Add(field);

            return field.Begin(data, groundPoint);
    }

    private void HandleFieldReturned(MidBossFireFieldPillar field)
    {
        if (!ownsExecution || !activeFields.Remove(field))
            return;

            TryCompleteActive();
    }

    private void TryCompleteActive()
    {
        if (!ownsExecution || !hasFinishedSpawning || activeFields.Count > 0 ||
            patternRunner.CurrentState != BossPatternState.Active)
            return;

        patternRunner.CompleteActive();
    }

    private void StopExecution(bool completedNormally = false)
    {
        if (!ownsExecution)
            return;

        ownsExecution = false;
        hasFinishedSpawning = false;
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }

        activeFields.Clear();
        fieldPool.ReturnAll();

        if (!completedNormally)
            bossVisual.CancelFireFieldAnim();
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
            patternRunner == null || !patternRunner.IsStartAllowed || fieldPool == null ||
            !fieldPool.isActiveAndEnabled || !patternRunner.TrySelectPattern(this))
            return false;

        patternRunner.StartPatternWaitingForActiveEvent();

        return ownsExecution && patternRunner.CurrentState == BossPatternState.Windup;
    }

    [ContextMenu("테스트/화염장판 실행")]
    private void TestStartFireField()
    {
        if (Application.isPlaying)
            TryStartAttack();
    }

}
