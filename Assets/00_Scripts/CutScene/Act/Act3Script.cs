using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class Act3Script : MonoBehaviour
{
    [Header("기본")]
    [SerializeField] private CutsceneDialogue dialogue;
    [SerializeField] private CutsceneCamera cutsceneCamera;
    [SerializeField] private CutsceneCameraZoom cameraZoom;
    [SerializeField] private CutsceneCameraShake cameraShake;

    [Header("배우")]
    [SerializeField] private CutsceneSpeakerData player;
    [SerializeField] private CutsceneSpeakerData eve;

    [Header("텍스트")]
    [SerializeField] private GameObject sceneText1;

    [Header("기타")]
    [SerializeField] private CanvasGroup fadeScreen;
    [SerializeField] private GameObject cutScene1;
    [SerializeField] private GameObject cutScene2;
    [SerializeField] private GameObject cutScene3;
    [SerializeField] private StageClear stageClear;

    private void Awake()
    {
        fadeScreen.alpha = 0f;
        sceneText1.SetActive(false);
        cutScene1.SetActive(true);
        cutScene2.SetActive(false);
        cutScene3.SetActive(false);
    }

    private void Start()
    {
        ScriptStart().Forget();
    }

    private async UniTask ScriptStart()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(1f));

        await dialogue.ShowAsync();
        await dialogue.ShowLineAsync(eve, PortraitSide.Right,
            "비밀.");
        await dialogue.ShowLineAsync(player, PortraitSide.Left,
            "열 번도 넘게 물어봤는데 열 번 다 비밀이야?");
        await dialogue.ShowLineAsync(eve, PortraitSide.Right,
            "열한 번째에도 비밀이야.");
        await dialogue.HideAsync();

        await UniTask.Delay(TimeSpan.FromSeconds(1f));
        await fadeScreen.DOFade(1f, 0.5f);
        await UniTask.Delay(TimeSpan.FromSeconds(2f));
        cutScene1.SetActive(false);
        cutScene2.SetActive(true);

        await dialogue.ShowAsync();
        await dialogue.ShowLineAsync(eve, PortraitSide.Right,
            "리아.");
        await dialogue.ShowLineAsync(eve, PortraitSide.Right,
            "만약에..");
        await dialogue.ShowLineAsync(eve, PortraitSide.Right,
            "진짜 만약에...");
        await dialogue.ShowLineAsync(eve, PortraitSide.Right,
            "내가 올라가게 되면, 너도 같이 가 줄래?");
        await dialogue.HideAsync();

        await UniTask.Delay(TimeSpan.FromSeconds(1f));

        await dialogue.ShowAsync();
        await dialogue.ShowLineAsync(player, PortraitSide.Left,
            "미쳤어? 죽어도 안 가.");
        await dialogue.ShowLineAsync(eve, PortraitSide.Right,
            "그럴 줄 알았어.");
        await dialogue.ShowLineAsync(player, PortraitSide.Left,
            "...그럴 줄 알았으면 왜 물어봐.");
        await dialogue.ShowLineAsync(eve, PortraitSide.Right,
            "그냥. 한 번은 물어보고 싶어서.");
        await dialogue.HideAsync();

        await fadeScreen.DOFade(0f, 0.5f);

        await dialogue.ShowAsync();
        await dialogue.ShowLineAsync(eve, PortraitSide.Right,
            "길 잃으면 이 별 보고 찾아와. 어디서든.");
        await dialogue.ShowLineAsync(player, PortraitSide.Left,
            "길 잃을 일 없거든.");
        await dialogue.HideAsync();

        await UniTask.Delay(TimeSpan.FromSeconds(1f));
        await fadeScreen.DOFade(1f, 0.5f);
        await UniTask.Delay(TimeSpan.FromSeconds(2f));
        cutScene2.SetActive(false);
        cutScene3.SetActive(true);

        sceneText1.SetActive(true);
        await UniTask.Delay(TimeSpan.FromSeconds(3f));
        sceneText1.SetActive(false);

        await dialogue.ShowAsync();
        await dialogue.ShowLineAsync(player, PortraitSide.Left,
            "...진짜로 간 거야?");
        await dialogue.HideAsync();
        
        await fadeScreen.DOFade(0f, 0.5f);

        await UniTask.Delay(TimeSpan.FromSeconds(0.5f));

        await dialogue.ShowAsync();
        await dialogue.ShowLineAsync(player, PortraitSide.Left,
            "찾지도 않는 게 규칙이라며. 하나뿐인 친구를 규칙 때문에 그냥 두라고? 웃기시네.");
        await dialogue.ShowLineAsync(player, PortraitSide.Left,
            "말도 없이 가는 게 어딨어...");
        await dialogue.HideAsync();

        await UniTask.Delay(TimeSpan.FromSeconds(1f));
        await fadeScreen.DOFade(1f, 0.5f);
        await UniTask.Delay(TimeSpan.FromSeconds(1f));

        await dialogue.ShowAsync();
        await dialogue.ShowLineAsync(player, PortraitSide.Left,
            "그놈의 비밀, 얼굴 보고 들을 거야.");
        await dialogue.HideAsync();

        stageClear.EventStart();
    }
}
