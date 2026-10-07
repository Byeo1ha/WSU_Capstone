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
    [Header("올라가는 높이")]
    [SerializeField] private float upperPower = 20f;
    [Header("생성 위치 보정")]
    [SerializeField] private float offset = 2.5f;
    [Header("공중 체공 보정")]
    [SerializeField] private float airOffset = 0.8f;

    private InputManager inputManager;
    private Rigidbody2D rigid;
    private PlayerState playerState;
    private AttackHitboxSpawner attackHitboxSpawner;

    private AttackHitboxPool attackHitboxPool;

    private CancellationTokenSource cts;

    private bool upInput;
    private float originalGravity;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        playerState = GetComponent<PlayerState>();
        attackHitboxSpawner = GetComponent<AttackHitboxSpawner>();

        originalGravity = rigid.gravityScale;
    }

    [Inject]
    public void Construct(
        InputManager inputManager, 
        [Key(PoolKey.HighAttack)]
        AttackHitboxPool attackHitboxPool)
    {
        this.inputManager = inputManager;
        this.attackHitboxPool = attackHitboxPool;
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

    private void HandleInput()
    {
        if (!upInput || playerState.IsActionLocked()) return;
        if (!playerState.IsGrounded) return;

        Attack();
    }

    private void Attack()
    {
        rigid.AddForce(Vector2.up * upperPower, ForceMode2D.Impulse);

        attackHitboxSpawner.Spawn(attackHitboxPool, offset);
        playerState.StartHighAttack();

        AirCorrection().Forget();
    }

    private async UniTask AirCorrection()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(0.2f));

        cts = new CancellationTokenSource();

        rigid.gravityScale = 0f;

        TestEndAttack().Forget();

        while (true)
        {
            rigid.linearVelocity = new Vector2(
                rigid.linearVelocity.x, 
                rigid.linearVelocity.y * airOffset);

            await UniTask.WaitForFixedUpdate(
                cancellationToken: cts.Token);
        }
    }

    //테스트 전용 함수
    //캐릭터 공격 애니메이션을 받으면 마지막 프레임에 EndAttack함수를 실행시키게 할 것
    private async UniTask TestEndAttack()
    {
        await UniTask.Delay(
            TimeSpan.FromSeconds(0.2f), 
            cancellationToken: this.GetCancellationTokenOnDestroy());

        rigid.gravityScale = originalGravity;
        cts?.Cancel();
        EndAttack();
    }

    public void EndAttack()
    {
        playerState.StopHighAttack();
    }
}
