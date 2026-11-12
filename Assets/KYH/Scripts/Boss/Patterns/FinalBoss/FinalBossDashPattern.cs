using DG.Tweening;
using UnityEngine;
using System.Collections.Generic;

public class FinalBossDashPattern : MonoBehaviour
{
    [Header("실행 연결")]
    [SerializeField] private BossPatternRunner patternRunner;
    [SerializeField] private BossHealth bossHealth;
    [SerializeField] private FinalBossPhaseController phaseController;
    [SerializeField] private BossTargetFinder targetFinder;
    [SerializeField] private Rigidbody2D bossBody;

    [Header("돌진 이동 한계")]
    [SerializeField] private Transform leftDashLimit;
    [SerializeField] private Transform rightDashLimit; 

    [Header("돌진 이동 - 임시값")]
    [SerializeField, Min(0.1f)] private float dashDistance = 4f;
    [SerializeField, Min(0.05f)] private float dashDuration = 0.25f;

    [Header("돌긴 경고")]
    [SerializeField] private BossLineWarning dashWarningPrefab; 
    [SerializeField, Min(0.1f)] private float warningHeight = 1.5f;
    [SerializeField] private float warningOffsetY;

    [Header("돌진 피해")]
    [SerializeField, Min(1)] private int dashDamage = 14;
    [SerializeField] private LayerMask damageLayer;
    [SerializeField] private Vector2 hitboxSize = new Vector2(1f, 1.5f);
    [SerializeField] private float hitboxOffsetY;

    [Header("중거리 공격 시작 범위")]
    [SerializeField, Min(0.1f)] private float approachStartRangeX = 4f;
    [SerializeField, Min(0.1f)] private float approachStartRangeY = 0.75f;

    private readonly List<Collider2D> dashHits = new List<Collider2D>();
    private readonly HashSet<IDamageable> damagedTargets = new HashSet<IDamageable>();

    public Vector2 ApproachStartRange => new Vector2(approachStartRangeX, approachStartRangeY);

    private ContactFilter2D damageFilter;
    private Vector2 lastHitPosition;
    private BossLineWarning dashWarning;
    private bool isDashHitting;
    private bool dashMovementFinished;
    private bool ownsExecution;
    private float destinationX;
    private Tween dashTween;

    private void Awake()
    {
        if(patternRunner != null && bossHealth != null && phaseController != null && bossBody != null)
        {
            if (dashWarningPrefab != null)
                dashWarning = Instantiate(dashWarningPrefab);

            return;
        }

        Debug.LogError("돌진 발도에 필요한 참조를 모두 연결해 주세요", this);
        enabled = false;
    } 

    private void Update()
    {
        if (ownsExecution && !CanExecute())
        {
            AbortPattern();
            return;
        }

        if (!isDashHitting || !dashMovementFinished)
            return;

        CheckDashDamage();

        if (ownsExecution && patternRunner.CurrentState == BossPatternState.Active)
            patternRunner.CompleteActive();    
    }

    private void FixedUpdate()
    {
        if (isDashHitting && CanExecute())
            CheckDashDamage();
    }


    private void OnEnable()
    {
        patternRunner.StateChanged += HandleStateChanged;
    }

    private void OnDisable()
    {
        if (patternRunner != null)
            patternRunner.StateChanged -= HandleStateChanged;

        if (ownsExecution)
            AbortPattern();
    }

    private void OnDestroy()
    {
        if (dashWarning != null)
            Destroy(dashWarning.gameObject);
    }

    private bool CanExecute()
    {
        return isActiveAndEnabled &&
               patternRunner.IsStartAllowed &&
               !bossHealth.IsDead &&
               phaseController.isActiveAndEnabled &&
               !phaseController.IsTransforming &&
               phaseController.CurrentPhase == FinalBossPhase.Phase1 &&
               bossBody.simulated &&
               bossBody.bodyType != RigidbodyType2D.Static;
    }

