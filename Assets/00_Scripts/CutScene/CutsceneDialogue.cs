using Cysharp.Threading.Tasks;
using DG.Tweening;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class CutsceneDialogue : MonoBehaviour
{
    [SerializeField] private CanvasGroup textBox;
    [SerializeField] private TMP_Text speakerName;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private GameObject leftPortraitBox;
    [SerializeField] private Image leftPortrait;
    [SerializeField] private GameObject rightPortraitBox;
    [SerializeField] private Image rightPortrait;
    [Min(0f)] [SerializeField] private float characterInterval = 0.03f;

    private InputManager inputManager;
    private bool isTyping;
    private bool isWaiting;
    private bool nextRequested;

    [Inject]
    public void Construct(InputManager inputManager)
    {
        this.inputManager = inputManager;
    }

    private void Awake()
    {
        textBox.alpha = 0f;
        textBox.interactable = false;
        textBox.blocksRaycasts = false;
        leftPortraitBox.SetActive(false);
        rightPortraitBox.SetActive(false);
    }

    private void Start()
    {
        inputManager.OnInteract
            .Where(_ => isActiveAndEnabled)
            .Subscribe(_ => HandleInteract())
            .AddTo(this);
    }

    private void HandleInteract()
    {
        if (!isWaiting) return;

        if (isTyping)
        {
            isTyping = false;
            dialogueText.maxVisibleCharacters = int.MaxValue;
        }
        else
        {
            nextRequested = true;
        }
    }

    public async UniTask ShowAsync()
    {
        speakerName.SetText("");
        dialogueText.SetText("");
        leftPortraitBox.SetActive(false);
        rightPortraitBox.SetActive(false);
        await textBox.DOFade(1f, 0.2f).SetLink(gameObject).AsyncWaitForCompletion();
    }

    public async UniTask HideAsync()
    {
        await textBox.DOFade(0f, 0.5f).SetLink(gameObject).AsyncWaitForCompletion();
    }

    public async UniTask ShowLineAsync(CutsceneSpeakerData speaker, bool isLeft, string text)
    {
        speakerName.SetText(speaker.DisplayName);
        leftPortraitBox.SetActive(isLeft);
        rightPortraitBox.SetActive(!isLeft);
        if (isLeft) leftPortrait.sprite = speaker.Portrait;
        else rightPortrait.sprite = speaker.Portrait;

        dialogueText.SetText(text);
        dialogueText.maxVisibleCharacters = 0;
        dialogueText.ForceMeshUpdate();
        int characterCount = dialogueText.textInfo.characterCount;
        float elapsed = 0f;
        isTyping = true;
        isWaiting = true;
        nextRequested = false;

        while (isTyping)
        {
            elapsed += Time.deltaTime;
            int visibleCount = characterCount;
            if (characterInterval > 0f)
                visibleCount = Mathf.FloorToInt(elapsed / characterInterval) + 1;

            dialogueText.maxVisibleCharacters = Mathf.Min(visibleCount, characterCount);
            if (visibleCount >= characterCount) isTyping = false;
            await UniTask.NextFrame(cancellationToken: this.GetCancellationTokenOnDestroy());
        }

        await UniTask.WaitUntil(() => nextRequested,
            cancellationToken: this.GetCancellationTokenOnDestroy());
        isWaiting = false;
    }
}
