using UnityEngine;
[RequireComponent(typeof(Rigidbody2D))]

public class BossFireball : MonoBehaviour
{
    private Rigidbody2D rigid;
    private Vector2 moveDirection;
    private float moveSpeed;
    private BossFireballPool ownerPool;
    private float lifetimeRemaining;
    private bool isLaunched;
    private Transform preparationPoint;
    private Vector3 preparationOffset;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }

    public void Initialize(BossFireballPool pool)
    {
        ownerPool = pool;
    }

    public void Prepare(float lifetime, Vector2 direction, float speed)
    {
        if(rigid == null)
            rigid = GetComponent<Rigidbody2D>();

        rigid.linearVelocity = Vector2.zero;
        rigid.angularVelocity = 0f;
        
        isLaunched = false;
        preparationPoint = null;
        lifetimeRemaining = Mathf.Max(0f, lifetime);
        moveDirection = direction.normalized;
        moveSpeed = Mathf.Max(0f, speed);
    }

    public void SetPreparationPoint(Transform point, Vector3 offset)
    {
        preparationPoint = point;
        preparationOffset = offset;
    }

    public void launch()
    {
        rigid.linearVelocity = Vector2.zero;
        rigid.angularVelocity = 0f;

        Vector2 impulse = moveDirection * moveSpeed * rigid.mass;
        rigid.AddForce(impulse, ForceMode2D.Impulse);
        isLaunched = true;
    }
    
    public void launch(Vector2 direction)
    {
        moveDirection = direction.normalized;
        launch();
    }

    private void Update()
    {
        if(!isLaunched)
        {
            if(preparationPoint != null)
                rigid.position = (Vector2)(preparationPoint.position + preparationOffset);

            return;
        }
        lifetimeRemaining = Mathf.Max(0f, lifetimeRemaining - Time.deltaTime);

        if(lifetimeRemaining <= 0f)
            ReturnToPool();
    }

    public void ReturnToPool()
    {
        if(ownerPool == null)
        {
            Debug.LogError("반환할 파이어볼 풀이 없습니다.", this);
                return;
        }

        ownerPool.Return(gameObject);
    }

    [ContextMenu("테스트/풀로 반환")]
    private void TestReturnToPool()
    {
        if(!Application.isPlaying)
            return;
        
        ReturnToPool();
    }
}
