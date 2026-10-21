using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

public class Title : MonoBehaviour
{
    [SerializeField] private string startSceneName = "SampleScene";
    private SceneFlowService sceneFlowService;

    [Inject]
    public void Construct(SceneFlowService sceneFlowService)
    {
        this.sceneFlowService = sceneFlowService;
    }

    public void StartBtn()
    {
        sceneFlowService.LoadSceneAsync(startSceneName).Forget();
    }

    public void QuitBtn()
    {
        Application.Quit();
    }
}
