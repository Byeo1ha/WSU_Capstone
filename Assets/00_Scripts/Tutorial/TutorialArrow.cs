using DG.Tweening;
using UnityEngine;

public class TutorialArrow : MonoBehaviour
{
    private Vector3 originalVec;

    private void OnEnable()
    {
        originalVec = transform.position;
        transform.DOMoveY(transform.position.y + 0.5f, 1f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetLink(gameObject);
    }

    private void OnDisable()
    {
        transform.position = originalVec;
    }
}
