using System;
using R3;
using UnityEngine;
using VContainer;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerState))]
[RequireComponent(typeof(PlayerGroundCheck))]
[RequireComponent(typeof(PlayerAirAttack))]
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerMove : MonoBehaviour
{
    [Header("플레이어의 이동 속도")]
    [SerializeField] private float maxSpeed = 8f; //최고 속도
    [SerializeField] private float acceleration = 45f; //목표 속도까지 빨라지는 속도
    [SerializeField] private float deceleration = 35f; //목표 속도까지 느려지는 속도
    [SerializeField] private float turnSpeed = 70f; //반대 방향으로 전환되는 속도

    private InputManager inputManager;

    private Rigidbody2D rigid;
    private PlayerState playerState;
    private PlayerGroundCheck playerGroundCheck;
    private PlayerAirAttack playerAirAttack;
    private SpriteRenderer spriteRenderer;

    private float moveInput;
    private float lockedAirSpeed;
    private float originalGravityScale;
    private float airAttackDirection;
    private float airDashRemainingTime;
    private bool wasAirAttack;

    [Inject]
    public void Construct(InputManager inputManager)
    {
        this.inputManager = inputManager;
    }
    
    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        playerState = GetComponent<PlayerState>();
        playerGroundCheck = GetComponent<PlayerGroundCheck>();
        playerAirAttack = GetComponent<PlayerAirAttack>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalGravityScale = rigid.gravityScale;
    }

    private void Start()
    {
        inputManager.MoveInput
            .Subscribe(input => moveInput = input)
            .AddTo(this);
    }

    private void OnEnable()
    {
        playerAirAttack.AirAttackStarted += StartAirDash;
    }

    private void FixedUpdate()
    {
        bool isActionLocked = playerState.IsActionLocked();
        bool isAirAttack = playerState.IsAirAttacking && !playerState.IsGrounded;

        if (isAirAttack && !wasAirAttack)
        {
            lockedAirSpeed = rigid.linearVelocity.x;
            if (playerAirAttack.MovementCase != AirAttackMovementCase.Original)
                rigid.gravityScale = 0f;
        }

        if (isAirAttack)
        {
            MaintainAirAttackMomentum();
        }
        else if (isActionLocked)
        {
            RestoreGravity();
            Move(0f);
        }
        else
        {
            RestoreGravity();
            Move(moveInput);
        }

        wasAirAttack = isAirAttack;
    }

    private void Move(float input)
    {
        float targetSpeed = input * maxSpeed;
        float speedChangeRate;

        if (input == 0)
        {
            speedChangeRate = deceleration;
        }
        else if (Mathf.Sign(input) != Math.Sign(rigid.linearVelocity.x) 
            && Mathf.Abs(rigid.linearVelocity.x) > 0.01f)
        {
            speedChangeRate = turnSpeed;
        }
        else
        {
            speedChangeRate = acceleration;
        }

        float xSpeed = Mathf.MoveTowards(
            rigid.linearVelocity.x, 
            targetSpeed, 
            speedChangeRate * Time.fixedDeltaTime);

        rigid.linearVelocity = new Vector2(xSpeed, rigid.linearVelocity.y);
    }

    private void MaintainAirAttackMomentum()
    {
        if (playerAirAttack.MovementCase == AirAttackMovementCase.Original)
        {
            rigid.linearVelocity = new Vector2(lockedAirSpeed, rigid.linearVelocity.y);
            return;
        }

        float xSpeed = GetAirAttackHorizontalSpeed();
        float ySpeed = IsHoverCase() ? 0f : -playerAirAttack.SlowFallSpeed;

        rigid.linearVelocity = new Vector2(xSpeed, ySpeed);
    }

    private float GetAirAttackHorizontalSpeed()
    {
        switch (playerAirAttack.MovementCase)
        {
            case AirAttackMovementCase.SlowFallNormalDeceleration:
            case AirAttackMovementCase.HoverNormalDeceleration:
                lockedAirSpeed = Mathf.MoveTowards(
                    lockedAirSpeed,
                    0f,
                    deceleration * Time.fixedDeltaTime);
                return lockedAirSpeed;

            case AirAttackMovementCase.SlowFallSlowDeceleration:
            case AirAttackMovementCase.HoverSlowDeceleration:
                lockedAirSpeed = Mathf.MoveTowards(
                    lockedAirSpeed,
                    0f,
                    playerAirAttack.SlowHorizontalDeceleration * Time.fixedDeltaTime);
                return lockedAirSpeed;

            case AirAttackMovementCase.SlowFallDash:
            case AirAttackMovementCase.HoverDash:
                return GetAirDashSpeed();

            default:
                return lockedAirSpeed;
        }
    }

    private bool IsHoverCase()
    {
        return playerAirAttack.MovementCase == AirAttackMovementCase.HoverNormalDeceleration ||
               playerAirAttack.MovementCase == AirAttackMovementCase.HoverSlowDeceleration ||
               playerAirAttack.MovementCase == AirAttackMovementCase.HoverDash;
    }

    private void StartAirDash()
    {
        if (playerAirAttack.MovementCase != AirAttackMovementCase.SlowFallDash &&
            playerAirAttack.MovementCase != AirAttackMovementCase.HoverDash)
            return;

        airAttackDirection = spriteRenderer.flipX ? -1f : 1f;
        airDashRemainingTime = playerAirAttack.DashDuration;
        lockedAirSpeed = 0f;
        rigid.linearVelocity = new Vector2(0f, rigid.linearVelocity.y);
    }

    private float GetAirDashSpeed()
    {
        if (airDashRemainingTime <= 0f || playerAirAttack.DashSpeed <= 0f)
            return 0f;

        airDashRemainingTime = Mathf.Max(
            0f,
            airDashRemainingTime - Time.fixedDeltaTime);

        return airAttackDirection * playerAirAttack.DashSpeed;
    }

    private void RestoreGravity()
    {
        if (rigid.gravityScale != originalGravityScale)
            rigid.gravityScale = originalGravityScale;
    }

    private void OnDisable()
    {
        playerAirAttack.AirAttackStarted -= StartAirDash;
        RestoreGravity();
        airDashRemainingTime = 0f;
        wasAirAttack = false;
    }
}
