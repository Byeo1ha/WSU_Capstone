using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

[RequireComponent(typeof(Animator))]
public class PlayerAttackFeedback : MonoBehaviour
{
    private TargetCamera targetCamera;

    private Animator anim;

    private float originalAnimSpeed;

    [Inject]
    public void Construct(TargetCamera targetCamera)
    {
        this.targetCamera = targetCamera;
    }

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void StartFeedback()
    {
        CameraShaking();
        HitStop().Forget();
    }

    private void CameraShaking()
    {
        targetCamera.Shaking();
    }

    private async UniTask HitStop()
    {
        originalAnimSpeed = anim.speed;
        anim.speed = 0f;
        
        await UniTask.Delay(TimeSpan.FromSeconds(0.05f),
            cancellationToken: this.GetCancellationTokenOnDestroy());

        anim.speed = originalAnimSpeed;
    }
}
