using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerState))]
public class PlayerGroundCheck : MonoBehaviour
{
    [Header("착지 판정")]
    [SerializeField] private Transform groundCheck; //바닥 감지용
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.7f, 0.08f); //바닥 감지용 크기
    [SerializeField] private LayerMask groundLayer; //바닥 레이어 지정

    [Header("근접 판정")]
    [SerializeField] private float nearGroundDistance = 1f; //바닥이랑 가까워지는 거리

    private Rigidbody2D rigid;
    private PlayerState playerState;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        playerState = GetComponent<PlayerState>();
    }

    private void FixedUpdate()
    {
        bool isGrounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer);

        if (playerState.IsJumping && rigid.linearVelocity.y > 0.1f)
            isGrounded = false;

        playerState.SetGrounded(isGrounded);
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
