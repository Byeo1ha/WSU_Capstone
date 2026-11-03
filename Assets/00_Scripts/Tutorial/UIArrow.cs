using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class UIArrow : MonoBehaviour
{
    private RectTransform rectTransform;
    private Vector3 originalRect;
    
    private enum HighlightDirection
    {
        Up,
        Down,
        Left,
        Right
    }

    [SerializeField] private HighlightDirection highlightDirection;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        originalRect = rectTransform.anchoredPosition;

        ArrowAction();
    }

    private void OnDisable()
    {
        rectTransform.anchoredPosition = originalRect;
    }

    private void ArrowAction()
    {
        float x = rectTransform.anchoredPosition.x;
        float y = rectTransform.anchoredPosition.y;
        switch (highlightDirection)
        {
            case HighlightDirection.Up:
                y = rectTransform.anchoredPosition.y - 30f;
                break;
            case HighlightDirection.Down:
                y = rectTransform.anchoredPosition.y + 30f;
                break;
            case HighlightDirection.Left:
                x = rectTransform.anchoredPosition.x + 30f;
                break;
            case HighlightDirection.Right:
                x = rectTransform.anchoredPosition.x - 30f;
                break;
            default:
                return;
        }

        rectTransform.DOAnchorPos(new Vector3(x, y, 0f), 1f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetLink(gameObject);
    }
}
