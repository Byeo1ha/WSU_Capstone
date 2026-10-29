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
        Cancelled
    }

    [Header("보스 연결")]
    [SerializeField] private Rigidbody2D bossBody;
    [SerializeField] private BossHealth bossHealth;
    [SerializeField] private FinalBossPhaseController phaseController;
    [SerializeField] private BossPatternRunner patternRunner;

    [Header("대상과 현재 근접 공격")]
    [SerializeField] private BossTargetFinder targetFinder;
    [SerializeField] private FinalBossComboData comboData;

    [Header("접근 설정 - 임시값")]
    [SerializeField, Min(0.01f)] private float moveSpeed = 3f;
    [SerializeField, Min(0.01f)] private float firstApproachDuration = 2f;
    [SerializeField, Min(0.1f)] private float additionalApproachDuration = 2f;

    [Header("현재 상태")]
    [SerializeField] private ApproachResult result;
    [SerializeField] private float timeRemaining;
    [SerializeField] private bool isAdditionalApproach;

    private Tweener movementTween;
    public ApproachResult Result => result;
    public bool IsApproaching => result == ApproachResult.Approaching;
    
    public event Action<ApproachResult> Finished;

    public bool BeginApproach()
    {
        if(!isActiveAndEnabled || IsApproaching)
            return false;

        if(bossBody == null || bossHealth == null || phaseController == null || patternRunner == null || targetFinder == null || comboData == null)
        {
            Debug.LogError("접근 이동에 필요한 참조를 모두 연결해 주세요.", this);

            return false;
        }

        if (!CanContinue())
            return false;

        StopHorizontalMovement();

        isAdditionalApproach = false;
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
               phaseController != null &&
               phaseController.isActiveAndEnabled &&
               !phaseController.IsTransforming &&
               patternRunner != null &&
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

        if(IsInsideMeleeRange(targetPosition, direction))
        {
            Finish(ApproachResult.ReadyForMelee);
            return;
        }

        timeRemaining = Mathf.Max(0f, timeRemaining - Time.fixedDeltaTime);

        if(timeRemaining <= 0f)
        {
            if(isAdditionalApproach || UnityEngine.Random.value < 0.5f)
            {
                Finish(ApproachResult.NeedsRanged);
                return;
            }

            isAdditionalApproach = true;
            timeRemaining = additionalApproachDuration;

            Debug.Log("추가 접근선택: 한 번 더 추적합니다", this);
        }

        UpdateMovement(targetPosition.x);
    }

    private bool IsInsideMeleeRange(Vector2 targetPosition, int direction)
    {
        Vector2 offset = comboData.SlashOffset;
        offset.x *= direction;

        Vector2 center = bossBody.position + offset;

        Vector2 halfSize = new Vector2(Mathf.Max(0.01f, comboData.SlashSize.x), Mathf.Max(0.01f, comboData.SlashSize.y)) * 0.5f;

        return Mathf.Abs(targetPosition.x - center.x) <= halfSize.x && Mathf.Abs(targetPosition.y - center.y) <= halfSize.y;
    }

    private void UpdateMovement(float targetX)
    {
        float distance = Mathf.Abs(targetX - bossBody.position.x);

        if(distance <= 0.001f)
        {
            StopHorizontalMovement();
            return;
        }

        float duration = distance / Mathf.Max(0.1f, moveSpeed);

        if(movementTween == null || !movementTween.IsActive())
        {
            movementTween = bossBody
                .DOMoveX(targetX, duration)
                .SetEase(Ease.Linear)
                .SetUpdate(UpdateType.Fixed)
                .SetAutoKill(false)
                .SetLink(gameObject, LinkBehaviour.KillOnDisable);

            return;
        }

        movementTween.ChangeEndValue(new Vector2(targetX, bossBody.position.y), duration, true);

        movementTween.Play();
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
