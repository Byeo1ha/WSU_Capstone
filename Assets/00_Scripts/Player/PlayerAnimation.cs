using System;
using R3;
using UnityEngine;
using VContainer;

[RequireComponent(typeof(PlayerState))]
[RequireComponent(typeof(Animator))]
public class PlayerAnimation : MonoBehaviour
{
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
        if (moveInput == 0) anim.SetBool("isRun", false);
        else anim.SetBool("isRun", true);

        GroundCheck();
    }

    private void GroundCheck()
    {
        if (playerState.IsGrounded) anim.SetBool("isJump", false);
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
