using System;
using R3;
using UnityEngine;
using VContainer;

[RequireComponent(typeof(PlayerState))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class PlayerAnimation : MonoBehaviour
{
    private Rigidbody2D rigid;
    private InputManager inputManager;
    private PlayerState playerState;
    private PlayerNormalAttack playerNormalAttack;
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
    }

    private void OnEnable()
    {
        if (playerNormalAttack == null) return;

        normalAttackSubscription = playerNormalAttack.OnNormalAttackStarted
            .Subscribe(StartNormalAttackAnim)
            .AddTo(this);
    }

    private void OnDisable()
    {
        normalAttackSubscription?.Dispose();
        normalAttackSubscription = null;
    }

    private void Start()
    {
        inputManager.MoveInput
            .Subscribe(moveInput => this.moveInput = moveInput)
            .AddTo(this);

        inputManager.OnJump
            .Subscribe(_ => StartJumpAnim())
            .AddTo(this);
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
}
