using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class TutorialUIArrow : MonoBehaviour
{
    private RectTransform rectTransform;
    private Vector3 originalRect;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        originalRect = rectTransform.anchoredPosition;

        rectTransform.DOAnchorPosX(
            rectTransform.anchoredPosition.x - 30f, 1f
            )
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetLink(gameObject);
    }

    private void OnDisable()
    {
        rectTransform.anchoredPosition = originalRect;
    }
}
