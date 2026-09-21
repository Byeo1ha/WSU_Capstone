using System;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using VContainer;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(PlayerState))]
public class PlayerNormalAttack : MonoBehaviour
{
    private const int MaxComboCount = 4;

    [Header("생성 위치 보정")]
    [SerializeField] private float offset = 1.5f;

    [Header("공격 종료 후 콤보 유예시간")]
    [SerializeField] private float comboGraceDuration = 0.4f;

    private InputManager inputManager;
    private NormalAttackPool normalAttackPool;

    private PlayerState playerState;
    private SpriteRenderer spriteRenderer;

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
        NormalAttackPool normalAttackPool)
    {
        this.inputManager = inputManager;
        this.normalAttackPool = normalAttackPool;
    }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerState = GetComponent<PlayerState>();
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

        if (!playerState.IsAttacking || !isGroundComboActive || !playerState.IsGrounded)
            return;

        if (upInput || !canQueueNextAttack || isNextAttackQueued || !HasNextComboAttack())
            return;

        isNextAttackQueued = true;
    }

    private void Attack()
    {
        comboIndex = 0;
        isGroundComboActive = playerState.IsGrounded && !upInput;
        canQueueNextAttack = false;
        isNextAttackQueued = false;
        isComboGraceActive = false;

        playerState.StartNormalAttack();

        if (upInput) HighAttack();
        else HorizontalAttack();
    }

    private void HorizontalAttack()
    {
        canQueueNextAttack = false;
        isNextAttackQueued = false;

        NormalAttack attack = normalAttackPool.Get(comboIndex);

        if (attack == null)
        {
            isGroundComboActive = false;
            EndAttack();
            return;
        }
        
        float x = spriteRenderer.flipX ? transform.position.x - offset : transform.position.x + offset;

        attack.transform.position = new Vector3(x, transform.position.y, transform.position.z);
        attack.GetComponent<SpriteRenderer>().flipX = spriteRenderer.flipX;

        attackStepVersion++;
        TestEndAttack(attackStepVersion).Forget();
    }

    private void HighAttack()
    {
        Debug.Log("상단 공격 수행");
        attackStepVersion++;
        TestEndAttack(attackStepVersion).Forget();
    }

    public void OpenComboInput()
    {
        if (!playerState.IsAttacking || !isGroundComboActive || !playerState.IsGrounded)
            return;

        if (HasNextComboAttack())
            canQueueNextAttack = true;
    }

    private bool HasNextComboAttack()
    {
        return comboIndex + 1 < Mathf.Min(MaxComboCount, normalAttackPool.ComboCount);
    }

    private void ContinueGroundCombo()
    {
        isComboGraceActive = false;
        comboIndex++;
        isGroundComboActive = true;

        if (!playerState.IsAttacking)
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
        if (!playerState.IsAttacking)
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
