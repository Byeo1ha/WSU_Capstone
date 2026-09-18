using R3;
using UnityEngine;
using VContainer;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerState))]
public class PlayerJump : MonoBehaviour
{
    [Header("플레이어 점프력")]
    [SerializeField] private float jumpPower = 20f;

    [Header("착지 판정")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.7f, 0.08f);
    [SerializeField] private LayerMask groundLayer;

    [Header("근접 판정")]
    [SerializeField] private float nearGroundDistance = 1f;

    private InputManager inputManager;
    private PlayerState playerState;

    private Rigidbody2D rigid;

    [Inject]
    public void Construct(InputManager inputManager)
    {
        this.inputManager = inputManager;
    }

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        playerState = GetComponent<PlayerState>();
    }

    private void Start()
    {
        inputManager.OnJump
            .Subscribe(_ => Jump())
            .AddTo(this);
    }

    private void FixedUpdate()
    {
        bool isGrounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer);

        if (playerState.IsJumping && rigid.linearVelocity.y > 0.1f) 
            isGrounded = false;

        playerState.SetGrounded(isGrounded);
    }

    private void Jump()
    {
        if (!playerState.IsGrounded) return;

        rigid.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
        playerState.StartJump();
    }

    public bool IsNearGround()
    {
        if (rigid.linearVelocity.y >= 0f) return false;

        Vector2 checkSize = new Vector2(groundCheckSize.x, nearGroundDistance);

        Vector2 checkPosition = (Vector2)groundCheck.position 
            + Vector2.down * nearGroundDistance * 0.5f;

        return Physics2D.OverlapBox(checkPosition, checkSize, 0f, groundLayer);
    }

}
