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
    private CanvasGroup confirmationGroup;
    private VolumeSlider[] volumeSliders;
    private Tween windowTween;
    private Tween confirmationTween;
    private bool isOpen;
    private bool isClosing;
    private bool isConfirmingClose;
    private int lastClosedFrame = -1;

    public bool IsBusy => isOpen || isClosing || lastClosedFrame == Time.frameCount;
    
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
            confirmationGroup = saveConfirmationPopup.GetComponent<CanvasGroup>();
            if (confirmationGroup == null)
            {
                confirmationGroup = saveConfirmationPopup.AddComponent<CanvasGroup>();
            }

            confirmationGroup.alpha = 0f;
            confirmationGroup.interactable = false;
            confirmationGroup.blocksRaycasts = false;
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
        isClosing = false;
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

            if (saveConfirmationPopup == null) return;

            ShowConfirmationPopup();
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
        isClosing = true;
        HideConfirmationPopup();
        windowTween?.Kill();
        optionWindow.interactable = false;
        optionWindow.blocksRaycasts = false;

        windowTween = optionWindow.DOFade(0f, 0.5f)
            .SetUpdate(true)
            .SetLink(gameObject)
            .OnComplete(() =>
            {
                isClosing = false;
                lastClosedFrame = Time.frameCount;
            });
    }

    private void ShowConfirmationPopup()
    {
        isConfirmingClose = true;
        confirmationTween?.Kill();
        confirmationGroup.alpha = 1f;
        confirmationGroup.interactable = true;
        confirmationGroup.blocksRaycasts = true;
        confirmationGroup.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
        saveConfirmationPopup.SetActive(true);

        confirmationTween = 
            confirmationGroup
                .transform
                .DOScale(1f, 0.5f)
                .SetEase(Ease.OutBack)
                .SetLink(gameObject)
                .SetUpdate(gameObject);
    }

    private void HideConfirmationPopup(bool immediate = false)
    {
        isConfirmingClose = false;
        if (saveConfirmationPopup == null) return;

        confirmationTween?.Kill();
        confirmationGroup.interactable = false;

        if (immediate || !saveConfirmationPopup.activeSelf)
        {
            confirmationGroup.alpha = 0f;
            confirmationGroup.blocksRaycasts = false;
            saveConfirmationPopup.SetActive(false);
            return;
        }

        confirmationTween = confirmationGroup.DOFade(0f, 0.5f)
            .SetUpdate(true)
            .SetLink(gameObject)
            .OnComplete(() =>
            {
                confirmationGroup.blocksRaycasts = false;
                saveConfirmationPopup.SetActive(false);
            });
    }

    private void OnDisable()
    {
        if (isOpen || isClosing) lastClosedFrame = Time.frameCount;

        windowTween?.Kill();
        RestoreVolumes();
        HideConfirmationPopup(true);
        isOpen = false;
        isClosing = false;
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
