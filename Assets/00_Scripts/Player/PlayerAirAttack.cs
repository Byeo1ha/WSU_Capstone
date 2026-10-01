using System;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using VContainer;

public enum AirAttackMovementCase
{
    Original,
    SlowFallNormalDeceleration,
    SlowFallSlowDeceleration,
    SlowFallDash,
    HoverNormalDeceleration,
    HoverSlowDeceleration,
    HoverDash
}

[RequireComponent(typeof(PlayerState))]
[RequireComponent(typeof(AttackHitboxSpawner))]
public class PlayerAirAttack : MonoBehaviour
{
    private const int MaxComboCount = 3;

    [Header("공격 종료 후 콤보 유예시간")]
    [SerializeField] private float comboGraceDuration = 0.4f;

    [Header("Air Attack Timing")]
    [SerializeField, Min(0f)] private float attackDuration = 0.4f;
    [SerializeField, Min(0f)] private float comboInputOpenDelay = 0.12f;

    [Header("생성 위치 보정")]
    [SerializeField] private float offset = 2.5f;

    [Header("Air Attack Movement")]
    [SerializeField] private AirAttackMovementCase movementCase;
    [SerializeField, Min(0f)] private float slowFallSpeed = 2f;
    [SerializeField, Min(0f)] private float horizontalDeceleration = 10f;
    [SerializeField, Min(0f)] private float dashSpeed = 12f;
    [SerializeField, Min(0f)] private float dashDuration = 0.12f;

    public AirAttackMovementCase MovementCase => movementCase;
    public float SlowFallSpeed => slowFallSpeed;
    public float SlowHorizontalDeceleration => horizontalDeceleration;
    public float DashSpeed => dashSpeed;
    public float DashDuration => dashDuration;

    public event Action AirAttackStarted;

    private InputManager inputManager;
    private AttackHitboxPool attackHitboxPool;
    private AttackHitboxSpawner attackHitboxSpawner;

    private PlayerState playerState;

    private bool upInput;
    private int comboIndex;
    private bool isAirComboActive;
    private bool canQueueNextAttack;
    private bool isNextAttackQueued;
    private bool isComboGraceActive;
    private float comboGraceEndTime;
    private int attackStepVersion;

    [Inject]
    public void Construct(
        InputManager inputManager,
        [Key(PoolKey.AirAttack)]
        AttackHitboxPool attackHitboxPool)
    {
        this.inputManager = inputManager;
        this.attackHitboxPool = attackHitboxPool;
    }

    private void Awake()
    {
        playerState = GetComponent<PlayerState>();
        attackHitboxSpawner = GetComponent<AttackHitboxSpawner>();
    }

    private void Start()
    {
        inputManager.OnNormalAttack
            .Subscribe(_ => HandleAttackInput())
            .AddTo(this);
        
        inputManager.UpInput
            .Subscribe(upInput => this.upInput = upInput)
            .AddTo(this);
    }

    private void HandleAttackInput()
    {
        if (!playerState.IsActionLocked())
        {
            if (isComboGraceActive &&
                Time.time <= comboGraceEndTime &&
                !playerState.IsGrounded &&
                !upInput &&
                HasNextComboAttack())
            {
                ContinueAirCombo();
            }
            else
            {
                Attack();
            }

            return;
        }

        if (!playerState.IsAirAttacking || !isAirComboActive || playerState.IsGrounded)
            return;

        if (upInput || !canQueueNextAttack || isNextAttackQueued || !HasNextComboAttack())
            return;

        isNextAttackQueued = true;
        
    }

    private void Attack()
    {
        if (upInput || playerState.IsGrounded) return;

        comboIndex = 0;
        isAirComboActive = !playerState.IsGrounded && !upInput;
        isComboGraceActive = false;

        playerState.StartAirAttack();
        HorizontalAttack();
    }

    private void HorizontalAttack()
    {
        canQueueNextAttack = false;
        isNextAttackQueued = false;

        Debug.Log($"진행 중인 Index: {comboIndex}");

        attackHitboxSpawner.Spawn(attackHitboxPool, offset);
        AirAttackStarted?.Invoke();

        attackStepVersion++;
        RunAttackStep(attackStepVersion).Forget();
    }

    public void OpenComboInput()
    {
        if (!playerState.IsAirAttacking || !isAirComboActive || playerState.IsGrounded)
            return;

        if (HasNextComboAttack())
            canQueueNextAttack = true;
    }

    private bool HasNextComboAttack()
    {
        return comboIndex + 1 < MaxComboCount;
    }

    private void ContinueAirCombo()
    {
        isComboGraceActive = false;
        comboIndex++;
        isAirComboActive = true;

        if (!playerState.IsAirAttacking)
            playerState.StartAirAttack();

        HorizontalAttack();
    }

    //테스트 전용 함수
    //캐릭터 공격 애니메이션을 받으면 마지막 프레임에 EndAttack함수를 실행시키게 할 것
    private async UniTask RunAttackStep(int version)
    {
        float inputOpenDelay = Mathf.Min(comboInputOpenDelay, attackDuration);

        if (inputOpenDelay > 0f)
        {
            await UniTask.Delay(
                TimeSpan.FromSeconds(inputOpenDelay),
                cancellationToken: this.GetCancellationTokenOnDestroy());
        }

        if (version != attackStepVersion)
            return;

        OpenComboInput();

        float remainingDuration = attackDuration - inputOpenDelay;

        if (remainingDuration > 0f)
        {
            await UniTask.Delay(
                TimeSpan.FromSeconds(remainingDuration),
                cancellationToken: this.GetCancellationTokenOnDestroy());
        }

        if (version != attackStepVersion)
            return;

        EndAttack();
    }

    public void EndAttack()
    {
        if (!playerState.IsAirAttacking)
            return;

        attackStepVersion++;
        canQueueNextAttack = false;

        if (isAirComboActive && isNextAttackQueued && !playerState.IsGrounded && HasNextComboAttack())
        {
            ContinueAirCombo();
            return;
        }

        if (isAirComboActive && !playerState.IsGrounded && HasNextComboAttack() && comboGraceDuration > 0f)
        {
            isComboGraceActive = true;
            comboGraceEndTime = Time.time + comboGraceDuration;
        }
        else
        {
            isComboGraceActive = false;
            comboIndex = 0;
        }

        isAirComboActive = false;
        isNextAttackQueued = false;
        playerState.StopAirAttack();
    }
}
