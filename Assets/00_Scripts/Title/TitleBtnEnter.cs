using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TitleBtnEnter : MonoBehaviour, 
    IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image borderImage;
    
    public event Action OnAction;

    private Tween tween;

    private void Awake()
    {
        borderImage.color =
            new Color(
                borderImage.color.r,
                borderImage.color.g,
                borderImage.color.b,
                0f
            );
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        tween?.Kill();
        tween = borderImage.DOFade(1f, 0.5f).SetLink(gameObject);
        OnAction?.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tween?.Kill();
        tween = borderImage.DOFade(0f, 0.5f).SetLink(gameObject);
    }
}
