using System;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using VContainer;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(PlayerState))]
public class PlayerHighAttack : MonoBehaviour
{
    [Header("올라가는 높이")]
    [SerializeField] private float upperPower = 20f;
    [Header("생성 위치 보정")]
    [SerializeField] private float offset = 2.5f;

    private InputManager inputManager;
    private Rigidbody2D rigid;
    private SpriteRenderer spriteRenderer;
    private PlayerState playerState;

    private AttackHitboxPool attackHitboxPool;

    private bool upInput;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerState = GetComponent<PlayerState>();
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
        AttackHitbox attack = attackHitboxPool.Get();

        float x = spriteRenderer.flipX ? transform.position.x - offset : transform.position.x + offset;

        attack.transform.position = new Vector3(x, transform.position.y, transform.position.z);
        attack.GetComponent<SpriteRenderer>().flipX = spriteRenderer.flipX;
        playerState.StartHighAttack();

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
        playerState.StopHighAttack();
    }
}
