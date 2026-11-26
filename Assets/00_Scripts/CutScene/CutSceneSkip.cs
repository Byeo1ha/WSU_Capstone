using DG.Tweening;
using UnityEngine;

public class CutSceneSkip : MonoBehaviour
{
    [SerializeField] private GameObject alert;
    [SerializeField] private CanvasGroup alertFadeScreen;
    [SerializeField] private StageClear skipStage;

    private bool stageLoadingStart;
    public bool StageLoadingStart => stageLoadingStart;

    private void Awake()
    {
        alertFadeScreen.alpha = 0f;
        alertFadeScreen.blocksRaycasts = false;
        alert.SetActive(false);
    }

    public void SkipBtn()
    {
        Debug.Log("hello");
        alert.transform.DOKill();
        alert.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
        alert.SetActive(true);

        alertFadeScreen.blocksRaycasts = true;
        alertFadeScreen
            .DOFade(0.8f, 0.5f)
            .SetLink(alertFadeScreen.gameObject)
            .SetUpdate(alertFadeScreen);
        
        Time.timeScale = 0f;

        alert
            .transform
            .DOScale(1f, 0.5f)
            .SetLink(alert)
            .SetUpdate(alert)
            .SetEase(Ease.OutBack);
    }

    public void SkipDeny()
    {
        alert.transform.DOKill();
        Time.timeScale = 1f;

        alertFadeScreen
            .DOFade(0f, 0.5f)
            .SetLink(alertFadeScreen.gameObject)
            .SetUpdate(alertFadeScreen)
            .OnComplete(() => alertFadeScreen.blocksRaycasts = false);

        alert
            .transform
            .DOScale(0f, 0.2f)
            .SetLink(alert)
            .SetUpdate(alert)
            .OnComplete(() => alert.SetActive(false));
    }

    public void SkipConfirm()
    {
        Time.timeScale = 1f;

        stageLoadingStart = true;
        skipStage.EventStart();
    }

    public void SkipConfirmLongLoading()
    {
        Time.timeScale = 1f;

        stageLoadingStart = true;
        skipStage.EventStartLongTime();
    }
}
