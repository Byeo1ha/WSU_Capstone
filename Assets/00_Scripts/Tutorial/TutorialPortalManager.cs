using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class TutorialPortalManager : MonoBehaviour
{
    [SerializeField] private TutorialEvent tutorialEvent;
    [SerializeField] private Portal tutorialOutPortal;
    [SerializeField] private CanvasGroup completePortalGuide;

    private bool guideEnable;

    private void Awake()
    {
        tutorialOutPortal.gameObject.SetActive(false);
        completePortalGuide.alpha = 0f;
    }

    private void FixedUpdate()
    {
        if (tutorialEvent.GetTutorialPhase == TutorialEvent.TutorialPhase.Completed &&
            !guideEnable)
        {
            guideEnable = true;
            tutorialOutPortal.gameObject.SetActive(true);
            GuideShow().SuppressCancellationThrow().Forget();
        }
    }

    private async UniTask GuideShow()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(1f),
            cancellationToken: this.GetCancellationTokenOnDestroy());

        completePortalGuide.DOFade(1f, 0.5f)
            .SetLink(completePortalGuide.gameObject);
    }
}
