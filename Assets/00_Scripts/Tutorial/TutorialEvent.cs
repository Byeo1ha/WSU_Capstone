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
    [SerializeField] private int maxNormalAttackSubject = 14;
    [SerializeField] private TMP_Text normalAttackSubjectText;
    [SerializeField] private TutorialDummy tutorialDummy;

    [SerializeField] private PlayerNormalAttack playerNormalAttack;

    private int normalAttackCount;
    private bool isFinish;
    
    private enum TutorialPhase
    {
        NormalAttack,
        AirAttack
    }

    private TutorialPhase tutorialPhase;

    private void Awake()
    {
        HideSubjectBorder();
        tutorialPhase = TutorialPhase.NormalAttack;

        normalAttackSubjectText
            .SetText("● 기본 공격 ({0} / {1})", normalAttackCount, maxNormalAttackSubject);
    }

    private void Start()
    {
        guideBorder.alpha = 0f;
        normalAttackSubjectText.color = Color.white;
        
        if (!tutorial) return;

        TutorialScript().Forget();
    }

    private void OnEnable()
    {
        playerNormalAttack.OnNormalAttackStarted
            .Subscribe(_ => UpdateSubjectNormalAttack())
            .AddTo(this);
    }

    private void HideSubjectBorder()
    {
        subjectBorder.anchoredPosition = new Vector2(170f, 0f);
    }

    private void ShowSubjectBorder()
    {
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

        Debug.Log("다음 미션");
        

    }

    private async UniTask TutorialFinish()
    {
        while (true)
        {
            if (isFinish)
            {
                isFinish = false;
                return;
            }
            
            await UniTask.NextFrame();
        }
    }

    private void UpdateSubjectNormalAttack()
    {
        if (tutorialPhase != TutorialPhase.NormalAttack) return;

        normalAttackCount++;

        normalAttackSubjectText
            .SetText("● 기본 공격 ({0} / {1})", normalAttackCount, maxNormalAttackSubject);

        if (normalAttackCount >= maxNormalAttackSubject)
        {
            normalAttackSubjectText.color = Color.green;
            tutorialPhase = TutorialPhase.AirAttack;

            HideSubjectBorder();
            isFinish = true;
        }
    }
}
