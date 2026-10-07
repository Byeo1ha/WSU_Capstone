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
                Attack();
            }

            return;
        }

        if (!playerState.IsAirAttacking || !isAirComboActive || playerState.IsGrounded)
            return;

        if (downInput || !canQueueNextAttack || isNextAttackQueued || !HasNextComboAttack())
            return;

        isNextAttackQueued = true;
        
    }

    private void Attack()
    {
        if (downInput || playerState.IsGrounded) return;

        comboIndex = 0;
        isAirComboActive = !playerState.IsGrounded && !downInput;
        isComboGraceActive = false;

        playerState.StartAirAttack();
        HorizontalAttack();
    }

    private void HorizontalAttack()
    {
        canQueueNextAttack = false;
        isNextAttackQueued = false;

        //Debug.Log($"진행 중인 Index: {comboIndex}");

        attackHitboxSpawner.Spawn(attackHitboxPool, offset);

        attackStepVersion++;
        AirCorrection().Forget();
        TestEndAttack(attackStepVersion).Forget();
    }

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
    private async UniTask TestEndAttack(int version)
    {
        await UniTask.Delay(
            TimeSpan.FromSeconds(0.4f), 
            cancellationToken: this.GetCancellationTokenOnDestroy());
        
        if (version != attackStepVersion)
            return;

        EndAttack();
    }

    public void EndAttack()
    {
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
        playerState.StopAirAttack();
    }
}
