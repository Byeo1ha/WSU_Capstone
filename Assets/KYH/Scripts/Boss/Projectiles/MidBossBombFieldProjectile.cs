using System;
using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
public class MidBossBombFieldProjectile : MonoBehaviour
{
    [Header("구체 포시")]
    [SerializeField] private SpriteRenderer orbVisual;
    [SerializeField] private CircleCollider2D hitCollider;
    private Rigidbody2D body;

    private Vector3 originalScale;
    private Animator orbAnimator;
    private Color originalColor;
    private bool isExploding;
    private bool isGrowing;
    private float explosionScaleMultiplier;

    [Header("구체 피해 대상")]
    [SerializeField] private LayerMask damageLayer;

    [Header("폭발 범위 경고")]
    [SerializeField] private BombFieldWarning warningPrefab;

    private BombFieldWarning warningInstance;

    private float moveSpeed;
    private Vector2 destination;
    private bool hasGrown;
    private bool isFlying;

    public event Action<MidBossBombFieldProjectile> GrowthFinished;
    public event Action<MidBossBombFieldProjectile, Vector3> ExplosionRequested;
    public event Action<MidBossBombFieldProjectile> ExplosionFinished;

    private readonly HashSet<IDamageable> damagedTargets = new();
    private int attackDamage;
    private int explosionDamage;
    private Vector3 predictedExplosionPosition;
    private float explosionRadius;
    private bool hasPrediction;

    public Vector3 PredictedExplosionPosition => predictedExplosionPosition;
    public float ExplosionRadius => explosionRadius;
    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();

        if (orbVisual == null || hitCollider == null || warningPrefab == null)
        {
            Debug.LogError("구체 그림, Collider, 경고 프리팹을 연결해 주세요.", this);
            enabled = false;
            return;
        }
        
        orbAnimator = GetComponent<Animator>();

        if (orbAnimator == null)
        {
            Debug.LogError("구체 루트에 Animator를 추가해 주세요.", this);
            enabled = false;
            return;
        }

        originalScale = orbVisual.transform.localScale;
        originalColor = orbVisual.color;
        warningInstance = Instantiate(warningPrefab);
        warningInstance.gameObject.SetActive(true);
        warningInstance.Hide();
    }

    private void OnEnable()
    {
        Stop();
    }

    public void PrepareAt(Vector3 position)
    {
        Stop();
        
        body.position = new Vector2(position.x, position.y);

        Vector3 currentPosition = transform.position;
        currentPosition.z = position.z;
        transform.position = currentPosition;
    }

    public bool BeginGrowth()
    {
        if (!isActiveAndEnabled || orbVisual == null ||
            orbAnimator == null || !orbAnimator.isActiveAndEnabled)
            return false;

        Stop();
        orbAnimator.Rebind();
        orbAnimator.Update(0f);
        orbVisual.color = originalColor;

        orbVisual.transform.localScale = new Vector3(0f, 0f, 1f);
        orbVisual.enabled = true;

       isGrowing = true;

       orbAnimator.SetTrigger("StartGrowth");

        return true;
    }

    public void OnGrowthFinished()
    {
        if (!isActiveAndEnabled || !isGrowing)
            return;

        isGrowing = false;
        hasGrown = true;

        orbVisual.transform.localScale = Vector3.one;

        orbAnimator.SetTrigger("StartFly");

        GrowthFinished?.Invoke(this);
    }

    public bool Launch(Vector2 targetPosition, MidBossBombFieldData data)
    {
        if (!isActiveAndEnabled || !hasGrown || isFlying ||
            data == null)
            return false;

        destination = targetPosition;

        moveSpeed = Mathf.Max(0.01f, data.ProjectileSpeed);
        explosionScaleMultiplier = data.ExplosionRadiusMultiplier;
        
        attackDamage = data.ProjectileDamage;
        explosionDamage = data.ExplosionDamage;
        damagedTargets.Clear();

        hasGrown = false;
        isFlying = true;

        hitCollider.enabled = true;
        Physics2D.SyncTransforms();

        float colliderRadius = Mathf.Max(hitCollider.bounds.extents.x, hitCollider.bounds.extents.y);

        explosionRadius = colliderRadius * data.ExplosionRadiusMultiplier;
        
        predictedExplosionPosition = new Vector3(destination.x, destination.y, transform.position.z);
        hasPrediction = true;
        warningInstance.Show(predictedExplosionPosition, explosionRadius);

        return true;
    }

    private void FixedUpdate()
    {
        if (!isFlying)
            return;

        float moveDistance = moveSpeed * Time.fixedDeltaTime;

        if (Vector2.Distance(body.position, destination) <= moveDistance)
        {
            body.position = destination;
            RequestExplosion();
            return;
        }

        Vector2 nextPosition = Vector2.MoveTowards(body.position, destination, moveDistance);

       body.MovePosition(nextPosition);
    } 

    private void TryDamagePlayer(Collider2D other)
    {

        if (!isActiveAndEnabled || (!isFlying && !isExploding) ||Time.timeScale <= 0f || other == null)
            return;
        
        if ((damageLayer.value & (1 << other.gameObject.layer)) == 0)
            return;

        if (!other.TryGetComponent<IDamageable>(out var target))
            return;

        if (!damagedTargets.Add(target))
            return;

        target.TakeDamage(attackDamage);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryDamagePlayer(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryDamagePlayer(other);
    }

    private void RequestExplosion()
    {
        if (!isFlying || isExploding)
            return;

        isFlying = false;
        isExploding = true;

        body.linearVelocity = Vector2.zero;

        damagedTargets.Clear();
        attackDamage = explosionDamage;

        hitCollider.enabled = true;
        body.WakeUp();
        
        if (warningInstance != null)
            warningInstance.Hide();

        orbVisual.transform.localScale = originalScale * explosionScaleMultiplier;

        orbVisual.color = originalColor;
        orbVisual.enabled = true;

        orbAnimator.ResetTrigger("Explode");
        orbAnimator.SetTrigger("Explode");

        Vector3 explosionPosition = transform.position;

        Debug.Log($"BombField 폭발 요청 위치: {explosionPosition}", this);
        ExplosionRequested?.Invoke(this, explosionPosition);
    }

    public void OnExplosionFinished()
    {
        if (!isActiveAndEnabled || !isExploding)
            return;

        isExploding = false;
        hitCollider.enabled = false;
        orbVisual.enabled = false;

        ExplosionFinished?.Invoke(this);
    }

    public void Stop()
    {
        isGrowing = false;
        isExploding = false;
        hasPrediction = false;
        isFlying = false;
        hasGrown = false;

        if (orbVisual != null)
            orbVisual.enabled = false;

        if (body != null)
            body.linearVelocity = Vector2.zero;

        if (hitCollider != null)
            hitCollider.enabled = false;

        if (warningInstance != null)
            warningInstance.Hide();
        damagedTargets.Clear();
    }

    private void OnDisable()
    {
        Stop();
    }

    [ContextMenu("테스트/구체 성장 1초")]
    private void TestGrowth()
    {
        if (Application.isPlaying)
            BeginGrowth();
    }

    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying || !hasPrediction)
            return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(predictedExplosionPosition, explosionRadius);
    }

    private void OnDestroy()
    {
        if (warningInstance != null)
            Destroy(warningInstance.gameObject);
    }
}
