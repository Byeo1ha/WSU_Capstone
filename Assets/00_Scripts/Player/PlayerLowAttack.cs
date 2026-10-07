using System;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using VContainer;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerState))]
[RequireComponent(typeof(AttackHitboxSpawner))]
public class PlayerLowAttack : MonoBehaviour
{
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

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        playerState = GetComponent<PlayerState>();
        attackHitboxSpawner = GetComponent<AttackHitboxSpawner>();
    }

    [Inject]
    public void Construct(
        InputManager inputManager, 
        [Key(PoolKey.LowAttack)]
        AttackHitboxPool attackHitboxPool)
    {
        this.inputManager = inputManager;
        this.attackHitboxPool = attackHitboxPool;
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

    private void HandleInput()
    {
        if (!downInput || playerState.IsActionLocked()) return;
        if (playerState.IsGrounded) return;

        Attack();
    }

    private void Attack()
    {
        rigid.AddForce(Vector2.down * slamPower, ForceMode2D.Impulse);

        attackHitboxSpawner.Spawn(attackHitboxPool, offset);
        playerState.StartDownAttack();

        TestEndAttack().Forget();
    }

    //테스트 전용 함수
    //캐릭터 공격 애니메이션을 받으면 마지막 프레임에 EndAttack함수를 실행시키게 할 것
    private async UniTask TestEndAttack()
    {
        await UniTask.Delay(
            TimeSpan.FromSeconds(0.4f), 
            cancellationToken: this.GetCancellationTokenOnDestroy());

        EndAttack();
    }

    public void EndAttack()
    {
        playerState.StopDownAttack();
    }
}
