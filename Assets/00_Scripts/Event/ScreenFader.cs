using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class ScreenFader : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeDuration = 0.5f;

    private void Awake()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
    }

    public async UniTask FadeOutAsync()
    {
        canvasGroup.blocksRaycasts = true;

        await canvasGroup.DOFade(1f, fadeDuration)
            .SetUpdate(true)
            .AsyncWaitForCompletion();
    }

    public async UniTask FadeInAsync()
    {
        await canvasGroup.DOFade(0f, fadeDuration)
            .SetUpdate(true)
            .AsyncWaitForCompletion();

        canvasGroup.blocksRaycasts = false;
    }
}
