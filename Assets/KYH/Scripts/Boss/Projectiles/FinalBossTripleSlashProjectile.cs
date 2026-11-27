using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class FinalBossTripleSlashProjectile : MonoBehaviour
{
    [Header("참격 표시")]
    [SerializeField] private SpriteRenderer slashRenderer;
    [SerializeField] private bool originalFacesRight = true;

    [Header("충돌 대상")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask damageLayer;

    private Rigidbody2D body;
    private BoxCollider2D hitCollider;

    private readonly HashSet<IDamageable> damagedTargets = new();

    private float moveSpeed;
    private int moveDirection;
    private int attackDamage;
    private bool isFlying;

    public event Action<FinalBossTripleSlashProjectile> Returned;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        hitCollider = GetComponent<BoxCollider2D>();
    }

    public bool Launch(Vector3 position, int direction, float speed,
        FinalBossTripleSlashData.SlashSettings settings)
    {
        if (!isActiveAndEnabled || slashRenderer == null || settings == null ||
            settings.Sprite == null || direction == 0 || speed <= 0f ||
            settings.ColliderSize.x <= 0f || settings.ColliderSize.y <= 0f)
        {
            return false;
        }

        moveDirection = direction > 0 ? 1 : -1;
        moveSpeed = speed;
        attackDamage = settings.Damage;

        damagedTargets.Clear();

        transform.position = position;
        body.position = new Vector2(position.x, position.y);
        body.linearVelocity = Vector2.zero;
        body.angularVelocity = 0f;

        bool flipX = (moveDirection > 0) != originalFacesRight;

        slashRenderer.sprite = settings.Sprite;
        slashRenderer.flipX = flipX;

        hitCollider.size = settings.ColliderSize;

        Vector2 offset = settings.ColliderOffset;

        if (flipX)
            offset.x = -offset.x;
        
        hitCollider.offset = offset;

        isFlying = true;
        return true;
    }

    private void FixedUpdate()
    {
        if (!isFlying)
            return;

        Vector2 movement = Vector2.right * moveDirection * moveSpeed * Time.fixedDeltaTime;

        body.MovePosition(body.position + movement);
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
        if (!isFlying || other == null)
            return;

        int layerBit = 1 << other.gameObject.layer;

        if ((groundLayer.value & layerBit) != 0)
        {
            ReturnToPool();
            return;
        }

        if ((damageLayer.value & layerBit) == 0)
            return;

        if (!other.TryGetComponent<IDamageable>(out var target))
            return;

        if (!damagedTargets.Add(target))
            return;

        target.TakeDamage(attackDamage);
    }

    public void ReturnToPool()
    {
        if (!isFlying)
            return;

        isFlying = false;
        gameObject.SetActive(false);

        Returned?.Invoke(this);
    }

    private void Osable()
    {
        isFlying = false;
        damagedTargets.Clear();

        if (body != null)
        {
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
        }
    }
}
