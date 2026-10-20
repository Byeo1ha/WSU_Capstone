using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using VContainer;

[RequireComponent(typeof(PlayerState))]
[RequireComponent(typeof(AttackHitboxSpawner))]
public class PlayerNormalAttack : MonoBehaviour
{
    private const int MaxComboCount = 4;

    private readonly Subject<int> onNormalAttackStarted = new();
    public Observable<int> OnNormalAttackStarted => onNormalAttackStarted;

    [Header("공격 종료 후 콤보 유예시간")]
    [SerializeField] private float comboGraceDuration = 0.4f;

    [Header("생성 위치 보정")]
    [SerializeField] private float offset = 1.5f;

    [Header("4타 생성 위치 보정")]
    [SerializeField] private float finalAttackOffset = 3.5f;

    private InputManager inputManager;
    private AttackHitboxPool attackHitboxPool;
    private AttackHitboxPool finalAttackHitboxPool;
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

    public int ComboIndex => comboIndex;
    public int MaxCombo => MaxComboCount;

    [Inject]
    public void Construct(
        InputManager inputManager,
        [Key(PoolKey.NormalAttack)]
        AttackHitboxPool attackHitboxPool,
        [Key(PoolKey.FinalNormalAttack)]
        AttackHitboxPool finalAttackHitboxPool)
    {
        this.inputManager = inputManager;
        this.attackHitboxPool = attackHitboxPool;
        this.finalAttackHitboxPool = finalAttackHitboxPool;
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

    private void OnDestroy()
    {
        onNormalAttackStarted.Dispose();
    }

    // 키 입력을 받는 조건 필터링 함수
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
                TryAttack();
            }

            return;
        }

        if (!playerState.IsNormalAttacking || !isGroundComboActive || !playerState.IsGrounded)
            return;

        if (upInput || !canQueueNextAttack || isNextAttackQueued || !HasNextComboAttack())
            return;

        isNextAttackQueued = true;
    }

    // 공격 실행을 시도하는 함수
    private void TryAttack()
    {
        if (upInput || !playerState.IsGrounded) return;

        comboIndex = 0;
        isGroundComboActive = playerState.IsGrounded && !upInput;
        isComboGraceActive = false;

        playerState.StartNormalAttack();
        HorizontalAttack();
    }

    // 수평 공격을 실제로 실행하는 함수
    private void HorizontalAttack()
    {
        canQueueNextAttack = false;
        isNextAttackQueued = false;

        onNormalAttackStarted.OnNext(comboIndex);

        attackStepVersion++;
    }

    // 애니메이션 키 프레임에 넣는 히트박스 소환 함수
    public void OnNormalAttackHitboxStart()
    {
        if (!playerState.IsNormalAttacking) return;
        
        attackHitboxSpawner.Spawn(attackHitboxPool, offset);
    }

    // 애니메이션 키 프레임에 넣는 마지막 공격 히트박스 소환 함수
    public void OnFinalNormalAttackHitboxStart()
    {
        if (!playerState.IsNormalAttacking) return;
        
        attackHitboxSpawner.Spawn(finalAttackHitboxPool, finalAttackOffset);
    }

    // 애니메이션 키 프레임에 넣는 선입력 함수
    public void OpenNormalAttackComboInput()
    {
        if (!playerState.IsNormalAttacking || !isGroundComboActive || !playerState.IsGrounded)
            return;

        if (HasNextComboAttack())
            canQueueNextAttack = true;
    }

    // 다음 콤보 공격이 남아 있는지 확인하는 함수
    private bool HasNextComboAttack()
    {
        return comboIndex + 1 < MaxComboCount;
    }

    // 콤보 수치를 올리고 다음 콤보 공격을 실행하는 함수
    private void ContinueGroundCombo()
    {
        isComboGraceActive = false;
        comboIndex++;
        isGroundComboActive = true;

        if (!playerState.IsNormalAttacking)
            playerState.StartNormalAttack();

        HorizontalAttack();
    }

    // 애니메이션 키 프레임에 넣는 공격 종료 함수
    public void EndNormalAttack()
    {
        EndAttackInternal(attackStepVersion);
    }

    // 현재 공격의 종료 요청인지 확인하고, 다음 콤보 진행 또는 공격 종료를 처리하는 함수
    private void EndAttackInternal(int version)
    {
        if (version != attackStepVersion)
            return;

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
        playerState.StopAction(PlayerActionState.NormalAttack);
    }
}
