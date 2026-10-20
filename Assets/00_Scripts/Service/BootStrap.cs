using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

public class BootStrap : MonoBehaviour
{
    [SerializeField] private string nextSceneName;

    private SceneFlowService sceneFlowService;

    [Inject]
    public void Construct(SceneFlowService sceneFlowService)
    {
        this.sceneFlowService = sceneFlowService;
    }

    private void Start()
    {
        bool bootStrap;

        bootStrap = RootLifetimeScopeCheck();

        if (!bootStrap) Application.Quit();

        sceneFlowService.LoadSceneAsync(nextSceneName).Forget();
    }

    // RootLifetimeScope 체크용 함수
    private bool RootLifetimeScopeCheck()
    {
        RootLifetimeScope lifetimeScope = 
            GameObject.Find("RootLifetimeScope(Clone)").GetComponent<RootLifetimeScope>();

        if (lifetimeScope == null)
        {
            Debug.LogWarning("RootLifetimeScope가 발견되지 않았습니다.");
            return false;
        }

        return true;
    }
}
