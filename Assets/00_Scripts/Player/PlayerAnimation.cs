using System;
using R3;
using UnityEngine;
using VContainer;

[RequireComponent(typeof(PlayerState))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PlayerNormalAttack))]
[RequireComponent(typeof(PlayerLowAttack))]
[RequireComponent(typeof(PlayerAttackFeedback))]
public class PlayerAnimation : MonoBehaviour
{
    private Rigidbody2D rigid;
    private InputManager inputManager;
    private PlayerState playerState;
    private PlayerNormalAttack playerNormalAttack;
    private PlayerLowAttack playerLowAttack;
    private PlayerAttackFeedback playerAttackFeedback;

    private IDisposable normalAttackSubscription;

    private Animator anim;

    private float moveInput;

    [Inject]
    public void Construct(InputManager inputManager)
    {
        this.inputManager = inputManager;
    }

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        playerState = GetComponent<PlayerState>();
        playerNormalAttack = GetComponent<PlayerNormalAttack>();
        playerLowAttack = GetComponent<PlayerLowAttack>();
        playerAttackFeedback = GetComponent<PlayerAttackFeedback>();
    }

    private void Start()
    {
        inputManager.MoveInput
            .Subscribe(moveInput => this.moveInput = moveInput)
            .AddTo(this);

        inputManager.OnJump
            .Subscribe(_ => StartJumpAnim())
            .AddTo(this);

        playerLowAttack.OnLowAttackStarted
            .Subscribe(_ => StartLowAttackAnim())
            .AddTo(this);
    }

    private void OnEnable()
    {  
        normalAttackSubscription = playerNormalAttack.OnNormalAttackStarted
            .Subscribe(StartNormalAttackAnim)
            .AddTo(this);
    }

    private void OnDisable()
    {
        normalAttackSubscription?.Dispose();
        normalAttackSubscription = null;
    }

    private void FixedUpdate()
    {
        RunAnimCheck();
        GroundCheck();
        FallCheck();
    }

    private void RunAnimCheck()
    {
        if (moveInput == 0) anim.SetBool("isRun", false);
        else anim.SetBool("isRun", true);
    }

    private void GroundCheck()
    {
        if (playerState.IsGrounded) anim.SetBool("isJump", false);
    }

    private void FallCheck()
    {
        if (playerAttackFeedback.IsHitStopping) return;

        if (rigid.linearVelocity.y < -0.3f)
        {
            anim.SetBool("isFall", true);
            anim.SetBool("isJump", false);
        }
        else
        {
            anim.SetBool("isFall", false);
        }
    }

    private void StartJumpAnim()
    {
        anim.SetBool("isJump", true);
    }

    private void StartNormalAttackAnim(int comboIndex)
    {
        anim.SetInteger("isNormalAttackCombo", comboIndex);
        anim.SetTrigger("normalAttack");
    }

    private void StartLowAttackAnim()
    {
        anim.SetBool("isLowAttack", true);
    }

    //애니메이션 클립 전용 함수
    public void StopLowAttackAnim()
    {
        anim.SetBool("isLowAttack", false);
        playerLowAttack.EndAttack();
    }
}
