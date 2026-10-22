using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class TutorialEvent : MonoBehaviour
{
    [Header("튜토리얼 활성화 여부")]
    [SerializeField] private bool tutorial = true;

    [Header("키 가이드 등장 속도")]
    [SerializeField] private float completeTime = 2f;

    [Header("키 가이드 보더")]
    [SerializeField] private CanvasGroup moveBorder;
    [SerializeField] private CanvasGroup normalAttackBorder;
    [SerializeField] private CanvasGroup airAttackBorder;

    [Header("우측 목표 보더")]
    [SerializeField] private RectTransform subjectBorder;

    [SerializeField] private int maxNormalAttackSubject = 8;
    [SerializeField] private int maxAirAttackSubject = 3;


    [FormerlySerializedAs("normalAttackSubjectText")]
    [SerializeField] private TMP_Text subjectText;

    [Header("더미")]
    [SerializeField] private TutorialDummy tutorialDummy;

    private int subjectCount;
    private bool isFinish;
    private bool isMissionActive;
    
    private enum TutorialPhase
    {
        Move,
        NormalAttack,
        AirAttack,
        Completed
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
        moveBorder.alpha = 0f;
        normalAttackBorder.alpha = 0f;
        airAttackBorder.alpha = 0f;
        subjectText.color = Color.white;
        
        if (!tutorial) return;

        if (tutorialDummy == null)
        {
            Debug.LogError("튜토리얼 더미를 Inspector에 연결해야 합니다.", this);
            return;
        }

        tutorialDummy.OnAttackHit
            .Where(_ => tutorial && isActiveAndEnabled && isMissionActive)
            .Subscribe(HandleDummyHit)
            .AddTo(this);

        TutorialScript().Forget();
    }

    private void OnDestroy()
    {
        subjectBorder.DOKill();
        normalAttackBorder.DOKill();
    }

    // 현재 미션과 더미에 명중한 공격 종류가 일치하면 진행도를 갱신하는 함수
    private void HandleDummyHit(AttackHitbox.AttackType attackType)
    {
        switch (tutorialPhase)
        {
            case TutorialPhase.NormalAttack:
                if (attackType == AttackHitbox.AttackType.Normal)
                    UpdateSubjectNormalAttack();
                break;
            case TutorialPhase.AirAttack:
                if (attackType == AttackHitbox.AttackType.Air)
                    UpdateSubjectAirAttack();
                break;
        }
    }

    // 미션 패널을 숨기는 연출이 끝날 때까지 기다리는 함수
    private async UniTask HideSubjectBorder()
    {
        subjectBorder.DOKill();
        await subjectBorder.DOAnchorPos(new Vector2(170f, 0f), 1f)
            .AsyncWaitForCompletion()
            .AsUniTask()
            .AttachExternalCancellation(this.GetCancellationTokenOnDestroy());
    }

    // 미션 패널을 표시하는 연출을 시작하는 함수
    private void ShowSubjectBorder()
    {
        subjectBorder.DOKill();
        subjectBorder.DOAnchorPos(new Vector2(-170f, 0f), 1f)
            .SetEase(Ease.OutCubic);
    }

    // 일반 공격과 공중 공격 미션을 순서대로 진행하는 함수
    private async UniTask TutorialScript()
    {
        await UniTask.Delay(
            TimeSpan.FromSeconds(1f),
            cancellationToken: this.GetCancellationTokenOnDestroy());

        moveBorder.DOFade(1f, completeTime);
        BeginMission(TutorialPhase.Move);
        
        await TutorialFinish();
        moveBorder.DOFade(0f, completeTime);
        await HideSubjectBorder();

        normalAttackBorder.DOFade(1f, completeTime);
        BeginMission(TutorialPhase.NormalAttack);

        await TutorialFinish();
        normalAttackBorder.DOFade(0f, completeTime);
        await HideSubjectBorder();

        airAttackBorder.DOFade(1f, completeTime);
        BeginMission(TutorialPhase.AirAttack);

        await TutorialFinish();
        airAttackBorder.DOFade(0f, completeTime);
        await HideSubjectBorder();

        tutorialPhase = TutorialPhase.Completed;
        Debug.Log("튜토리얼 완료");
    }

    // 미션 단계와 횟수, 완료 상태를 초기화하고 패널을 표시한 뒤 집계를 시작하는 함수
    private void BeginMission(TutorialPhase phase)
    {
        tutorialPhase = phase;
        subjectCount = 0;
        isFinish = false;
        subjectText.color = Color.white;

        if (phase == TutorialPhase.Move)
            subjectText.SetText("● 더미 앞으로 이동해보세요.");
        else if (phase == TutorialPhase.NormalAttack)
            subjectText.SetText("● 기본 공격 ({0} / {1})", subjectCount, maxNormalAttackSubject);
        else if (phase == TutorialPhase.AirAttack)
            subjectText.SetText("● 공중 공격 ({0} / {1})", subjectCount, maxAirAttackSubject);

        ShowSubjectBorder();
        isMissionActive = true;
    }

    // 현재 미션의 목표 달성까지 기다리는 함수
    private async UniTask TutorialFinish()
    {
        await UniTask.WaitUntil(() => isFinish, 
            cancellationToken: this.GetCancellationTokenOnDestroy());

        isFinish = false;
    }

    public void FinishMoveTutorial()
    {
        if (tutorialPhase != TutorialPhase.Move) return;

        CompleteMission().Forget();
    }

    // 일반 공격 명중 횟수를 갱신하고 목표 달성 여부를 확인하는 함수
    private void UpdateSubjectNormalAttack()
    {
        if (tutorialPhase != TutorialPhase.NormalAttack) return;

        subjectCount++;

        subjectText
            .SetText("● 기본 공격 ({0} / {1})", subjectCount, maxNormalAttackSubject);

        if (subjectCount >= maxNormalAttackSubject)
        {
            CompleteMission().Forget();
        }
    }

    // 공중 공격 명중 횟수를 갱신하고 목표 달성 여부를 확인하는 함수
    private void UpdateSubjectAirAttack()
    {
        if (tutorialPhase != TutorialPhase.AirAttack) return;

        subjectCount++;

        subjectText
            .SetText("● 공중 공격 ({0} / {1})", subjectCount, maxAirAttackSubject);

        if (subjectCount >= maxAirAttackSubject)
        {
            CompleteMission().Forget();
        }
    }

    // 목표 달성 후 추가 집계를 중지하고 완료 상태를 표시하는 함수
    private async UniTask CompleteMission()
    {
        isMissionActive = false;
        subjectText.color = Color.green;

        await UniTask.Delay(TimeSpan.FromSeconds(1f),
            cancellationToken: this.GetCancellationTokenOnDestroy());

        isFinish = true;
    }
}