    private void HandleStateChanged(BossPatternState state)
    {
        if (state == BossPatternState.Windup && patternRunner.IsSelectedPattern(this))
        {
            ownsExecution = true;

            if (!CanExecute() || !targetFinder.isActiveAndEnabled || !targetFinder.TryGetTargetPosition(out Vector2 targetPosition))
            {
                AbortPattern();
                return;
            }

            if (leftDashLimit == null || rightDashLimit == null ||
                leftDashLimit.position.x >= rightDashLimit.position.x)
            {
                Debug.LogError("돌진 한계를 왼쪽과 오른쪽 순서로 연결해 주세요.", this);
                AbortPattern();
                return;
            }

            float startX = bossBody.position.x;
            int direction = targetPosition.x < startX ? -1 : 1;
            float limitX = direction < 0 ? leftDashLimit.position.x : rightDashLimit.position.x;

            float availableDistance = Mathf.Max(0f, (limitX - startX) * direction);
            float travelDistance = Mathf.Min(dashDistance, availableDistance);

            destinationX = startX + direction * travelDistance;
            ShowDashWarning(startX);
            return;
        }

        if (!ownsExecution)
            return;

        if (state == BossPatternState.Active)
            BeginDash();
        else if (state == BossPatternState.Recovery || state == BossPatternState.Idle)
            CancelExecution();
    }

    private void BeginDash()
    {
        HideDashWarning();
        if (!CanExecute())
        {
            AbortPattern();
            return;
        }

        if (Mathf.Abs(destinationX - bossBody.position.x) <= 0.01f)
        {
            patternRunner.CompleteActive();
            return;
        }

        damagedTargets.Clear();
        damageFilter = new ContactFilter2D();
        damageFilter.SetLayerMask(damageLayer);
        damageFilter.useTriggers = true;

        lastHitPosition = bossBody.position;
        dashMovementFinished = false;
        isDashHitting = true;

        dashTween = bossBody.DOMoveX(destinationX, dashDuration)
            .SetEase(Ease.Linear)
            .SetUpdate(UpdateType.Fixed)
            .OnComplete(FinishDash);
    }

    private void FinishDash()
    {
        dashTween = null;

        dashMovementFinished = true;
    }

    private void CheckDashDamage()
    {
        Vector2 currentPosition = bossBody.position;
        Vector2 center = (lastHitPosition + currentPosition) * 0.5f;
        center.y += hitboxOffsetY;

        Vector2 movement = currentPosition - lastHitPosition;
        Vector2 size = new Vector2(Mathf.Max(0.01f, hitboxSize.x) + Mathf.Abs(movement.x), Mathf.Max(0.01f, hitboxSize.y) + Mathf.Abs(movement.y));

        lastHitPosition = currentPosition;
        dashHits.Clear();

        Physics2D.OverlapBox(center, size, 0f, damageFilter, dashHits);

        foreach (Collider2D hit in dashHits)
        {
            if (!isDashHitting)
                return;

            if (hit == null || hit.gameObject.layer != LayerMask.NameToLayer("Player"))
                continue;

            if (hit.TryGetComponent<IDamageable>(out var target) && damagedTargets.Add(target))
            {
                target.TakeDamage(dashDamage);
            }
        }


    }

    private void CancelExecution()
    {
        ownsExecution = false;
        dashTween?.Kill(false);
        dashTween = null;
        HideDashWarning();

        isDashHitting = false;
        dashMovementFinished = false;
        damagedTargets.Clear();
    }

    private void AbortPattern()
    {
        CancelExecution();
        patternRunner.StopPattern();
    }

    private void ShowDashWarning(float startX)
    {
        if (dashWarning == null)
            return;

       dashWarning.Show(startX, destinationX, bossBody.position.y + warningOffsetY, warningHeight);
    }

    private void HideDashWarning()
    {
        if (dashWarning != null)
            dashWarning.Hide();
    }

    public bool TryStartDash()
    {
        if (!CanExecute() || !patternRunner.TrySelectPattern(this))
            return false;

        patternRunner.StartPattern();

        return ownsExecution && patternRunner.CurrentState == BossPatternState.Windup;
    }

    [ContextMenu("테스트/돌진 발도 이동")]
    private void TestDash()
    {
        if (Application.isPlaying)
            TryStartDash();
    }
}
