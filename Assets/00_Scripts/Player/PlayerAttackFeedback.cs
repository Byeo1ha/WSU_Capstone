using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

[RequireComponent(typeof(PlayerNormalAttack))]
[RequireComponent(typeof(Animator))]
public class PlayerAttackFeedback : MonoBehaviour
{
    private TargetCamera targetCamera;

    private PlayerNormalAttack playerNormalAttack;
    private Animator anim;

    private float originalAnimSpeed;
    private bool isHitStopping;

    [Inject]
    public void Construct(TargetCamera targetCamera)
    {
        this.targetCamera = targetCamera;
    }

    private void Awake()
    {
        playerNormalAttack = GetComponent<PlayerNormalAttack>();
        anim = GetComponent<Animator>();
    }

    public void StartFeedback()
    {
        CameraShaking();
        HitStop().Forget();
    }

    private void CameraShaking()
    {
        if (playerNormalAttack.ComboIndex
            == playerNormalAttack.MaxCombo - 1)
            targetCamera.PowerShaking();
        else
            targetCamera.NormalShaking();
    }

    private async UniTask HitStop()
    {
        if (isHitStopping) return;
        
        isHitStopping = true;

        originalAnimSpeed = anim.speed;
        anim.speed = 0f;

        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(0.05f),
                cancellationToken: this.GetCancellationTokenOnDestroy());
        }
        finally
        {
            anim.speed = originalAnimSpeed;
            isHitStopping = false;
        }
    }
}
