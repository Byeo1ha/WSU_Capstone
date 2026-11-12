using Cysharp.Threading.Tasks;
using DG.Tweening;
using R3;
using UnityEngine;
using VContainer;

public class PauseManager : MonoBehaviour
{
    [SerializeField] private string titleSceneName = "01_TitleScene";
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private CanvasGroup titleCheckPanel;

    public bool IsActive { get; private set; }
    public bool CanPause { get; private set; } = true;

    private InputManager inputManager;
    private OptionUI optionUI;
    private SceneFlowService sceneFlowService;

    public void SetPauseTime(bool value) 
        => CanPause = value;

    private bool isTitleCheck;

    [Inject]
    public void Construct(
        InputManager inputManager,
        OptionUI optionUI,
        SceneFlowService sceneFlowService)
    {
        this.inputManager = inputManager;
        this.optionUI = optionUI;
        this.sceneFlowService = sceneFlowService;
    }

    private void Awake()
    {
        pausePanel.SetActive(false);
        titleCheckPanel.gameObject.SetActive(false);
    }

    private void Start()
    {
        inputManager.OnPause
            .Where(_ => CanPause)
            .Subscribe(_ => InputHandler())
            .AddTo(this);

        inputManager.OnCancel
            .Where(_ => isTitleCheck)
            .Subscribe(_ => TitleDeny())
            .AddTo(this);
    }

    private void InputHandler()
    {
        if (!IsActive) OpenPauseUI();
        else ClosePauseUI();
    }

    private void OpenPauseUI()
    {
        IsActive = true;
        Time.timeScale = 0f;
        pausePanel.SetActive(true);
    }

    private void ClosePauseUI()
    {
        if (optionUI.IsBusy || titleCheckPanel.gameObject.activeSelf) return;

        IsActive = false;
        Time.timeScale = 1f;
        pausePanel.SetActive(false);
        titleCheckPanel.gameObject.SetActive(false);
        titleCheckPanel.alpha = 1f;
    }

    public void ResumeBtn()
    {
        ClosePauseUI();
    }

    public void OptionBtn()
    {
        if (!IsActive || optionUI.IsBusy || titleCheckPanel.gameObject.activeSelf) return;

        optionUI.OpenUI();
    }

    public void TitleBtn()
    {
        if (!IsActive || optionUI.IsBusy || titleCheckPanel.gameObject.activeSelf) return;

        titleCheckPanel.transform.DOKill();
        titleCheckPanel.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
        titleCheckPanel.gameObject.SetActive(true);
        titleCheckPanel.alpha = 1f;

        isTitleCheck = true;

        titleCheckPanel
            .transform
            .DOScale(1f, 0.5f)
            .SetEase(Ease.OutBack)
            .SetLink(titleCheckPanel.gameObject)
            .SetUpdate(titleCheckPanel);
    }

    public void TitleConfirm()
    {
        sceneFlowService
            .LoadSceneAsync(
                titleSceneName, 
                SceneFlowService.LoadingSceneType.Long
            )
            .Forget();

        Time.timeScale = 1f;
    }

    public void TitleDeny()
    {
        isTitleCheck = false;
        titleCheckPanel.transform.DOKill();
        titleCheckPanel
            .DOFade(0f, 0.5f)
            .SetLink(titleCheckPanel.gameObject)
            .SetUpdate(titleCheckPanel)
            .OnComplete(() => titleCheckPanel.gameObject.SetActive(false));
    }
}
