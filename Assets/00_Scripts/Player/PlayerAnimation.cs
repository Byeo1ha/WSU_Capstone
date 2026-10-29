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
[RequireComponent(typeof(PlayerAirAttack))]
[RequireComponent(typeof(PlayerDodge))]
public class PlayerAnimation : MonoBehaviour
{
    private Rigidbody2D rigid;
    private InputManager inputManager;
    private PlayerState playerState;
    private PlayerNormalAttack playerNormalAttack;
    private PlayerLowAttack playerLowAttack;
    private PlayerAttackFeedback playerAttackFeedback;
    private PlayerAirAttack playerAirAttack;
    private PlayerDodge playerDodge;
    private PlayerHighAttack playerHighAttack;

    private IDisposable normalAttackSubscription;
    private IDisposable airAttackSubscription;

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
        playerHighAttack = GetComponent<PlayerHighAttack>();
        playerAttackFeedback = GetComponent<PlayerAttackFeedback>();
        playerAirAttack = GetComponent<PlayerAirAttack>();
        playerDodge = GetComponent<PlayerDodge>();
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
            
        playerDodge.OnDodged
            .Subscribe(_ => StartDodgeAnim())
            .AddTo(this);

        playerHighAttack.OnHighAttackStarted
            .Subscribe(_ => StartHighAttackAnim())
            .AddTo(this);
    }

    private void OnEnable()
    {  
        normalAttackSubscription = playerNormalAttack.OnNormalAttackStarted
            .Subscribe(StartNormalAttackAnim)
            .AddTo(this);

        airAttackSubscription = playerAirAttack.OnAirAttackStarted
            .Subscribe(StartAirAttackAnim)
            .AddTo(this);
    }

    private void OnDisable()
    {
        normalAttackSubscription?.Dispose();
        normalAttackSubscription = null;

        airAttackSubscription?.Dispose();
        airAttackSubscription = null;
    }

    private void FixedUpdate()
    {
        StopAirAttackAnim();
        RunAnimCheck();
        GroundCheck();
        FallCheck();
    }

    // 달리는 상태를 체크하고 Run 애니메이션을 재생하는 함수
    private void RunAnimCheck()
    {
        anim.SetBool("isRun", playerState.IsGrounded && moveInput != 0);
    }

    // 점프 애니메이션을 재생하는 함수
    private void StartJumpAnim()
    {
        anim.SetBool("isJump", true);
    }

    // 지면에 닿아있는 상태일 경우 Jump 애니메이션 재생을 취소하는 함수
    private void GroundCheck()
    {
        if (playerState.IsGrounded) anim.SetBool("isJump", false);
    }

    // 하단 공격 애니메이션을 재생하는 함수
    private void StartLowAttackAnim()
    {
        anim.SetBool("isLowAttack", true);
    }

    // 애니메이션 키 프레임에 넣는 하단 공격 애니메이션 재생을 취소하는 함수
    public void StopLowAttackAnim()
    {
        anim.SetBool("isLowAttack", false);
        playerLowAttack.EndLowAttack();
    }

    // 추락을 감지하면 Fall 애니메이션을 재생하는 함수
    private void FallCheck()
    {
        if (playerState.IsDamaging)
        {
            anim.SetBool("isFall", false);
            return;
        }

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

    // 콤보 수치에 따라 Normal Attack 애니메이션을 재생하는 함수
    private void StartNormalAttackAnim(int comboIndex)
    {
        anim.SetInteger("isNormalAttackCombo", comboIndex);
        anim.SetTrigger("normalAttack");
    }

    // 콤보 수치에 따라 Air Attack 애니메이션을 재생하는 함수
    private void StartAirAttackAnim(int comboIndex)
    {
        anim.SetBool("isAirAttack", true);
        anim.SetInteger("isAirAttackCombo", comboIndex);
        anim.SetTrigger("airAttack");
    }

    public void StopAirAttackAnim()
    {
        anim.SetBool("isAirAttack", playerState.IsAirAttacking);
    }

    private void StartDodgeAnim()
    {
        anim.SetTrigger("dodge");
    }

    private void StartHighAttackAnim()
    {
        anim.SetTrigger("highAttack");
    }

    public void StartDamagedAnim()
    {
        anim.SetBool("isFall", false);
        anim.SetBool("isJump", false);
        anim.SetBool("isAirAttack", false);
        anim.SetBool("isLowAttack", false);
        anim.ResetTrigger("normalAttack");
        anim.ResetTrigger("airAttack");
        anim.ResetTrigger("highAttack");
        anim.ResetTrigger("dodge");
        anim.SetTrigger("damaged");
    }
}
