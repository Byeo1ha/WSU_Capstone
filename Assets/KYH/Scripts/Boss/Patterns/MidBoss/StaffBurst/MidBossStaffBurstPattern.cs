using System.Collections.Generic;
using UnityEngine;

public class MidBossStaffBurstPattern : BossAttackPattern
{
    [Header("보스 연결")]
    [SerializeField] private BossPatternRunner patternRunner;
    [SerializeField] private BossHealth bossHealth;
    [SerializeField] private MidBossVisual bossVisual;
    [SerializeField] private SpriteRenderer bossRenderer;
    [SerializeField] private Transform staffBurstPoint;
    [Tooltip("발사점을 배치할 때 보스의 FlipX 상태")]
    [SerializeField] private bool pointReferenceFlipX;

    [Header("광선 연결")]
    [SerializeField] private MidBossStaffBurstData data;
    [SerializeField] private MidBossStaffBurstBeam beamPrefab;
    [SerializeField] private Transform beamRoot;

    [Header("검사할 레이어")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask damageLayer;

    private readonly Vector2[] directions = {Vector2.left,
        Vector2.right, Vector2.up, new Vector2(-1f, 1f).normalized,
        new Vector2(1f, 1f).normalized};

    private readonly MidBossStaffBurstBeam[] beams = new MidBossStaffBurstBeam[5];


    private readonly List<RaycastHit2D> terrainHits = new();
    private readonly List<Collider2D> foundColliders = new();
    private readonly HashSet<IDamageable> damagedTargets = new();

    private ContactFilter2D terrainFilter;
    private ContactFilter2D damageFilter;

    private Vector3 castPosition;
    private float castWidth;

    private bool ownsExecution;
    private bool hasFired;

    private void Awake()
    {
        if (patternRunner == null || bossHealth == null || bossVisual == null ||
            bossRenderer == null || staffBurstPoint == null ||
            staffBurstPoint.parent == null || data == null ||
            beamPrefab == null || beamRoot == null)
        {
            Debug.LogError("지팡이방사 실행 참조를 모두 연결해 주세요.", this);
            enabled = false;
            return;
        }

        for (int i = 0; i < beams.Length; i++)
            beams[i] = Instantiate(beamPrefab, beamRoot);
    }

    private void Update()
    {
        if (!ownsExecution)
            return;

        if (bossHealth.IsDead || !patternRunner.IsStartAllowed ||
            !patternRunner.isActiveAndEnabled || !patternRunner.IsSelectedPattern(this))
        {
            AbortPattern();
            return;
        }

        BossPatternState state = patternRunner.CurrentState;

        if (state != BossPatternState.Windup && state != BossPatternState.Active)
        {
            AbortPattern();
            return;
        }

        if (!hasFired || Time.timeScale <= 0f)
            return;

        bool hasRunningBeam = false;

        for (int i = 0; i < beams.Length; i++)
        {
            if (!ownsExecution || bossHealth.IsDead || Time.timeScale <= 0f)
                return;

            beams[i].Tick(Time.deltaTime);
            CheckBeamDamage(i);
            hasRunningBeam |= beams[i].IsRunning;
        }

        if (!hasRunningBeam)
            TryCompleteActive();
    }

    public override bool TryStartAttack()
    {
        if (!isActiveAndEnabled || ownsExecution || bossHealth.IsDead ||
            !patternRunner.IsStartAllowed || !beamRoot.gameObject.activeInHierarchy ||
            !patternRunner.TrySelectPattern(this))
        {
            return false;
        }

        foreach (MidBossStaffBurstBeam beam in beams)
        {
            if (beam == null || !beam.IsReady || !beam.isActiveAndEnabled)
            {
                Debug.LogError("광선 프리팹의 레이저 그림과 Renderer 연결을 확인해 주세요.", this);
                return false;
            }
        }

        ownsExecution = true;
        hasFired = false;
        damagedTargets.Clear();

        patternRunner.StartPatternWaitingForActiveEvent();

        if (patternRunner.CurrentState != BossPatternState.Windup)
        {
            AbortPattern();
            return false;
        }

        PrepareWarnings();
        bossVisual.StartStaffBurstAnim();
        
        return true;
    }

    private void PrepareWarnings()
    {
        Vector3 localPoint = staffBurstPoint.localPosition;

        if (bossRenderer.flipX != pointReferenceFlipX)
            localPoint.x = -localPoint.x;

        castPosition = staffBurstPoint.parent.TransformPoint(localPoint);

        castWidth = data.BeamWidth;

        terrainFilter = new ContactFilter2D();
        terrainFilter.SetLayerMask(groundLayer);
        terrainFilter.useTriggers = false;

        damageFilter = new ContactFilter2D();
        damageFilter.SetLayerMask(damageLayer);
        damageFilter.useTriggers = true;

        for (int i = 0; i < directions.Length; i++)
        {
            float length = FindBeamLength(directions[i]);

            Vector3 endPosition = castPosition + (Vector3)(directions[i] * length);

            beams[i].ShowWarning(castPosition, endPosition, castWidth);
        }
    }

    private float FindBeamLength(Vector2 direction)
    {
        terrainHits.Clear();

        Physics2D.Raycast((Vector2)castPosition, direction,
            terrainFilter, terrainHits, data.MaxDistance);

        float length = data.MaxDistance;

        foreach (RaycastHit2D hit in terrainHits)
        {
            if (hit.collider != null)
            length = Mathf.Min(length, hit.distance);
        }

        return length;
    }

    public void OnAttackFrame()
    {
        if (!isActiveAndEnabled || !ownsExecution || hasFired ||
            bossHealth.IsDead || Time.timeScale <= 0f ||
            !patternRunner.IsSelectedPattern(this) ||
            patternRunner.CurrentState != BossPatternState.Windup)
        {
            return;
        }

        patternRunner.EnterActiveFromEvent(this);

        if (patternRunner.CurrentState != BossPatternState.Active)
            return;

        hasFired = true;

        for (int i = 0; i < beams.Length; i ++)
        {
            if (!ownsExecution || bossHealth.IsDead ||
                Time.timeScale <= 0f)
            {
                return;
            }

            beams[i].ShowAttack(data.EmissionDuration, data.MoveSpeed);
        }
    }

    private void CheckBeamDamage(int index)
    {
        float length = beams[index].VisibleLength;

        if (length <= 0.001f)
            return;

        Vector2 direction = directions[index];

        float centerDistance = beams[index].VisibleStart + length * 0.5f;

        Vector2 center = (Vector2)castPosition + direction * centerDistance;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        foundColliders.Clear();

        Physics2D.OverlapBox(center, new Vector2(length, castWidth), angle,
            damageFilter, foundColliders);

        foreach (Collider2D other in foundColliders)
        {
            if (!ownsExecution || bossHealth.IsDead ||
                Time.timeScale <= 0f)
            {
                return;
            }

            if (other == null ||
                !other.TryGetComponent<IDamageable>(out var target))
            {
                continue;
            }

            if (!damagedTargets.Add(target))
                continue;

            target.TakeDamage(data.Damage);
        }
    }

    public void OnAnimationFinished()
    {
        TryCompleteActive();
    }

    private void TryCompleteActive()
    {
        if (!ownsExecution || !hasFired ||
            !patternRunner.IsSelectedPattern(this) ||
            patternRunner.CurrentState != BossPatternState.Active)
        {
            return;
        }

        // 보스 모션이 먼저 끝나도 레이저의 마지막 꼬리까지 기다립니다.
        foreach (MidBossStaffBurstBeam beam in beams)
        {
            if (beam != null && beam.IsRunning)
                return;
        }

        ClearExecution();
        patternRunner.CompleteActive();
    }

    private void ClearExecution()
    {
        ownsExecution = false;
        hasFired = false;

        damagedTargets.Clear();

        foreach (MidBossStaffBurstBeam beam in beams)
        {
            if (beam != null)
                beam.Hide();
        }
    }

    private void AbortPattern()
    {
        bool shouldStopRunner = ownsExecution;
        ClearExecution();

        if (shouldStopRunner && bossVisual != null)
            bossVisual.CancelStaffBurstAnim();

        if (shouldStopRunner && patternRunner != null &&
            patternRunner.IsSelectedPattern(this))
        {
            patternRunner.StopPattern();
        }
    }

    private void OnDisable()
    {
        AbortPattern();
    }

    private void OnDestroy()
    {
        foreach (MidBossStaffBurstBeam beam in beams)
        {
            if (beam != null)
                Destroy(beam.gameObject);
        }
    }

    [ContextMenu("테스트/지팡이방사 준비")]
    private void TestStart()
    {
        if (Application.isPlaying)
            TryStartAttack();
    }

    [ContextMenu("테스트/지팡이방사 공격")]
    private void TestAttack()
    {
        if (Application.isPlaying)
            OnAttackFrame();
    }

    [ContextMenu("테스트/지팡이방사 종료")]
    private void TestFinish()
    {
        if (Application.isPlaying)
            OnAnimationFinished();
    }
}


