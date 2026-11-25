using Cysharp.Threading.Tasks;
using UnityEngine;

public class Act2Script : MonoBehaviour
{
    [Header("기본")]
    [SerializeField] private CutsceneDialogue dialogue;
    [SerializeField] private CutsceneBackgroundFade backgroundFade;
    [SerializeField] private SpriteRenderer changeBackground;
    [SerializeField] private CutsceneCamera cutsceneCamera;
    [SerializeField] private CutsceneCameraZoom cameraZoom;
    [SerializeField] private CutsceneCameraShake cameraShake;

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
        
    }
}
