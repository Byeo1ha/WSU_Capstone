using Cysharp.Threading.Tasks;
using UnityEngine;

public class Act1Script : MonoBehaviour
{
    [Header("기본")]
    [SerializeField] private CutsceneDialogue dialogue;
    [SerializeField] private CutsceneCamera cutsceneCamera;
    [SerializeField] private CutsceneCameraZoom cameraZoom;
    [SerializeField] private CutsceneCameraShake cameraShake;
    [SerializeField] private StageClear stageClear;

    [Header("배우")]
    [SerializeField] private CutsceneSpeakerData narrator;

    [Header("카메라 포인트")]
    [SerializeField] private Transform cameraPoint1;

    private void Start()
    {
        ScriptStart().Forget();
    }

    private async UniTask ScriptStart()
    {
        dialogue.SetCantInteract(true);
        
        cutsceneCamera.SetMoveSpeed(0.5f);
        cutsceneCamera.FocusAsync(cameraPoint1).Forget();
        await dialogue.ShowAsync();
        await dialogue.ShowLineAsync(narrator, PortraitSide.None, 
            "아홉 해 전 어느 아침, 마을 북쪽 밭 한가운데에 탑이 서 있었다.", autoAdvanceDelay: 2f);

        await dialogue.ShowLineAsync(narrator, PortraitSide.None, 
            "밭 주인은 눈을 세 번 비볐고, 이장은 지도를 펼쳐 봤지만, 설명해 주는 사람은 없었다.", autoAdvanceDelay: 2f);

        cameraShake.Shake(0.5f, 0.05f);
        await dialogue.ShowLineAsync(narrator, PortraitSide.None, 
            "탑의 문을 넘은 사람은 아무도 돌아오지 않았다.", autoAdvanceDelay: 2f);

        await dialogue.ShowLineAsync(narrator, PortraitSide.None, 
            "다만 문 앞에 서면, 안쪽에서 이름을 부르는 소리가 들린다고들 했다.", autoAdvanceDelay: 2f);

        stageClear.EventStart();
    }
}
