using System;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using VContainer;

[RequireComponent(typeof(PlayerState))]
[RequireComponent(typeof(AttackHitboxSpawner))]
public class PlayerNormalAttack : MonoBehaviour
{
    private const int MaxComboCount = 4;

    [Header("공격 종료 후 콤보 유예시간")]
    [SerializeField] private float comboGraceDuration = 0.4f;

    [Header("생성 위치 보정")]
    [SerializeField] private float offset = 2.5f;

    private InputManager inputManager;
    private AttackHitboxPool attackHitboxPool;
    private AttackHitboxSpawner attackHitboxSpawner;

    private PlayerState playerState;

    private bool upInput;
    private int comboIndex;
    private bool isGroundComboActive;
    private bool canQueueNextAttack;
    private bool isNextAttackQueued;
    private bool isComboGraceActive;
    private float comboGraceEndTime;
    private int attackStepVersion;

    [Inject]
    public void Construct(
        InputManager inputManager,
        [Key(PoolKey.NormalAttack)]
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
                playerState.IsGrounded &&
                !upInput &&
                HasNextComboAttack())
            {
                ContinueGroundCombo();
            }
            else
            {
                Attack();
            }

            return;
        }

        if (!playerState.IsNormalAttacking || !isGroundComboActive || !playerState.IsGrounded)
            return;

        if (upInput || !canQueueNextAttack || isNextAttackQueued || !HasNextComboAttack())
            return;

        isNextAttackQueued = true;
    }

    private void Attack()
    {
        if (upInput || !playerState.IsGrounded) return;

        comboIndex = 0;
        isGroundComboActive = playerState.IsGrounded && !upInput;
        isComboGraceActive = false;

        playerState.StartNormalAttack();
        HorizontalAttack();
    }

    private void HorizontalAttack()
    {
        canQueueNextAttack = false;
        isNextAttackQueued = false;

        //Debug.Log($"진행 중인 Index: {comboIndex}");

        attackHitboxSpawner.Spawn(attackHitboxPool, offset);

        attackStepVersion++;
        TestEndAttack(attackStepVersion).Forget();
    }

    public void OpenComboInput()
    {
        if (!playerState.IsNormalAttacking || !isGroundComboActive || !playerState.IsGrounded)
            return;

        if (HasNextComboAttack())
            canQueueNextAttack = true;
    }

    private bool HasNextComboAttack()
    {
        return comboIndex + 1 < MaxComboCount;
    }

    private void ContinueGroundCombo()
    {
        isComboGraceActive = false;
        comboIndex++;
        isGroundComboActive = true;

        if (!playerState.IsNormalAttacking)
            playerState.StartNormalAttack();

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
        if (!playerState.IsNormalAttacking)
            return;

        attackStepVersion++;
        canQueueNextAttack = false;

        if (isGroundComboActive && isNextAttackQueued && playerState.IsGrounded && HasNextComboAttack())
        {
            ContinueGroundCombo();
            return;
        }

        if (isGroundComboActive && playerState.IsGrounded && HasNextComboAttack() && comboGraceDuration > 0f)
        {
            isComboGraceActive = true;
            comboGraceEndTime = Time.time + comboGraceDuration;
        }
        else
        {
            isComboGraceActive = false;
            comboIndex = 0;
        }

        isGroundComboActive = false;
        isNextAttackQueued = false;
        playerState.StopNormalAttack();
    }
}
