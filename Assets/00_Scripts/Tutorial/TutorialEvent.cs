using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using VContainer;

public class TutorialEvent : MonoBehaviour
{
    private InputManager inputManager;

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

    [Header("튜토리얼 스킵")]
    [SerializeField] private GameObject skipWindow;
    [SerializeField] private CanvasGroup skipBtnCanvas;

    private int subjectCount;
    private bool isFinish;
    private bool isMissionActive;
    private bool wasJumping;

    private bool openSkipWindow;
    private CancellationTokenSource tutorialCancellation;
    
    public enum TutorialPhase
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

    public TutorialPhase GetTutorialPhase => tutorialPhase;

    [Inject]
    public void Construct(InputManager inputManager)
    {
        this.inputManager = inputManager;
    }

    private void Awake()
    {
        subjectBorder.anchoredPosition = new Vector2(170f, 0f);
        tutorialPhase = TutorialPhase.NormalAttack;

        subjectText
            .SetText("● 기본 공격 ({0} / {1})", subjectCount, maxNormalAttackSubject);

        skipBtnCanvas.alpha = 0f;
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

        tutorialCancellation = CancellationTokenSource.CreateLinkedTokenSource(
            this.GetCancellationTokenOnDestroy());
        TutorialScript().SuppressCancellationThrow().Forget();
        skipWindow.SetActive(false);
        skipBtnCanvas.DOFade(1f, 0.5f)
            .SetLink(skipBtnCanvas.gameObject);
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
        tutorialCancellation?.Cancel();
        tutorialCancellation?.Dispose();
        if (openSkipWindow && inputManager != null)
            inputManager.SetGameplayInputBlocked(false);
        HideTutorialUI(false);
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
            .SetLink(subjectBorder.gameObject)
            .AsyncWaitForCompletion()
            .AsUniTask()
            .AttachExternalCancellation(tutorialCancellation.Token);
    }

    // 우측 목표 보더를 함께 숨기고 키 가이드가 완전히 사라질 때까지 기다리는 함수
    private async UniTask HideKeyGuideBorder(CanvasGroup keyGuideBorder)
    {
        if (keyGuideBorder == null)
        {
            await HideSubjectBorder();
            return;
        }

        HideSubjectBorder().SuppressCancellationThrow().Forget();
        keyGuideBorder.DOKill();
        await keyGuideBorder.DOFade(0f, completeTime)
            .SetLink(keyGuideBorder.gameObject)
            .AsyncWaitForCompletion()
            .AsUniTask()
            .AttachExternalCancellation(tutorialCancellation.Token);
    }

    // 미션 패널을 표시하는 연출을 시작하는 함수
    private void ShowSubjectBorder()
    {
        subjectBorder.DOKill();
        subjectBorder.DOAnchorPos(new Vector2(-170f, 0f), 1f)
            .SetLink(subjectBorder.gameObject)
            .SetEase(Ease.OutCubic);
    }

    // 이동, 기본 공격, 점프, 공중 공격, 상단 공격, 하단 공격, 회피 미션을 순서대로 진행하는 함수
    private async UniTask TutorialScript()
    {
        await UniTask.Delay(
            TimeSpan.FromSeconds(1f),
            cancellationToken: tutorialCancellation.Token);

        moveBorder.DOFade(1f, completeTime)
            .SetLink(moveBorder.gameObject);
        BeginMission(TutorialPhase.Move);
        
        await TutorialFinish();
        await HideKeyGuideBorder(moveBorder);

        normalAttackBorder.DOFade(1f, completeTime)
            .SetLink(normalAttackBorder.gameObject);
        BeginMission(TutorialPhase.NormalAttack);

        await TutorialFinish();
        await HideKeyGuideBorder(normalAttackBorder);

        if (jumpBorder != null)
            jumpBorder.DOFade(1f, completeTime)
                .SetLink(jumpBorder.gameObject);
        BeginMission(TutorialPhase.Jump);

        await TutorialFinish();
        await HideKeyGuideBorder(jumpBorder);

        airAttackBorder.DOFade(1f, completeTime)
            .SetLink(airAttackBorder.gameObject);
        BeginMission(TutorialPhase.AirAttack);

        await TutorialFinish();
        await HideKeyGuideBorder(airAttackBorder);

        if (highAttackBorder != null)
            highAttackBorder.DOFade(1f, completeTime)
                .SetLink(highAttackBorder.gameObject);
        BeginMission(TutorialPhase.HighAttack);

        await TutorialFinish();
        await HideKeyGuideBorder(highAttackBorder);

        if (lowAttackBorder != null)
            lowAttackBorder.DOFade(1f, completeTime)
                .SetLink(lowAttackBorder.gameObject);
        BeginMission(TutorialPhase.LowAttack);

        await TutorialFinish();
        await HideKeyGuideBorder(lowAttackBorder);

        if (dodgeBorder != null)
            dodgeBorder.DOFade(1f, completeTime)
                .SetLink(dodgeBorder.gameObject);
        BeginMission(TutorialPhase.Dodge);

        await TutorialFinish();
        DisableSkipBtn();
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
            cancellationToken: tutorialCancellation.Token);

