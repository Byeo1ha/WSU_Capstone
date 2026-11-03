using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using R3;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VContainer;

[RequireComponent(typeof(CanvasGroup))]
public class UIDie : MonoBehaviour
{
    [SerializeField] private string titleSceneName = "01_TitleScene";
    [SerializeField] private Image blackBorder;
    private CanvasGroup gameOverPanel;
    
    private PlayerAttributes playerAttributes;
    private SceneFlowService sceneFlowService;

    [Inject]
    public void Construct(
        PlayerAttributes playerAttributes,
        SceneFlowService sceneFlowService)
    {
        this.playerAttributes = playerAttributes;
        this.sceneFlowService = sceneFlowService;
    }

    private void Awake()
    {
        gameOverPanel = GetComponent<CanvasGroup>();
        gameOverPanel.interactable = false;  

        gameOverPanel.alpha = 0f;
        blackBorder.color = new Color(0f, 0f, 0f, 0f);
    }

    private void Start()
    {
        playerAttributes.Hp
            .Where(hp => hp <= 0)
            .Subscribe(_ => Die().Forget())
            .AddTo(this);
    }

    private async UniTask Die()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(1f),
            cancellationToken: this.GetCancellationTokenOnDestroy());

        DieEvent();
    }

    private void DieEvent()
    {
        blackBorder.DOFade(0.5f, 0.5f);
        gameOverPanel.interactable = true;
        gameOverPanel.alpha = 1f;
        gameOverPanel.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
        gameOverPanel.transform
            .DOScale(1f, 0.5f)
            .SetEase(Ease.OutBack);
    }

    public void RespawnBtn()
    {
        sceneFlowService
            .LoadSceneAsync(SceneManager.GetActiveScene().name)
            .Forget();

        playerAttributes.ResetHp();
    }

    public void TitleBtn()
    {
        sceneFlowService
            .LoadSceneAsync(titleSceneName)
            .Forget();
    }
}
