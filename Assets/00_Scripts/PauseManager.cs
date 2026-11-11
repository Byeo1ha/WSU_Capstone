using R3;
using UnityEngine;
using VContainer;

public class PauseManager : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;

    public bool IsActive { get; private set; }
    public bool CanPause { get; private set; } = true;

    private InputManager inputManager;
    private OptionUI optionUI;

    public void SetPauseTime(bool value) 
        => CanPause = value;

    private bool blockCancel;

    [Inject]
    public void Construct(
        InputManager inputManager,
        OptionUI optionUI)
    {
        this.inputManager = inputManager;
        this.optionUI = optionUI;
    }

    private void Awake()
    {
        pausePanel.SetActive(false);
    }

    private void Start()
    {
        inputManager.OnPause
            .Where(_ => CanPause)
            .Subscribe(_ => InputHandler())
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
        IsActive = false;
        Time.timeScale = 1f;
        pausePanel.SetActive(false);
    }

    public void ResumeBtn()
    {
        ClosePauseUI();
    }

    public void OptionBtn()
    { 
        optionUI.OpenUI();
    }

}
