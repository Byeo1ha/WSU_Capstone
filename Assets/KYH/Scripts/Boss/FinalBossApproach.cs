using System;
using DG.Tweening;
using UnityEngine;

public class FinalBossApproach : MonoBehaviour
{
    public enum ApproachResult
    {
        None,
        Approaching,
        ReadyForMelee,
        NeedsRanged,
        Cancelled,
        ReadyForMidRange
    }

    [Header("보스 연결")]
    [SerializeField] private Rigidbody2D bossBody;
    [SerializeField] private BossHealth bossHealth;
    [SerializeField] private BossPatternRunner patternRunner;

    [Header("대상과 현재 근접 공격")]
    [SerializeField] private BossTargetFinder targetFinder;
    [SerializeField] private FinalBossComboData comboData;

    [Header("접근 설정 - 임시값")]
    [SerializeField, Min(0.01f)] private float moveSpeed = 3f;
    [SerializeField, Min(0.1f)] private float meleeStartDistance = 1f;
    [SerializeField, Min(0.01f)] private float firstApproachDuration = 2f;
    [SerializeField, Min(0.1f)] private float additionalApproachDuration = 2f;

    [Header("현재 상태")]
    [SerializeField] private ApproachResult result;
    [SerializeField] private float timeRemaining;
    [SerializeField] private bool isAdditionalApproach;

    private Tweener movementTween;
    private float movementDestinationX;
    private bool isMidRangeApproach;
    private Vector2 midRangeStartRange;
    public ApproachResult Result => result;
    public bool IsApproaching => result == ApproachResult.Approaching;
    
    public event Action<ApproachResult> Finished;

    public bool BeginApproach()
    {
        return BeginApproachInternal(false, Vector2.zero);
    }

    public bool BeginMidRangeApproach(Vector2 startRange)
    {
        return BeginApproachInternal(true, startRange);
    }

    private bool BeginApproachInternal(bool forMidRange, Vector2 startRange)
    {
        if(!isActiveAndEnabled || IsApproaching)
            return false;

        if(bossBody == null || bossHealth == null || patternRunner == null || targetFinder == null || comboData == null)
        {
            Debug.LogError("접근 이동에 필요한 참조를 모두 연결해 주세요.", this);

            return false;
        }

        if (!CanContinue())
            return false;

        StopHorizontalMovement();

        isAdditionalApproach = false;
        isMidRangeApproach = forMidRange;
        midRangeStartRange = startRange;
        timeRemaining = firstApproachDuration;
        result = ApproachResult.Approaching;

        Debug.Log("최종보스 첫 접근 시작", this);
        return true;
    }

    private bool CanContinue()
    {
        return bossBody != null &&
               bossBody.gameObject.activeInHierarchy &&
               bossBody.simulated &&
               bossBody.bodyType != RigidbodyType2D.Static &&
               bossHealth != null &&
               !bossHealth.IsDead &&
               patternRunner.IsStartAllowed &&
               patternRunner.CurrentState == BossPatternState.Idle &&
               targetFinder != null &&
               targetFinder.isActiveAndEnabled &&
               comboData != null;
    }

    private void FixedUpdate()
    {
        if(!IsApproaching)
            return;

        if(!CanContinue() || !targetFinder.TryGetTargetPosition(out Vector2 targetPosition))
        {
            Finish(ApproachResult.Cancelled);
            return;
        }        

        int direction = targetPosition.x < bossBody.position.x ? -1 :1;

        Vector2 distanceToTarget = targetPosition - bossBody.position;

        float horizontalDistance = Mathf.Abs(targetPosition.x - bossBody.position.x);
        float startDistance = isMidRangeApproach ? midRangeStartRange.x : meleeStartDistance;
        bool isInsiderStartRange = horizontalDistance <= startDistance + 0.001f;

        if (isInsiderStartRange)
        {
            Finish(isMidRangeApproach ? ApproachResult.ReadyForMidRange : ApproachResult.ReadyForMelee);
            return;
        }

        timeRemaining = Mathf.Max(0f, timeRemaining - Time.fixedDeltaTime);

        if(timeRemaining <= 0f)
        {
            if(isAdditionalApproach)
            {
                Finish(isMidRangeApproach ? ApproachResult.ReadyForMidRange : ApproachResult.ReadyForMelee);
                return;
            }

            isAdditionalApproach = true;
            timeRemaining = additionalApproachDuration;

            Debug.Log("추가 접근선택: 한 번 더 추적합니다", this);
        }

        UpdateMovement(targetPosition.x, startDistance);
    }

    private bool IsInsideMeleeRange(Vector2 targetPosition, int direction)
    {
        Vector2 offset = comboData.SlashOffset;
        offset.x *= direction;

        Vector2 center = bossBody.position + offset;

        Vector2 halfSize = new Vector2(Mathf.Max(0.01f, comboData.SlashSize.x), Mathf.Max(0.01f, comboData.SlashSize.y)) * 0.5f;

        return Mathf.Abs(targetPosition.x - center.x) <= halfSize.x && Mathf.Abs(targetPosition.y - center.y) <= halfSize.y;
    }

    private void UpdateMovement(float targetX, float startDistance)
    {
        float currentX = bossBody.position.x;
        float direction = Mathf.Sign(targetX - currentX);
        float destinationX = targetX - direction * startDistance;

        if (movementTween != null && movementTween.IsActive() && Mathf.Abs(destinationX - movementDestinationX) <= 0.001f)
            return;

        movementTween?.Kill(false);

        movementDestinationX = destinationX;
        float duration = Mathf.Abs(destinationX - currentX) / Mathf.Max(0.01f, moveSpeed);

        movementTween = bossBody
            .DOMoveX(destinationX, duration)
            .SetEase(Ease.Linear)
            .SetUpdate(UpdateType.Fixed)
            .SetAutoKill(true)
            .SetLink(gameObject, LinkBehaviour.KillOnDisable);
    }

    private void Finish(ApproachResult nextResult)
    {
        if(!IsApproaching)
            return;

        result = nextResult;
        timeRemaining = 0f;

        StopHorizontalMovement();

        Debug.Log($"최종보스 접근 결과 : {nextResult}", this);

        Finished?.Invoke(nextResult);
    }


    private void StopHorizontalMovement()
    {
        if(movementTween != null)
        {
            movementTween.Kill(false);
            movementTween = null;
        }

        if(bossBody == null)
            return;

        Vector2 velocity = bossBody.linearVelocity;
        velocity.x = 0f;
        bossBody.linearVelocity = velocity;
    }

    public void CancelApproach()
    {
        Finish(ApproachResult.Cancelled);
    }

    private void OnDisable()
    {
        CancelApproach();

        StopHorizontalMovement();
    }

    [ContextMenu("테스트/근접 공격을 위한 접근")]
    private void TestBeginApproach()
    {
        if(Application.isPlaying)
            BeginApproach();
    }

    [ContextMenu("테스트/접근 중단")]
    private void TectCancelApproach()
    {
        if(Application.isPlaying)
            CancelApproach();
    }

}
