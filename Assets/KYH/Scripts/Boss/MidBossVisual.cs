using UnityEngine;
using DG.Tweening;

public class MidBossVisual : MonoBehaviour
{
    [Header("전투 연결")]
    [SerializeField] private BossPatternRunner patternRunner;
    [SerializeField] private MidBossFireballPattern fireballPattern;
    [SerializeField] private MidBossFlamePillarPattern flamePillarPattern;
    [SerializeField] private MidBossTotemPattern totemPattern;

    [Header("애니메이션 연결")]
    [SerializeField] private Animator bossAnimator;
    [SerializeField] private AnimationClip fireballWindupClip;
    [SerializeField] private AnimationClip flamePillarWindupClip;
    [SerializeField] private AnimationClip TotemWindupClip;
    
    [Header("바라보는 방향")]
    [SerializeField] private BossTargetFinder targetFinder;
    [SerializeField] private SpriteRenderer bossRenderer;

    [SerializeField] private bool originalFacesRight = true;
    [SerializeField] private MidBossPatternSelector patternSelector;
    [SerializeField] private AnimationClip blinkFireWindupClip;
    [Header("블링크 이벤트 연결")]
    [SerializeField] private MidBossBlinkController blinkController;
    [Header("사망 전 떨림 - 임시값")]
    [SerializeField, Min(0.01f)] private float deathShakeDuration = 0.6f;
    [SerializeField, Min(0f)] private float deathShakeStrength = 0.035f;
    [SerializeField, Min(1)] private int deathShakeVibrato = 20;

    private readonly int DeadState = Animator.StringToHash("Base Layer.Dead");
    private readonly int IdleState = Animator.StringToHash("Base Layer.Idle");
    private readonly int FireballWindupState = Animator.StringToHash("Base Layer.FireballWindup");
    private readonly int FlamePillarWindupState = Animator.StringToHash("Base Layer.FlamePillarWindup");
    private readonly int TotemWindupState = Animator.StringToHash("Base Layer.TotemWindup");
    private readonly int blinkFireWindupState = Animator.StringToHash("Base Layer.BlinkFireWindup");
    private readonly int BlinkState = Animator.StringToHash("Base Layer.Blink");
    private bool hasStarted;
    private bool isDying;
    private Tween deathShake;
    private Transform shakeTarget;
    private Vector3 shakeOrigin;

    private void OnEnable()
    {
        if (patternRunner == null ||
            fireballPattern == null ||
            bossAnimator == null ||
            fireballWindupClip == null ||
            targetFinder == null ||
            bossRenderer == null ||
            flamePillarPattern == null ||
            flamePillarWindupClip == null ||
            totemPattern == null ||
            TotemWindupClip == null ||
            patternSelector == null ||
            blinkFireWindupClip == null ||
            blinkController == null)
        {
            Debug.LogError("전투,애니메이션, 방향설정 참조를 모두 연결해 주세요.", this);

            enabled = false;
            return;
        }

        patternRunner.StateChanged += HandleStateChanged;

        if (hasStarted)
            RefreshAnimation();
    }

    private void Start()
    {
        hasStarted = true;

        FaceTarget();
        RefreshAnimation();
    }

    private void OnDisable()
    {
        if (patternRunner != null)
            patternRunner.StateChanged -= HandleStateChanged;

        if(bossAnimator == null)
            return;

            bossAnimator.speed = 1f;

        if (hasStarted && bossAnimator.isActiveAndEnabled && bossAnimator.HasState(0, IdleState))
        {
            bossAnimator.Play(IdleState, 0, 0f);
        }
    }

    private void Update()
    {
        if (isDying)
            return;

        if(patternRunner.CurrentState == BossPatternState.Active && patternRunner.IsSelectedPattern(totemPattern))
        {
            FaceTarget();
        }
    }

    private void HandleStateChanged(BossPatternState state)
    {
        FaceTarget();
        RefreshAnimation();
    }

