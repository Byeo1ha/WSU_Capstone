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

    // 외부에서 실행하는 공격 연출 함수
    public void StartFeedback(ShakingType type)
    {
        CameraShaking(type);
        HitStop().Forget();
    }

    // enum 타입에 따라 재생시킬 카메라 흔들림 정도를 결정하는 함수
    private void CameraShaking(ShakingType type)
    {
        if (type == ShakingType.Power)
            targetCamera.PowerShaking();
        else
            targetCamera.NormalShaking();
    }

    // 히트 스톱 함수
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
            if (hitStopCancellation == cancellation)
            {
                hitStopCancellation = null;
                RestoreHitStop();
            }

            cancellation.Dispose();
        }
    }

    // 히트스톱 종료 후 원래의 속도로 되돌릴 함수
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
