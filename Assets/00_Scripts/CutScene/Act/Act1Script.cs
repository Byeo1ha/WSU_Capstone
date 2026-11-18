using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class Act1Script : MonoBehaviour
{
    [SerializeField] private CutsceneActor actorA;
    [SerializeField] private CutsceneActor actorB;
    [SerializeField] private CutsceneActor actorC;
    [SerializeField] private CutsceneDialogue dialogue;
    [SerializeField] private CutsceneBackgroundFade backgroundFade;
    [SerializeField] private SpriteRenderer changeBackground;
    [SerializeField] private CutsceneCamera cutsceneCamera;
    [SerializeField] private CutsceneCameraZoom cameraZoom;
    [SerializeField] private CutsceneCameraShake cameraShake;
    
    [SerializeField] private Transform cameraMovePoint1;
    [SerializeField] private Transform cameraMovePoint2;
    [SerializeField] private Transform cameraMovePoint3;

    [SerializeField] private StageClear stageClear;

    private SpriteRenderer actorASprite;

    private void Awake()
    {
        actorASprite = actorA.GetComponent<SpriteRenderer>();
        changeBackground.color = new Color(1f, 1f, 1f, 0f);
    }

    private void Start()
    {
        ScriptStart().Forget();
    }

    private async UniTask ScriptStart()
    {
        cutsceneCamera.SetMoveSpeed(1f);
        await cutsceneCamera.FocusAsync(cameraMovePoint1);
        await dialogue.ShowAsync();
        await dialogue.ShowLineAsync(actorA.Speaker, true, "이제 끝이야.");
        await dialogue.ShowLineAsync(actorA.Speaker, true, "난 널 쓰러뜨리고 더 위로 올라가야만 해.");
        await dialogue.ShowLineAsync(actorB.Speaker, false, "크큭..");
        await dialogue.ShowLineAsync(actorA.Speaker, true, "뭐가 우습지?");
        await dialogue.ShowLineAsync(actorB.Speaker, false, "...그 분께서 하신 말씀이 옳았군");
        await dialogue.ShowLineAsync(actorA.Speaker, true, "...뭐?");
        await dialogue.HideAsync();

        cameraShake.Shake(2f, 0.2f);
        actorASprite.flipX = true;
        await UniTask.Delay(TimeSpan.FromSeconds(1f));
        actorASprite.flipX = false;

        await changeBackground.DOFade(1f, 1f);

        await dialogue.ShowAsync();
        await dialogue.ShowLineAsync(actorA.Speaker, true, "!!");
        await dialogue.HideAsync();
        cutsceneCamera.SetMoveSpeed(3f);
        await cutsceneCamera.FocusAsync(cameraMovePoint2);

        await dialogue.ShowAsync();
        await dialogue.ShowLineAsync(actorC.Speaker, false, "여기까지 직접 발을 들이다니..");
        await dialogue.ShowLineAsync(actorC.Speaker, false, "복수심에 눈이 멀어서 겁을 상실한건가? 그게 아니라면...");
        await dialogue.ShowLineAsync(actorC.Speaker, false, "단순히 멍청하다던가.");
        await dialogue.HideAsync();

        cutsceneCamera.SetMoveSpeed(1f);
        cutsceneCamera.FocusAsync(cameraMovePoint3).Forget();
        await cameraZoom.ZoomToAsync(5f, 3f);
        await dialogue.ShowAsync();
        await dialogue.ShowLineAsync(actorA.Speaker, true, "유감이지만 딱히 복수심에 눈이 멀었다던가, 멍청해서 여기까지 온건 아니야.");
        await dialogue.ShowLineAsync(actorA.Speaker, true, "내가 이곳에 온 이유는...");
        await dialogue.ShowLineAsync(actorA.Speaker, true, "나 스스로를 증명하기 위해서지.");
        await dialogue.ShowLineAsync(actorC.Speaker, false, "증명이라...");
        await dialogue.ShowLineAsync(actorC.Speaker, false, "그걸 보이기엔 지금은 너무나도 부끄러운 실력 같군.");
        await dialogue.ShowLineAsync(actorC.Speaker, false, "보여봐라. 그 증명이란걸 말이야.");
        await dialogue.HideAsync();

        stageClear.EventStart();
    }
}
