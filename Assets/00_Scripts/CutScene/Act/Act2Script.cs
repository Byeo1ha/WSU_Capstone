using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class Act2Script : MonoBehaviour
{
    [Header("기본")]
    [SerializeField] private CutsceneDialogue dialogue;
    [SerializeField] private CutsceneCamera cutsceneCamera;
    [SerializeField] private CutsceneCameraZoom cameraZoom;
    [SerializeField] private CutsceneCameraShake cameraShake;

    [Header("배우")]
    [SerializeField] private CutsceneActor player;
    [SerializeField] private CutsceneActor friend;

    [Header("카메라 포인트")]
    [SerializeField] private Transform cameraPoint1;

    [Header("플레이어 포인트")]
    [SerializeField] private Transform playerPoint;

    [Header("기타")]
    [SerializeField] private GameObject cutSceneText1;
    [SerializeField] private CanvasGroup fadeScreen;
    [SerializeField] private CanvasGroup cutScene1;
    [SerializeField] private StageClear stageClear;
    [SerializeField] private CutSceneSkip cutSceneSkip;

    private Animator playerAnim;

    private void Start()
    {
        playerAnim = player.GetComponent<Animator>();
        fadeScreen.alpha = 1f;
        cutScene1.alpha = 0f;
        cutSceneText1.SetActive(false);
        ScriptStart().Forget();
    }

    private async UniTask ScriptStart()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(1f));
        cutSceneText1.SetActive(true);
        await UniTask.Delay(TimeSpan.FromSeconds(3f));
        cutSceneText1.SetActive(false);

        await UniTask.Delay(TimeSpan.FromSeconds(2f));

        _ = fadeScreen.DOFade(0f, 0.5f);

        MovePlayerAsync().Forget();

        cutsceneCamera.SetMoveSpeed(1f);
        await cutsceneCamera.FocusAsync(cameraPoint1);

        await dialogue.ShowAsync();
        await dialogue.ShowLineAsync(friend.Speaker, PortraitSide.Right,
            "늦었네.");
        await dialogue.ShowLineAsync(friend.Speaker, PortraitSide.Right,
            "오늘은 몇 마리 잡았어?");
        await dialogue.ShowLineAsync(player.Speaker, PortraitSide.Left,
            "열네 마리. 탑 근처에 더 늘었어.");
        await dialogue.ShowLineAsync(friend.Speaker, PortraitSide.Right,
            "탑 생기고부터 계속 늘잖아.");  
        await dialogue.ShowLineAsync(player.Speaker, PortraitSide.Left,
            "그러니까 내가 나가는 거야. 안 나가면 누가 해."); 
        await dialogue.ShowLineAsync(friend.Speaker, PortraitSide.Right,
            "소매 찢어진 건 알고 있어?");  
        await dialogue.ShowLineAsync(player.Speaker, PortraitSide.Left,
            "...소매만 찢어졌어.");
        await dialogue.HideAsync();

        await UniTask.Delay(TimeSpan.FromSeconds(1f));
        await cutScene1.DOFade(1f, 0.5f);

        await dialogue.ShowAsync();
        await dialogue.ShowLineAsync(friend.Speaker, PortraitSide.Right,
            "이거 네 거. 오늘은 짠 게 먹고 싶다고 했잖아.");
        await dialogue.ShowLineAsync(player.Speaker, PortraitSide.Left,
            "말한 적 없는데.");
        await dialogue.ShowLineAsync(friend.Speaker, PortraitSide.Right,
            "얼굴에 써 있었어.");  
        await dialogue.ShowLineAsync(player.Speaker, PortraitSide.Left,
            "...너는 어떻게 맨날 그렇게 알아."); 
        await dialogue.ShowLineAsync(friend.Speaker, PortraitSide.Right,
            "같이 산 지 오래됐으니까.");  
        await dialogue.HideAsync();

        await cutScene1.DOFade(0f, 0.5f);
        await UniTask.Delay(TimeSpan.FromSeconds(1f));

        await dialogue.ShowAsync();
        await dialogue.ShowLineAsync(friend.Speaker, PortraitSide.Right,
            "있잖아. 나 저 탑 올라가 보고 싶어.");
        await dialogue.ShowLineAsync(player.Speaker, PortraitSide.Left,
            "또 그 소리.");
        await dialogue.ShowLineAsync(player.Speaker, PortraitSide.Left,
            "이번 달에만 몇 번째야.");
        await dialogue.ShowLineAsync(player.Speaker, PortraitSide.Left,
            "들어간 사람 중에 돌아온 사람 봤어? 들어가면 찾지도 않는 게 마을 규칙이야.");
        await dialogue.ShowLineAsync(friend.Speaker, PortraitSide.Right,
            "알아. 그래도 가 보고 싶어.");  
        await dialogue.ShowLineAsync(player.Speaker, PortraitSide.Left,
            "하... 도대체 왜 올라가고 싶은 건데? 이유라도 말해 봐.");  
        await dialogue.HideAsync();    

        if (cutSceneSkip.StageLoadingStart) return;

        stageClear.EventStart();
    }

    private async UniTask MovePlayerAsync()
    {
        playerAnim.SetBool("isWalk", true);
        await player.MoveXAsync(playerPoint.position.x, 7f);

        if (playerAnim != null)
        {
            playerAnim.SetBool("isWalk", false);
        }
    }
}
