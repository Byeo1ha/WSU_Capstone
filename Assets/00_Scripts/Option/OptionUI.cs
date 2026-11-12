using DG.Tweening;
using R3;
using UnityEngine;
using VContainer;

[RequireComponent(typeof(CanvasGroup))]
public class OptionUI : MonoBehaviour
{
    [SerializeField] private GameObject screenWindow;
    [SerializeField] private GameObject soundWindow;
    [SerializeField] private GameObject saveConfirmationPopup;

    private CanvasGroup optionWindow;
    private VolumeSlider[] volumeSliders;
    private Tween windowTween;
    private bool isOpen;
    private bool isConfirmingClose;
    
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
        volumeSliders = soundWindow.GetComponentsInChildren<VolumeSlider>(true);

        if (saveConfirmationPopup != null)
        {
            saveConfirmationPopup.SetActive(false);
        }

        optionWindow.alpha = 0f;
        optionWindow.interactable = false;
        optionWindow.blocksRaycasts = false;
    }

    public void OpenUI()
    {
        if (!isActiveAndEnabled || isOpen) return;

        isOpen = true;
        windowTween?.Kill();
        OnScreenClassClick();
        optionWindow.interactable = true;
        optionWindow.blocksRaycasts = true;

        windowTween = optionWindow.DOFade(1f, 0.5f)
            .SetUpdate(true)
            .SetLink(gameObject);
    }

    public void CloseUI()
    {
        if (!isActiveAndEnabled || !isOpen || isConfirmingClose) return;

        foreach (VolumeSlider volumeSlider in volumeSliders)
        {
            if (!volumeSlider.HasUnsavedChanges) continue;

            if (saveConfirmationPopup == null)
            {
                Debug.LogError("저장 확인 팝업을 OptionUI에 연결해 주세요.", this);
                return;
            }

            isConfirmingClose = true;
            saveConfirmationPopup.SetActive(true);
            return;
        }

        CloseWindow();
    }

    public void ApplySettings()
    {
        if (!isOpen || isConfirmingClose) return;

        SaveVolumes();
    }

    public void SaveAndClose()
    {
        if (!isOpen || !isConfirmingClose) return;

        SaveVolumes();
        CloseWindow();
    }

    public void DiscardAndClose()
    {
        if (!isOpen || !isConfirmingClose) return;

        RestoreVolumes();
        CloseWindow();
    }

    private void SaveVolumes()
    {
        foreach (VolumeSlider volumeSlider in volumeSliders)
        {
            volumeSlider.SaveVolume();
        }

        PlayerPrefs.Save();
    }

    private void RestoreVolumes()
    {
        foreach (VolumeSlider volumeSlider in volumeSliders)
        {
            if (volumeSlider.HasUnsavedChanges) volumeSlider.RestoreVolume();
        }
    }

    private void CloseWindow()
    {
        isOpen = false;
        HideConfirmationPopup();
        windowTween?.Kill();
        optionWindow.interactable = false;
        optionWindow.blocksRaycasts = false;

        windowTween = optionWindow.DOFade(0f, 0.5f)
            .SetUpdate(true)
            .SetLink(gameObject);
    }

    private void HideConfirmationPopup()
    {
        isConfirmingClose = false;
        if (saveConfirmationPopup != null) saveConfirmationPopup.SetActive(false);
    }

    private void OnDisable()
    {
        windowTween?.Kill();
        RestoreVolumes();
        HideConfirmationPopup();
        isOpen = false;
        optionWindow.alpha = 0f;
        optionWindow.interactable = false;
        optionWindow.blocksRaycasts = false;
    }

    public void OnScreenClassClick()
    {
        if (isConfirmingClose) return;

        screenWindow.SetActive(true);
        soundWindow.SetActive(false);
    }

    public void OnSoundClassClick()
    {
        if (isConfirmingClose) return;

        screenWindow.SetActive(false);
        soundWindow.SetActive(true);
    }
}
