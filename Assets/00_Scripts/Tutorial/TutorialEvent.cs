using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using R3;
using TMPro;
using UnityEngine;

public class TutorialEvent : MonoBehaviour
{
    [SerializeField] private bool tutorial = true;
    [SerializeField] private float compeleteTime = 2f;
    [SerializeField] private CanvasGroup guideBorder;
    [SerializeField] private RectTransform subjectBorder;
    [SerializeField] private int maxNormalAttackSubject = 8;
    [SerializeField] private int maxAirAttackSubject = 3;
    [SerializeField] private TMP_Text subjectText;
    [SerializeField] private TutorialDummy tutorialDummy;

    [SerializeField] private PlayerNormalAttack playerNormalAttack;

    private int subjectCount;
    private bool isFinish;
    
    private enum TutorialPhase
    {
        NormalAttack,
        AirAttack
    }

    private TutorialPhase tutorialPhase;

    private void Awake()
    {
        subjectBorder.anchoredPosition = new Vector2(170f, 0f);
        tutorialPhase = TutorialPhase.NormalAttack;

        subjectText
            .SetText("● 기본 공격 ({0} / {1})", subjectCount, maxNormalAttackSubject);
    }

    private void Start()
    {
        playerNormalAttack.OnNormalAttackStarted
            .Subscribe(_ => UpdateSubjectNormalAttack())
            .AddTo(this);

        guideBorder.alpha = 0f;
        subjectText.color = Color.white;
        
        if (!tutorial) return;

        TutorialScript().Forget();
    }

    private async UniTask HideSubjectBorder()
    {
        subjectBorder.DOKill();
        await subjectBorder.DOAnchorPos(new Vector2(170f, 0f), 1f)
            .SetEase(Ease.OutCubic)
            .AsyncWaitForCompletion();
    }

    private void ShowSubjectBorder()
    {
        subjectBorder.DOKill();
        subjectBorder.DOAnchorPos(new Vector2(-170f, 0f), 1f)
            .SetEase(Ease.OutCubic);
    }

    private async UniTask TutorialScript()
    {
        await UniTask.Delay(
            TimeSpan.FromSeconds(1f),
            cancellationToken: this.GetCancellationTokenOnDestroy());

        guideBorder.DOFade(1f, compeleteTime);
        ShowSubjectBorder();

        await TutorialFinish();
        
        await HideSubjectBorder();

        ShowSubjectBorder();

        Debug.Log("다음 미션");
    }

    private async UniTask TutorialFinish()
    {
        await UniTask.WaitUntil(() => isFinish, 
            cancellationToken: this.GetCancellationTokenOnDestroy());

        isFinish = false;
    }

    private void UpdateSubjectNormalAttack()
    {
        if (tutorialPhase != TutorialPhase.NormalAttack) return;

        subjectCount++;

        subjectText
            .SetText("● 기본 공격 ({0} / {1})", subjectCount, maxNormalAttackSubject);

        if (subjectCount >= maxNormalAttackSubject)
        {
            subjectText.color = Color.green;
            tutorialPhase = TutorialPhase.AirAttack;

            isFinish = true;
        }
    }

    private void UpdateSubjectAirAttack()
    {
        if (tutorialPhase != TutorialPhase.AirAttack) return;

        subjectCount++;

        subjectText
            .SetText("● 공중 공격 ({0} / {1})", subjectCount, maxAirAttackSubject);

        if (subjectCount >= maxAirAttackSubject)
        {
            subjectText.color = Color.green;
            tutorialPhase = TutorialPhase.AirAttack;

            isFinish = true;
        }
    }
}