        isFinish = false;
    }

    public void FinishMoveTutorial()
    {
        if (!tutorial || !isMissionActive || tutorialPhase != TutorialPhase.Move) return;

        CompleteMission().SuppressCancellationThrow().Forget();
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
            CompleteMission().SuppressCancellationThrow().Forget();
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
            CompleteMission().SuppressCancellationThrow().Forget();
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
            CompleteMission().SuppressCancellationThrow().Forget();
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
            CompleteMission().SuppressCancellationThrow().Forget();
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
            CompleteMission().SuppressCancellationThrow().Forget();
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
            CompleteMission().SuppressCancellationThrow().Forget();
        }
    }

    // 목표 달성 후 추가 집계를 중지하고 완료 상태를 표시하는 함수
    private async UniTask CompleteMission()
    {
        if (!tutorial || !isMissionActive) return;

        isMissionActive = false;
        subjectText.color = Color.green;

        await UniTask.Delay(TimeSpan.FromSeconds(1f),
            cancellationToken: tutorialCancellation.Token);

        isFinish = true;
    }

    public void TutorialSkip()
    {
        if (!tutorial || tutorialPhase == TutorialPhase.Completed || openSkipWindow) return;

        inputManager.SetGameplayInputBlocked(true);
        openSkipWindow = true;

        skipWindow.transform.DOKill();
        skipWindow.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
        skipWindow.SetActive(true);

        skipWindow
            .transform
            .DOScale(1f, 0.5f)
            .SetLink(skipWindow)
            .SetEase(Ease.OutBack);
    }

    public void TutorialSkipDeny()
    {
        if (!openSkipWindow) return;

        openSkipWindow = false;
        inputManager.SetGameplayInputBlocked(false);

        skipWindow.transform.DOKill();
        skipWindow
            .transform
            .DOScale(0f, 0.2f)
            .SetLink(skipWindow)
            .OnComplete(() => skipWindow.SetActive(false));
    }

    public void TutorialSkipConfirm()
    {
        if (!tutorial) return;

        tutorial = false;
        isMissionActive = false;
        isFinish = false;
        tutorialPhase = TutorialPhase.Completed;

        tutorialCancellation?.Cancel();
        HideTutorialUI();

        if (openSkipWindow)
        {
            openSkipWindow = false;
            inputManager.SetGameplayInputBlocked(false);
        }

        DisableSkipBtn();
    }

    private void DisableSkipBtn()
    {
        skipBtnCanvas.interactable = false;
        
        skipBtnCanvas
            .DOFade(0f, 0.5f)
            .SetLink(skipBtnCanvas.gameObject)
            .OnComplete(
                () => 
                {
                    skipBtnCanvas.gameObject.SetActive(false);
                    tutorialPhase = TutorialPhase.Completed;
                }
            );
    }

    private void HideTutorialUI(bool animate = true)
    {
        HideKeyGuide(moveBorder, animate);
        HideKeyGuide(normalAttackBorder, animate);
        HideKeyGuide(jumpBorder, animate);
        HideKeyGuide(airAttackBorder, animate);
        HideKeyGuide(highAttackBorder, animate);
        HideKeyGuide(lowAttackBorder, animate);
        HideKeyGuide(dodgeBorder, animate);

        if (subjectBorder != null)
        {
            subjectBorder.DOKill();
            if (animate && subjectBorder.gameObject.activeInHierarchy)
            {
                subjectBorder.DOAnchorPos(new Vector2(170f, 0f), 1f)
                    .SetLink(subjectBorder.gameObject)
                    .OnComplete(() => subjectBorder.gameObject.SetActive(false));
            }
            else
            {
                subjectBorder.gameObject.SetActive(false);
            }
        }

        if (skipWindow != null)
        {
            skipWindow.transform.DOKill();
            if (animate && skipWindow.activeInHierarchy)
            {
                skipWindow.transform.DOScale(0f, 0.2f)
                    .SetLink(skipWindow)
                    .OnComplete(() => skipWindow.SetActive(false));
            }
            else
            {
                skipWindow.SetActive(false);
            }
        }
    }

    private void HideKeyGuide(CanvasGroup keyGuideBorder, bool animate)
    {
        if (keyGuideBorder == null) return;

        keyGuideBorder.DOKill();
        keyGuideBorder.interactable = false;
        keyGuideBorder.blocksRaycasts = false;

        if (animate && keyGuideBorder.gameObject.activeInHierarchy && keyGuideBorder.alpha > 0f)
        {
            keyGuideBorder.DOFade(0f, completeTime)
                .SetLink(keyGuideBorder.gameObject)
                .OnComplete(() => keyGuideBorder.gameObject.SetActive(false));
        }
        else
        {
            keyGuideBorder.alpha = 0f;
            keyGuideBorder.gameObject.SetActive(false);
        }
    }
}
