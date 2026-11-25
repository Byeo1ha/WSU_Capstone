using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class Act5Script : MonoBehaviour
{
    [Header("기본")]
    [SerializeField] private CutsceneDialogue dialogue;
    [SerializeField] private CutsceneCamera cutsceneCamera;
    [SerializeField] private CutsceneCameraZoom cameraZoom;
    [SerializeField] private CutsceneCameraShake cameraShake;
    [SerializeField] private StageClear stageClear;
    
    [Header("배우")]
    [SerializeField] private CutsceneActor player;
    [SerializeField] private CutsceneActor boss;

    [Header("기타")]
    [SerializeField] private Transform playerPoint;
    [SerializeField] private Transform cameraPoint;

    private Animator playerAnim;

    private void Start()
    {
        playerAnim = player.GetComponent<Animator>();
        ScriptStart().Forget();
    }

    private async UniTask ScriptStart()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(1f));

        MovePlayerAsync().Forget();
        cutsceneCamera.SetMoveSpeed(1.5f);
        cutsceneCamera.FocusAsync(cameraPoint).Forget();

        await UniTask.Delay(TimeSpan.FromSeconds(0.5f));

        await dialogue.ShowAsync();
        await dialogue.ShowLineAsync(boss.Speaker, PortraitSide.Right,
            "큭큭.. 또 올라왔나. 이번 달 들어 세 번째다.");
        await dialogue.ShowLineAsync(player.Speaker, PortraitSide.Left,
            "비켜. 시간 없어.");
        await dialogue.ShowLineAsync(boss.Speaker, PortraitSide.Right,
            "여기를 지나가려면 나를 넘어야 한다. 돌아가라고 해도 안 듣겠지.");
        await dialogue.ShowLineAsync(player.Speaker, PortraitSide.Left,
            "알면서 왜 물어.");
        await dialogue.ShowLineAsync(boss.Speaker, PortraitSide.Right,
            "전에 온 녀석들도 똑같이 말했거든. 그리고 전부 여기서 끝났다.");
        await dialogue.ShowLineAsync(player.Speaker, PortraitSide.Left,
            "그래? 난 다른데.");
        await dialogue.ShowLineAsync(boss.Speaker, PortraitSide.Right,
            "큭큭.. 허세는 인정해 주마.");
        await dialogue.HideAsync();

        await UniTask.Delay(TimeSpan.FromSeconds(1f));

        stageClear.EventStart();
    }

    private async UniTask MovePlayerAsync()
    {
        playerAnim.SetBool("isWalk", true);
        await player.MoveXAsync(playerPoint.position.x, 3f);

        if (playerAnim != null)
        {
            playerAnim.SetBool("isWalk", false);
        }
    }
}