    private void RefreshAnimation()
    {
        if (isDying)
            return;

        if (!bossAnimator.isActiveAndEnabled)
            return;

        if (!bossAnimator.HasState(0, IdleState))
        {
            Debug.LogError("Animator의 Base Layer에 Idle 상태가 필요합니다.", this);
            return;
        }

        int selectedState = IdleState;
        AnimationClip selectedClip = null;

        if (patternRunner.CurrentState == BossPatternState.Windup)
        {
            if (patternRunner.IsSelectedPattern(fireballPattern))
            {
                if(patternSelector.CurrentPatternFollowsBlink)
                {
                selectedState = blinkFireWindupState;
                selectedClip = blinkFireWindupClip;
                }
                else
                {
                    selectedState = FireballWindupState;
                    selectedClip = fireballWindupClip;
                }
            
            }
            else if (patternRunner.IsSelectedPattern(flamePillarPattern))
            {
                selectedState = FlamePillarWindupState;
                selectedClip = flamePillarWindupClip;
            }
            else if (patternRunner.IsSelectedPattern(totemPattern))
            {
                selectedState = TotemWindupState;
                selectedClip = TotemWindupClip;
            }
        }

        if (selectedClip == null)
        {
            bossAnimator.speed = 1f;
            bossAnimator.Play(IdleState, 0, 0f);
            return;
        }

        if (!bossAnimator.HasState(0, selectedState))
        {
            Debug.LogError("선택한 준비 모션의 Animator 상태를 확인해 주세요.", this);

            bossAnimator.speed = 1f;
            bossAnimator.Play(IdleState, 0, 0f);
            return;
        }

        float duration = patternRunner.CurrentWindupDuration;

        if (duration <= 0f || selectedClip.length <= 0f)
        {
            bossAnimator.speed = 1f;
            bossAnimator.Play(IdleState, 0, 0f);
            return;
        }

        float remaining = Mathf.Clamp(
            patternRunner.StateTimeRemaing,
            0f,
            duration);

        float normalizedTime = Mathf.Clamp01(1f - remaining / duration);

        bossAnimator.speed = selectedClip.length / duration;

        bossAnimator.Play(selectedState, 0, normalizedTime);
    }

    private void FaceTarget()
    {
        if (isDying)
            return;

        if (!targetFinder.TryGetTargetPosition(out Vector2 targetPosition))
            return;

        float difference = targetPosition.x - bossRenderer.transform.position.x;

        if (Mathf.Abs(difference) <= 0.01f)
            return;

        bool shouldFaceRight = difference > 0f;

        bossRenderer.flipX = shouldFaceRight != originalFacesRight;
    }

    public bool TryPlayBlink()
    {
        if (isDying)
            return false;

        if(!isActiveAndEnabled || bossAnimator == null || !bossAnimator.isActiveAndEnabled)
            return false;

        if(!bossAnimator.HasState(0, BlinkState))
        {
            Debug.LogError("Animator의 Base Layer에 Blink상태를 만들어 주세요.", this);
            return false;

        }

        FaceTarget();

        bossAnimator.speed = 1f;
        bossAnimator.Play(BlinkState, 0, 0f);

        return true;
    }
    
    public void OnBlinkMove()
    {
        if (blinkController != null)
            blinkController.ExecuteBlinkMovement();
    }

    public void OnBlinkFinished()
    {
        if (blinkController != null)
            blinkController.FinishBlink();
    }

    public void BeginDeath()
    {
        if (isDying)
            return;

        isDying = true;

        if (blinkController != null)
            blinkController.CancelBlink();

        if (bossAnimator == null || !bossAnimator.isActiveAndEnabled)
        {
            Debug.LogError("Animator가 활성화되어 있지 않습니다.", this);
            return;
        }

        if (bossAnimator.HasState(0, IdleState))
        {
            bossAnimator.speed = 1f;
            bossAnimator.Play(IdleState, 0, 0f);
            bossAnimator.Update(0f);
        }

        bossAnimator.speed = 0f;

        shakeTarget = bossAnimator.transform;
        shakeOrigin = shakeTarget.localPosition;

        deathShake = shakeTarget.DOShakePosition(deathShakeDuration,
                new Vector3(deathShakeStrength, 0f, 0f),
                deathShakeVibrato,
                90f,
                false,
                false)
            .SetLink(gameObject, LinkBehaviour.KillOnDisable)
            .OnComplete(() =>
            {
                deathShake = null;

                if (shakeTarget != null)
                    shakeTarget.localPosition = shakeOrigin;

                PlayDeadAnimation();
            });
    }

    private void PlayDeadAnimation()
    {
        if (!isDying ||
            bossAnimator == null ||
            !bossAnimator.isActiveAndEnabled)
            return;

        if (!bossAnimator.HasState(0, DeadState))
        {
            Debug.LogError(
                "Animator의 Base Layer에 MidBossDead 상태가 필요합니다.",
                this);

            return;
        }

        // 저장된 클립 시간을 그대로 사용한다.
        bossAnimator.speed = 1f;
        bossAnimator.Play(DeadState, 0, 0f);
    }

    private void StopDeathShake()
    {
        if (deathShake == null)
            return;

        deathShake.Kill(false);
        deathShake = null;

        if (shakeTarget != null)
            shakeTarget.localPosition = shakeOrigin;
    }


}


