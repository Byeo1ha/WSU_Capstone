using System;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using VContainer;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerDodge : MonoBehaviour
{
    [SerializeField] private float dashDistance = 3f;
    [SerializeField] private float dashSpeed = 12f;

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

    private void HandleInput()
    {
        if (!playerState.IsGrounded) return;
        if (playerState.IsActionLocked()) return;

        Dodge().Forget();
    }

    private async UniTask Dodge()
    {
        playerState.StartDodge();
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

        playerState.StopDodge();

        gameObject.layer = originalLayer;
    }
}
