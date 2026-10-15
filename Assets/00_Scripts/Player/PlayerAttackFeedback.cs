using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

[RequireComponent(typeof(PlayerNormalAttack))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerAttackFeedback : MonoBehaviour
{
    [Header("히트 스톱 시간")]
    [SerializeField, Min(0f)] private float duration = 0.05f;

    private TargetCamera targetCamera;

    private PlayerNormalAttack playerNormalAttack;
    private Animator anim;
    private Rigidbody2D rigid;

    private float originalAnimSpeed;
    private Vector2 originalVelocity;
    private RigidbodyConstraints2D originalConstraints;
    private bool isHitStopping;
    private CancellationTokenSource hitStopCancellation;

    public bool IsHitStopping => isHitStopping;

    [Inject]
    public void Construct(TargetCamera targetCamera)
    {
        this.targetCamera = targetCamera;
    }

    private void Awake()
    {
        playerNormalAttack = GetComponent<PlayerNormalAttack>();
        anim = GetComponent<Animator>();
        rigid = GetComponent<Rigidbody2D>();
    }

    private void OnDisable()
    {
        if (hitStopCancellation == null) return;

        CancellationTokenSource cancellation = hitStopCancellation;
        hitStopCancellation = null;
        cancellation.Cancel();
        RestoreHitStop();
    }

    public void StartFeedback(ShakingType type)
    {
        CameraShaking(type);
        HitStop().Forget();
    }

    private void CameraShaking(ShakingType type)
    {
        if (type == ShakingType.Power)
            targetCamera.PowerShaking();
        else
            targetCamera.NormalShaking();
    }

    private async UniTask HitStop()
    {
        if (isHitStopping || !isActiveAndEnabled || duration <= 0f) return;

        isHitStopping = true;

        originalAnimSpeed = anim.speed;
        originalVelocity = rigid.linearVelocity;
        originalConstraints = rigid.constraints;

        anim.speed = 0f;
        rigid.linearVelocity = Vector2.zero;
        rigid.constraints = originalConstraints | RigidbodyConstraints2D.FreezePosition;

        CancellationTokenSource cancellation = CancellationTokenSource.CreateLinkedTokenSource(
            this.GetCancellationTokenOnDestroy());
        hitStopCancellation = cancellation;

        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(duration),
                cancellationToken: cancellation.Token);
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
        {
        }
        finally
        {
            // 이전 대기가 재활성화 후 시작된 히트 스톱을 해제하지 않도록 한다.
            if (hitStopCancellation == cancellation)
            {
                hitStopCancellation = null;
                RestoreHitStop();
            }

            cancellation.Dispose();
        }
    }

    private void RestoreHitStop()
    {
        if (anim != null)
            anim.speed = originalAnimSpeed;

        if (rigid != null)
        {
            rigid.constraints = originalConstraints;
            rigid.linearVelocity = originalVelocity;
        }

        isHitStopping = false;
    }
}
