using System;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using VContainer;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(PlayerState))]
public class PlayerDodge : MonoBehaviour
{
    [SerializeField] private float dashDistance = 3f;
    [SerializeField] private float dashSpeed = 12f;

    private Subject<Unit> onDodged = new Subject<Unit>();
    public Observable<Unit> OnDodged => onDodged;

    private InputManager inputManager;

    private Rigidbody2D rigid;
    private SpriteRenderer spriteRenderer;
    private PlayerState playerState;

    private int originalLayer;
    private int invincibleLayer;

    [Inject]
    public void Construct(InputManager inputManager)
    {
        this.inputManager = inputManager;
    }

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerState = GetComponent<PlayerState>();

        originalLayer = LayerMask.NameToLayer("Player");
        invincibleLayer = LayerMask.NameToLayer("Player_inv");
    }

    private void Start()
    {
        gameObject.layer = originalLayer;

        inputManager.OnDodge
            .Subscribe(_ => HandleInput())
            .AddTo(this);
    }

    // 회피 입력을 받아 회피 실행을 시도하는 함수
    private void HandleInput()
    {
        TryDodge();
    }

    // 활성 상태와 지상 여부, 행동 잠금을 확인하고 회피 시작 여부를 반환하는 함수
    public bool TryDodge()
    {
        if (!isActiveAndEnabled) return false;
        if (!playerState.IsGrounded) return false;
        if (playerState.IsActionLocked()) return false;

        Dodge().Forget();
        return true;
    }

    // 회피 함수
    private async UniTask Dodge()
    {
        playerState.StartDodge();
        onDodged.OnNext(Unit.Default);
        
        gameObject.layer = invincibleLayer;
        
        int flip = spriteRenderer.flipX ? -1 : 1;
        float moved = 0f;   

        rigid.linearVelocity = new Vector2(0, rigid.linearVelocity.y);  

        await UniTask.Delay(
            TimeSpan.FromSeconds(0.05f), 
            cancellationToken: this.GetCancellationTokenOnDestroy());  

        while (moved < dashDistance)
        {
            float frameMove = dashSpeed * Time.fixedDeltaTime;

            if (moved + frameMove > dashDistance)
                frameMove = dashDistance - moved;   

            rigid.linearVelocity = new Vector2(dashSpeed * flip, rigid.linearVelocity.y);   
            moved += frameMove;

            await UniTask.WaitForFixedUpdate(
                cancellationToken: this.GetCancellationTokenOnDestroy());
        }

        playerState.StopAction(PlayerActionState.Dodge);

        gameObject.layer = originalLayer;
    }
}
