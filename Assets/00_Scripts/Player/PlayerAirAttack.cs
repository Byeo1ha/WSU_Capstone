using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using VContainer;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerState))]
[RequireComponent(typeof(AttackHitboxSpawner))]
public class PlayerAirAttack : MonoBehaviour
{
    private const int MaxComboCount = 3;

    [Header("공격 종료 후 콤보 유예시간")]
    [SerializeField] private float comboGraceDuration = 0.4f;

    [Header("생성 위치 보정")]
    [SerializeField] private float offset = 2.5f;

    [Header("체공 보정")]
    [SerializeField] private float xAirOffset = 0.2f;
    [SerializeField] private float yAirOffset = 0.05f;

    private readonly Subject<int> onAirAttackStarted = new();
    public Observable<int> OnAirAttackStarted => onAirAttackStarted;

    private InputManager inputManager;
    private AttackHitboxPool attackHitboxPool;
    private AttackHitboxSpawner attackHitboxSpawner;

    private Rigidbody2D rigid;
    private PlayerState playerState;

    private bool downInput;
    private int comboIndex;
    private bool isAirComboActive;
    private bool canQueueNextAttack;
    private bool isNextAttackQueued;
    private bool isComboGraceActive;
    private float comboGraceEndTime;
    private int attackStepVersion;

    private CancellationTokenSource cts;

    private float originalGravity;

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
        rigid = GetComponent<Rigidbody2D>();
        playerState = GetComponent<PlayerState>();
        attackHitboxSpawner = GetComponent<AttackHitboxSpawner>();

        originalGravity = rigid.gravityScale;
    }

    private void Start()
    {
        inputManager.OnNormalAttack
            .Subscribe(_ => HandleAttackInput())
            .AddTo(this);
        
        inputManager.DownInput
            .Subscribe(downInput => this.downInput = downInput)
            .AddTo(this);
    }

    private void OnDestroy()
    {
        onAirAttackStarted.Dispose();
    }

    // 키 입력을 받는 조건 필터링 함수
    private void HandleAttackInput()
    {
        if (!playerState.IsActionLocked())
        {
            if (isComboGraceActive &&
                Time.time <= comboGraceEndTime &&
                !playerState.IsGrounded &&
                !downInput &&
                HasNextComboAttack())
            {
                ContinueAirCombo();
            }
            else
            {
                TryAttack();
            }

            return;
        }

        if (!playerState.IsAirAttacking || !isAirComboActive || playerState.IsGrounded)
            return;

        if (downInput || !canQueueNextAttack || isNextAttackQueued || !HasNextComboAttack())
            return;

        isNextAttackQueued = true;
        
    }

    // 공격 실행을 시도하는 함수
    private void TryAttack()
    {
        if (downInput || playerState.IsGrounded) return;

        comboIndex = 0;
        isAirComboActive = !playerState.IsGrounded && !downInput;
        isComboGraceActive = false;

        playerState.StartAirAttack();
        HorizontalAttack();
    }

    // 수평 공격을 실제로 실행하는 함수
    private void HorizontalAttack()
    {
        canQueueNextAttack = false;
        isNextAttackQueued = false;

        onAirAttackStarted.OnNext(comboIndex);

        attackStepVersion++;
        AirCorrection().Forget();
    }

    public void OnAirAttackHitboxStart()
    {
        if (!playerState.IsAirAttacking) return;
        
        attackHitboxSpawner.Spawn(attackHitboxPool, offset);
    }

    // 공격 도중에 실행하는 공중 체공 함수
    private async UniTask AirCorrection()
    {
        cts = new CancellationTokenSource();

        rigid.gravityScale = 0f;

        while (true)
        {
            rigid.linearVelocity = new Vector2(
                rigid.linearVelocity.x * xAirOffset, 
                rigid.linearVelocity.y * yAirOffset);

            await UniTask.WaitForFixedUpdate(
                cancellationToken: cts.Token);
        }
    }

    // 다음 콤보 공격이 남아 있는지 확인하는 함수
    private bool HasNextComboAttack()
    {
        return comboIndex + 1 < MaxComboCount;
    }

    // 콤보 수치를 올리고 다음 콤보 공격을 실행하는 함수
    private void ContinueAirCombo()
    {
        isComboGraceActive = false;
        comboIndex++;
        isAirComboActive = true;

        if (!playerState.IsAirAttacking)
            playerState.StartAirAttack();

        HorizontalAttack();
    }

    // 애니메이션 키 프레임에 넣는 선입력 함수
    public void OpenComboInput()
    {
        if (!playerState.IsAirAttacking || !isAirComboActive || playerState.IsGrounded)
            return;

        if (HasNextComboAttack())
            canQueueNextAttack = true;
    }

    public void EndAirAttack()
    {
        EndAttackInternal(attackStepVersion);
    }

    // 애니메이션 키 프레임에 넣는 공격 종료 함수
    private void EndAttackInternal(int version)
    {
        if (version != attackStepVersion)
            return;

        if (!playerState.IsAirAttacking)
            return;

        cts?.Cancel();
        rigid.gravityScale = originalGravity;
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
        playerState.StopAction(PlayerActionState.AirAttack);
    }
}
