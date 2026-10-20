using R3;
using UnityEngine;
using VContainer;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerState))]
[RequireComponent(typeof(AttackHitboxSpawner))]
public class PlayerLowAttack : MonoBehaviour
{
    private readonly Subject<Unit> onLowAttackStarted = new();
    public Observable<Unit> OnLowAttackStarted => onLowAttackStarted;
    
    [Header("내려가는 강도")]
    [SerializeField] private float slamPower = 20f;
    [Header("생성 위치 보정")]
    [SerializeField] private float offset = 2.5f;

    private InputManager inputManager;
    private Rigidbody2D rigid;
    private PlayerState playerState;
    private AttackHitboxSpawner attackHitboxSpawner;

    private AttackHitboxPool attackHitboxPool;

    private bool downInput;

    [Inject]
    public void Construct(
        InputManager inputManager, 
        [Key(PoolKey.LowAttack)]
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
        inputManager.DownInput
            .Subscribe(value => downInput = value)
            .AddTo(this);

        inputManager.OnNormalAttack
            .Subscribe(_ => HandleInput())
            .AddTo(this);
    }

    // 키 입력을 받는 조건 필터링 함수
    private void HandleInput()
    {
        if (!downInput || playerState.IsActionLocked()) return;
        if (playerState.IsGrounded) return;

        Attack();
    }

    // 공격 함수
    private void Attack()
    {
        rigid.AddForce(Vector2.down * slamPower, ForceMode2D.Impulse);

        playerState.StartLowAttack();

        onLowAttackStarted.OnNext(Unit.Default);
    }

    // 애니메이션 키 프레임에 넣는 히트박스 소환 함수
    public void OnLowAttackHitboxStart()
    {
        attackHitboxSpawner.Spawn(attackHitboxPool, offset);
    }

    // 애니메이션 키 프레임에 넣는 공격 종료 함수
    public void EndLowAttack()
    {
        playerState.StopAction(PlayerActionState.LowAttack);
    }
}
