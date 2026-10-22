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
    [SerializeField] private CanvasGroup jumpBorder;
    [SerializeField] private CanvasGroup airAttackBorder;
    [SerializeField] private CanvasGroup highAttackBorder;
    [SerializeField] private CanvasGroup lowAttackBorder;
    [SerializeField] private CanvasGroup dodgeBorder;

    [Header("우측 목표 보더")]
    [SerializeField] private RectTransform subjectBorder;

    [SerializeField] private int maxNormalAttackSubject = 8;
    [SerializeField] private int maxAirAttackSubject = 3;
    [SerializeField] private int maxJumpSubject = 3;
    [SerializeField] private int maxHighAttackSubject = 3;
    [SerializeField] private int maxLowAttackSubject = 3;
    [SerializeField] private int maxDodgeSubject = 3;


    [FormerlySerializedAs("normalAttackSubjectText")]
    [SerializeField] private TMP_Text subjectText;

    [Header("더미")]
    [SerializeField] private TutorialDummy tutorialDummy;

    [Header("플레이어 행동 감지")]
    [SerializeField] private PlayerState playerState;
    [SerializeField] private PlayerDodge playerDodge;

    private int subjectCount;
    private bool isFinish;
    private bool isMissionActive;
    private bool wasJumping;
    
    private enum TutorialPhase
    {
        Move,
        NormalAttack,
        Jump,
        AirAttack,
        HighAttack,
        LowAttack,
        Dodge,
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
        if (jumpBorder != null) jumpBorder.alpha = 0f;
        if (highAttackBorder != null) highAttackBorder.alpha = 0f;
        if (lowAttackBorder != null) lowAttackBorder.alpha = 0f;
        if (dodgeBorder != null) dodgeBorder.alpha = 0f;
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

        if (playerDodge != null)
        {
            playerDodge.OnDodged
                .Where(_ => tutorial && isActiveAndEnabled && isMissionActive)
                .Subscribe(_ => UpdateSubjectDodge())
                .AddTo(this);
        }

        TutorialScript().Forget();
    }

    private void Update()
    {
        if (playerState == null) return;

        bool isJumping = playerState.IsJumping;

        if (tutorial && isMissionActive && isJumping && !wasJumping)
            UpdateSubjectJump();

        wasJumping = isJumping;
    }

    private void OnDestroy()
    {
        subjectBorder.DOKill();
        normalAttackBorder.DOKill();
        if (jumpBorder != null) jumpBorder.DOKill();
        if (highAttackBorder != null) highAttackBorder.DOKill();
        if (lowAttackBorder != null) lowAttackBorder.DOKill();
        if (dodgeBorder != null) dodgeBorder.DOKill();
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
            case TutorialPhase.HighAttack:
                if (attackType == AttackHitbox.AttackType.High)
                    UpdateSubjectHighAttack();
                break;
            case TutorialPhase.LowAttack:
                if (attackType == AttackHitbox.AttackType.Low)
                    UpdateSubjectLowAttack();
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

    // 우측 목표 보더를 함께 숨기고 키 가이드가 완전히 사라질 때까지 기다리는 함수
    private async UniTask HideKeyGuideBorder(CanvasGroup keyGuideBorder)
    {
        if (keyGuideBorder == null)
        {
            await HideSubjectBorder();
            return;
        }

        HideSubjectBorder().Forget();
        keyGuideBorder.DOKill();
        await keyGuideBorder.DOFade(0f, completeTime)
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

    // 이동, 기본 공격, 점프, 공중 공격, 상단 공격, 하단 공격, 회피 미션을 순서대로 진행하는 함수
    private async UniTask TutorialScript()
    {
        await UniTask.Delay(
            TimeSpan.FromSeconds(1f),
            cancellationToken: this.GetCancellationTokenOnDestroy());

        moveBorder.DOFade(1f, completeTime);
        BeginMission(TutorialPhase.Move);
        
        await TutorialFinish();
        await HideKeyGuideBorder(moveBorder);

        normalAttackBorder.DOFade(1f, completeTime);
        BeginMission(TutorialPhase.NormalAttack);

        await TutorialFinish();
        await HideKeyGuideBorder(normalAttackBorder);

        if (jumpBorder != null) jumpBorder.DOFade(1f, completeTime);
        BeginMission(TutorialPhase.Jump);

        await TutorialFinish();
        await HideKeyGuideBorder(jumpBorder);

        airAttackBorder.DOFade(1f, completeTime);
        BeginMission(TutorialPhase.AirAttack);

        await TutorialFinish();
        await HideKeyGuideBorder(airAttackBorder);

        if (highAttackBorder != null) highAttackBorder.DOFade(1f, completeTime);
        BeginMission(TutorialPhase.HighAttack);

        await TutorialFinish();
        await HideKeyGuideBorder(highAttackBorder);

        if (lowAttackBorder != null) lowAttackBorder.DOFade(1f, completeTime);
        BeginMission(TutorialPhase.LowAttack);

        await TutorialFinish();
        await HideKeyGuideBorder(lowAttackBorder);

        if (dodgeBorder != null) dodgeBorder.DOFade(1f, completeTime);
        BeginMission(TutorialPhase.Dodge);

        await TutorialFinish();
        await HideKeyGuideBorder(dodgeBorder);

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
        else if (phase == TutorialPhase.Jump)
            subjectText.SetText("● 점프 ({0} / {1})", subjectCount, maxJumpSubject);
        else if (phase == TutorialPhase.AirAttack)
            subjectText.SetText("● 공중 공격 ({0} / {1})", subjectCount, maxAirAttackSubject);
        else if (phase == TutorialPhase.HighAttack)
            subjectText.SetText("● 상단 공격 ({0} / {1})", subjectCount, maxHighAttackSubject);
        else if (phase == TutorialPhase.LowAttack)
            subjectText.SetText("● 하단 공격 ({0} / {1})", subjectCount, maxLowAttackSubject);
        else if (phase == TutorialPhase.Dodge)
            subjectText.SetText("● 회피 ({0} / {1})", subjectCount, maxDodgeSubject);

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

    // 실제 점프 시작 횟수를 갱신하고 목표 달성 여부를 확인하는 함수
    private void UpdateSubjectJump()
    {
        if (tutorialPhase != TutorialPhase.Jump) return;

        subjectCount++;

        subjectText.SetText("● 점프 ({0} / {1})", subjectCount, maxJumpSubject);

        if (subjectCount >= maxJumpSubject)
        {
            CompleteMission().Forget();
        }
    }

    // 상단 공격으로 더미에 명중한 횟수를 갱신하고 목표 달성 여부를 확인하는 함수
    private void UpdateSubjectHighAttack()
    {
        if (tutorialPhase != TutorialPhase.HighAttack) return;

        subjectCount++;

        subjectText.SetText("● 상단 공격 ({0} / {1})", subjectCount, maxHighAttackSubject);

        if (subjectCount >= maxHighAttackSubject)
        {
            CompleteMission().Forget();
        }
    }

    // 하단 공격으로 더미에 명중한 횟수를 갱신하고 목표 달성 여부를 확인하는 함수
    private void UpdateSubjectLowAttack()
    {
        if (tutorialPhase != TutorialPhase.LowAttack) return;

        subjectCount++;

        subjectText.SetText("● 하단 공격 ({0} / {1})", subjectCount, maxLowAttackSubject);

        if (subjectCount >= maxLowAttackSubject)
        {
            CompleteMission().Forget();
        }
    }

    // 실제 회피 시작 횟수를 갱신하고 목표 달성 여부를 확인하는 함수
    private void UpdateSubjectDodge()
    {
        if (tutorialPhase != TutorialPhase.Dodge) return;

        subjectCount++;

        subjectText.SetText("● 회피 ({0} / {1})", subjectCount, maxDodgeSubject);

        if (subjectCount >= maxDodgeSubject)
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
