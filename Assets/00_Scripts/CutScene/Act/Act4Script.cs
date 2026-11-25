using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class Act4Script : MonoBehaviour
{
    [Header("기본")]
    [SerializeField] private CutsceneDialogue dialogue;
    [SerializeField] private CutsceneCamera cutsceneCamera;
    [SerializeField] private CutsceneCameraZoom cameraZoom;
    [SerializeField] private CutsceneCameraShake cameraShake;
    
    [Header("배우")]
    [SerializeField] private CutsceneActor player;

    [Header("컷신")]
    [SerializeField] private CanvasGroup cutScene;

    [Header("좌표")]
    [SerializeField] private Transform playerPoint1;
    [SerializeField] private Transform cameraPoint1;
    [SerializeField] private Transform playerPoint2;
    [SerializeField] private StageClear stageClear;

    private Animator playerAnim;

    private void Start()
    {
        playerAnim = player.GetComponent<Animator>();
        ScriptStart().Forget();
    }

    private async UniTask ScriptStart()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(1f));
        cutsceneCamera.SetMoveSpeed(1.5f);
        cutsceneCamera.FocusAsync(cameraPoint1).Forget();

        playerAnim.SetBool("isWalk", true);
        await player.MoveXAsync(playerPoint1.position.x, 3f);
        playerAnim.SetBool("isWalk", false);

        await UniTask.Delay(TimeSpan.FromSeconds(1f));
        await cutScene.DOFade(1f, 0.5f);
        await UniTask.Delay(TimeSpan.FromSeconds(0.5f));

        await dialogue.ShowAsync();
        await dialogue.ShowLineAsync(player.Speaker, PortraitSide.Left,
            "...별");
        await dialogue.ShowLineAsync(player.Speaker, PortraitSide.Left,
            "걔가 지나갔네. 길은 맞게 왔어.");
        await dialogue.HideAsync();

        await cutScene.DOFade(0f, 0.5f);
        await UniTask.Delay(TimeSpan.FromSeconds(0.5f));

        await dialogue.ShowAsync();
        await dialogue.ShowLineAsync(player.Speaker, PortraitSide.Left,
            "서두르자.");
        await dialogue.HideAsync();

        playerAnim.SetBool("isRun", true);
        player.MoveXAsync(playerPoint2.position.x, 2f).Forget();
        
        stageClear.EventStartLongTime();
    }
}
