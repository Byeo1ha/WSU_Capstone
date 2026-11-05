using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

public class Title : MonoBehaviour
{
    [SerializeField] private string startSceneName;

    private SceneFlowService sceneFlowService;

    [Inject]
    public void Construct(SceneFlowService sceneFlowService)
    {
        this.sceneFlowService = sceneFlowService;
    }

    public void StartBtn()
    {
        sceneFlowService.LoadSceneAsync(startSceneName,
            SceneFlowService.LoadingSceneType.Long
        )
        .Forget();
    }

    public void OptionBtn()
    {
        
    }

    public void QuitBtn()
    {
        Application.Quit();
    }
}
