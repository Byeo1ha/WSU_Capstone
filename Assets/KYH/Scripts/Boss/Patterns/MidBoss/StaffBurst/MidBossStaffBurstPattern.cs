using System.Collections.Generic;
using UnityEngine;

public class MidBossStaffBurstPattern : BossAttackPattern
{
    [Header("보스 연결")]
    [SerializeField] private BossPatternRunner patternRunner;
    [SerializeField] private BossHealth bossHealth;
    [SerializeField] private SpriteRenderer bossRenderer;
    [SerializeField] private Transform staffBurstPoint;
    [Tooltip("발사점을 배치할 떄 보스의 FlipX 상태")]
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
    private readonly float[] beamLengths = new float[5];

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
        if (patternRunner == null || bossHealth == null ||
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
        }
    }

    public override bool TryStartAttack()
    {
        if (!isActiveAndEnabled || ownsExecution || bossHealth.IsDead ||
            !patternRunner.IsStartAllowed || !beamRoot.gameObject.activeInHierarchy ||
            !patternRunner.TrySelectPattern(this))
        {
            return false;
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
            beamLengths[i] = length;

            Vector3 enddPosition = castPosition + (Vector3)(directions[i] * length);

            beams[i].ShowWarning(castPosition, enddPosition, castWidth);
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

            beams[i].ShowAttack();
            CheckBeamDamage(i);
        }
    }

    private void CheckBeamDamage(int index)
    {
        float length = beamLengths[index];

        if (length <= 0.001f)
            return;

        Vector2 direction = directions[index];

        Vector2 center = (Vector2)castPosition + direction * (length * 0.5f);

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
        if (!ownsExecution || !hasFired ||
            !patternRunner.IsSelectedPattern(this) ||
            patternRunner.CurrentState != BossPatternState.Active)
        {
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


