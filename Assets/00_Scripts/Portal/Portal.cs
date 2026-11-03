using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using VContainer;

public class Portal : MonoBehaviour
{
    [SerializeField] private GameObject guideMsg;
    [SerializeField] private string nextSceneName;

    private SceneFlowService sceneFlowService;
    private (int layer1, int layer2) targetLayer;
    private bool isReady;
    private InputManager inputManager;

    //GameLifetimeScope에 Auto Inject Game Objects의 대상으로 지정해둘 것.
    [Inject]
    public void Construct(
        SceneFlowService sceneFlowService,
        InputManager inputManager)
    {
        this.sceneFlowService = sceneFlowService;
        this.inputManager = inputManager;
    }

    private void Awake()
    {
        targetLayer = 
            (LayerMask.NameToLayer("Player"),
            LayerMask.NameToLayer("Player_inv"));
        guideMsg.SetActive(false);
    }

    private void Start()
    {
        inputManager.OnInteract
            .Subscribe(_ => MoveNextScene())
            .AddTo(this);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == targetLayer.layer1 ||
            collision.gameObject.layer == targetLayer.layer2)
        {
            PortalReady(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == targetLayer.layer1 ||
            collision.gameObject.layer == targetLayer.layer2)
        {
            PortalReady(false);
        }
    }

    private void PortalReady(bool value)
    {
        isReady = value;

        guideMsg.SetActive(value);
    }

    private void MoveNextScene()
    {
        if (!isReady) return;

        sceneFlowService.LoadSceneAsync(nextSceneName).Forget();
    }
}   
