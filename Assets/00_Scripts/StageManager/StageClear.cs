using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

public class StageClear : MonoBehaviour
{
    [SerializeField] private string nextSceneName = "Actor1";
    [SerializeField] private float nextSceneTime = 3f;

    private SceneFlowService sceneFlowService;

    [Inject]
    public void Construct(SceneFlowService sceneFlowService)
    {
        this.sceneFlowService = sceneFlowService;
    }

    public void EventStart()
    {
        EventStartAsync(SceneFlowService.LoadingSceneType.Short).Forget();
    }

    public void EventStartLongTime()
    {
        EventStartAsync(SceneFlowService.LoadingSceneType.Long).Forget();
    }

    private async UniTask EventStartAsync(SceneFlowService.LoadingSceneType type)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(nextSceneTime));

        sceneFlowService.LoadSceneAsync(nextSceneName, type).Forget();
    }
}
