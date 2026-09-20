using R3;
using UnityEngine;
using VContainer;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerState))]
[RequireComponent(typeof(PlayerGroundCheck))]
public class PlayerJump : MonoBehaviour
{
    [Header("플레이어 점프력")]
    [SerializeField] private float jumpPower = 20f; //점프력

    private InputManager inputManager;
    private PlayerState playerState;

    private Rigidbody2D rigid;
    private PlayerGroundCheck playerGroundCheck;

    [Inject]
    public void Construct(InputManager inputManager)
    {
        this.inputManager = inputManager;
    }

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        playerState = GetComponent<PlayerState>();
        playerGroundCheck = GetComponent<PlayerGroundCheck>();
    }

    private void Start()
    {
        inputManager.OnJump
            .Where(_ => 
                playerState.IsGrounded &&
                !playerState.IsActionLocked())
            .Subscribe(_ => Jump())
            .AddTo(this);
    }

    private void Jump()
    {
        rigid.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
        playerState.StartJump();
    }

    public bool IsNearGround()
    {
        return playerGroundCheck.IsNearGround();
    }
}
