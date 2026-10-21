using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using VContainer;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerState))]
[RequireComponent(typeof(AttackHitboxSpawner))]
public class PlayerHighAttack : MonoBehaviour
{
    private readonly Subject<Unit> onHighAttackStarted = new();
    public Observable<Unit> OnHighAttackStarted => onHighAttackStarted;

    [Header("올라가는 높이")]
    [SerializeField] private float upperPower = 20f;
    [Header("생성 위치 보정")]
    [SerializeField] private float offset = 2.5f;

    private InputManager inputManager;
    private Rigidbody2D rigid;
    private PlayerState playerState;
    private AttackHitboxSpawner attackHitboxSpawner;

    private AttackHitboxPool attackHitboxPool;

    private bool upInput;

    [Inject]
    public void Construct(
        InputManager inputManager, 
        [Key(PoolKey.HighAttack)]
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
    }

    private void Start()
    {
        inputManager.UpInput
            .Subscribe(value => upInput = value)
            .AddTo(this);

        inputManager.OnNormalAttack
            .Subscribe(_ => HandleInput())
            .AddTo(this);
    }

    // 키 입력을 받는 조건 필터링 함수
    private void HandleInput()
    {
        if (!upInput || playerState.IsActionLocked()) return;
        if (!playerState.IsGrounded) return;

        Attack();
    }

    // 공격하는 함수
    private void Attack()
    {
        onHighAttackStarted.OnNext(Unit.Default);

        rigid.AddForce(Vector2.up * upperPower, ForceMode2D.Impulse);

        playerState.StartHighAttack();
    }

    public void OnHighAttackHitboxStart()
    {
        attackHitboxSpawner.Spawn(attackHitboxPool, offset);
    }

    // 애니메이션 키 프레임에 넣는 공격 종료 함수
    public void EndHighAttack()
    {
        playerState.StopAction(PlayerActionState.HighAttack);
    }
}
