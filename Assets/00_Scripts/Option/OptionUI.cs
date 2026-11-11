using Cysharp.Threading.Tasks;
using DG.Tweening;
using R3;
using UnityEngine;
using VContainer;

[RequireComponent(typeof(CanvasGroup))]
public class OptionUI : MonoBehaviour
{
    [SerializeField] private GameObject screenWindow;
    [SerializeField] private GameObject soundWindow;

    private CanvasGroup optionWindow;
    
    private InputManager inputManager;

    [Inject]
    public void Construct(InputManager inputManager)
    {
        this.inputManager = inputManager;
    }

    private void Start()
    {
        inputManager.OnCancel
            .Subscribe(_ => CloseUI())
            .AddTo(this);
    }

    private void Awake()
    {
        optionWindow = GetComponent<CanvasGroup>();

        optionWindow.alpha = 0f;
        optionWindow.interactable = false;
        optionWindow.blocksRaycasts = false;
    }

    public void OpenUI()
    {
        UIOpenAsync().Forget();
    }

    private async UniTask UIOpenAsync()
    {
        OnScreenClassClick();
        optionWindow.interactable = true;
        optionWindow.blocksRaycasts = true;

        await optionWindow.DOFade(1f, 0.5f)
            .SetUpdate(true)
            .SetLink(gameObject);
    }

    public void CloseUI()
    {
        UICloseAsync().Forget();
    }

    private async UniTask UICloseAsync()
    {
        await optionWindow.DOFade(0f, 0.5f)
            .SetUpdate(true)
            .SetLink(gameObject);

        optionWindow.interactable = false;
        optionWindow.blocksRaycasts = false;
    }

    public void OnScreenClassClick()
    {
        screenWindow.SetActive(true);
        soundWindow.SetActive(false);
    }

    public void OnSoundClassClick()
    {
        screenWindow.SetActive(false);
        soundWindow.SetActive(true);
    }
}
