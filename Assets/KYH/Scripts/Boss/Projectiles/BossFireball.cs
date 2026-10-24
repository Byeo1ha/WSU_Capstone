using System;
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
    private int damage;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }

    public void Initialize(BossFireballPool pool)
    {
        ownerPool = pool;
    }

    public void Prepare(float lifetime, Vector2 direction, float speed, int damage)
    {
        if(rigid == null)
            rigid = GetComponent<Rigidbody2D>();

        rigid.linearVelocity = Vector2.zero;
        rigid.angularVelocity = 0f;
        
        isLaunched = false;
        lifetimeRemaining = Mathf.Max(0f, lifetime);
        moveDirection = direction.normalized;
        moveSpeed = Mathf.Max(0f, speed);
        this.damage = Mathf.Max(0, damage);
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
            return;
            
        lifetimeRemaining = Mathf.Max(0f, lifetimeRemaining - Time.deltaTime);

        if(lifetimeRemaining <= 0f)
            ReturnToPool();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        HandleContact(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        HandleContact(other);
    }

    private void HandleContact(Collider2D other)
    {
        if(!isLaunched || !gameObject.activeInHierarchy)
            return;

        if(other.CompareTag("Ground") || other.CompareTag("Wall"))
        {
            ReturnToPool();
            return;
        }

        if(other.gameObject.layer != LayerMask.NameToLayer("Player"))
            return;

        if(!other.TryGetComponent<IDamageable>(out var target))
            return;

        isLaunched = false;

        target.TakeDamage(damage);
        ReturnToPool();
    }

    public void ReturnToPool()
    {
        if(ownerPool == null)
        {
            Debug.LogError("반환할 파이어볼 풀이 없습니다.", this);
                return;
        }
        
        isLaunched = false;
        rigid.linearVelocity = Vector2.zero;
        rigid.angularVelocity = 0f;

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
