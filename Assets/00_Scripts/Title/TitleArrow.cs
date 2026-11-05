using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class TitleArrow : MonoBehaviour
{
    [SerializeField] private TitleBtnEnter startButton;
    [SerializeField] private RectTransform startPoint;

    [SerializeField] private TitleBtnEnter optionButton;
    [SerializeField] private RectTransform optionPoint;

    [SerializeField] private TitleBtnEnter quitButton;
    [SerializeField] private RectTransform quitPoint;

    [SerializeField] private float moveSpeed = 0.25f;

    private RectTransform rectTransform;
    private Tween tween;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        startButton.OnAction += MoveStartBtnPoint;
        optionButton.OnAction += MoveOptionBtnPoint;
        quitButton.OnAction += MoveQuitBtnPoint;
    }

    private void OnDisable()
    {
        startButton.OnAction -= MoveStartBtnPoint;
        optionButton.OnAction -= MoveOptionBtnPoint;
        quitButton.OnAction -= MoveQuitBtnPoint;
    }

    private void MoveStartBtnPoint()
    {
        tween?.Kill();
        tween = rectTransform
            .DOMoveY(startPoint.position.y, moveSpeed)
            .SetLink(gameObject);
    }

    private void MoveOptionBtnPoint()
    {
        tween?.Kill();
        tween = rectTransform
            .DOMoveY(optionPoint.position.y, moveSpeed)
            .SetLink(gameObject);
    }

    private void MoveQuitBtnPoint()
    {
        tween?.Kill();
        tween = rectTransform
            .DOMoveY(quitPoint.position.y, moveSpeed)
            .SetLink(gameObject);
    }
}
