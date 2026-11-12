using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

public class Title : MonoBehaviour
{
    [SerializeField] private string startSceneName;

    private SceneFlowService sceneFlowService;
    private OptionUI optionUI;

    [Inject]
    public void Construct(
        SceneFlowService sceneFlowService,
        OptionUI optionUI)
    {
        this.sceneFlowService = sceneFlowService;
        this.optionUI = optionUI;
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
        optionUI.OpenUI();
    }

    public void QuitBtn()
    {
        Application.Quit();
    }
}
