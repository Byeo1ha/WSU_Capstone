using System;
using DG.Tweening;
using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
public class MidBossBombFieldProjectile : MonoBehaviour
{
    [Header("구체 포시")]
    [SerializeField] private SpriteRenderer orbVisual;
    [SerializeField] private CircleCollider2D hitCollider;
    private Rigidbody2D body;

    [Header("성장 시작 크기")]
    [SerializeField, Range(0.01f, 1f)] private float StartScaleRatio = 0.1f;

    private Vector3 originalScale;
    private Animator orbAnimator;
    private Color originalColor;
    private bool isExploding;
    private float explosionScaleMultiplier;
    private Tween growthTween;

    [Header("충돌할 지형")]
    [SerializeField] private LayerMask terrainLayer;
    [Header("구체 피해 대상")]
    [SerializeField] private LayerMask damageLayer;

    [Header("수동 테스트")]
    [SerializeField] private MidBossBombFieldData testData;
    [SerializeField] private BossTargetFinder testTargetFinder;
    [SerializeField] private Transform testSpawnPoint;

    [Header("폭발 범위 경고")]
    [SerializeField] private BombFieldWarning warningPrefab;

    private BombFieldWarning warningInstance;

    private Vector2 moveDirection;
    private float moveSpeed;
    private float explosionDelay;
    private float timeUntilExplosion;

    private bool hasGrown;
    private bool isFlying;
    private bool hasDetectedTerrain;

    public event Action<MidBossBombFieldProjectile> GrowthFinished;
    public event Action<MidBossBombFieldProjectile, Vector3> ExplosionRequested;
    public event Action<MidBossBombFieldProjectile> ExplosionFinished;

    private readonly List<RaycastHit2D> predictionHits = new();
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

    public bool BeginGrowth(float duration)
    {
        if (!isActiveAndEnabled || orbVisual == null)
            return false;

        Stop();
        orbAnimator.Rebind();
        orbAnimator.Update(0f);
        orbVisual.color = originalColor;

        orbVisual.transform.localScale = originalScale * StartScaleRatio;
        orbVisual.enabled = true;

        growthTween = orbVisual.transform
            .DOScale(originalScale, Mathf.Max(0.01f, duration))
            .SetEase(Ease.Linear)
            .OnComplete(HandleGrowthFinished);

        return true;
    }

    private void HandleGrowthFinished()
    {
        growthTween = null;
        hasGrown = true;

        GrowthFinished?.Invoke(this);
    }

    public bool Launch(Vector2 direction, MidBossBombFieldData data)
    {
        if (!isActiveAndEnabled || !hasGrown || isFlying ||
            data == null || direction.sqrMagnitude <= 0.0001f)
            return false;

        moveDirection = direction.normalized;
        moveSpeed = Mathf.Max(0.01f, data.ProjectileSpeed);
        explosionDelay = Mathf.Max(0f, data.ExplosionDelay);
        explosionScaleMultiplier = data.ExplosionRadiusMultiplier;
        attackDamage = data.ProjectileDamage;
        explosionDamage = data.ExplosionDamage;
        damagedTargets.Clear();

        hasGrown = false;
        hasDetectedTerrain = false;
        timeUntilExplosion = 0f;
        isFlying = true;

        hitCollider.enabled = true;
        Physics2D.SyncTransforms();

        if (!TryPredictExplosionPosition(out predictedExplosionPosition))
        {
            Debug.LogWarning("진행 방향에서 Wll 또는 Ground를 찾지 못했습니다.", this);


            Stop();
            return false;
        }

        float colliderRadius = Mathf.Max(hitCollider.bounds.extents.x, hitCollider.bounds.extents.y);

        explosionRadius = colliderRadius * data.ExplosionRadiusMultiplier;
        hasPrediction = true;
        warningInstance.Show(predictedExplosionPosition, explosionRadius);

        return true;
    }

    private void FixedUpdate()
    {
        if (!isFlying)
            return;

        if (hasDetectedTerrain)
        {
           timeUntilExplosion -= Time.fixedDeltaTime;

           if (timeUntilExplosion <= 0f)
            {
                RequestExplosion();
                return;
            }
        }

       Vector2 nextPosition = body.position + moveDirection * moveSpeed * Time.fixedDeltaTime;

       body.MovePosition(nextPosition);
    } 

    private bool TryPredictExplosionPosition(out Vector3 position)
    {
        position = transform.position;

        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(terrainLayer);
        filter.useTriggers = false;

        predictionHits.Clear();

        hitCollider.Cast(
            moveDirection,
            filter,
            predictionHits,
            Mathf.Infinity);

        float nearestDistance = Mathf.Infinity;
        bool found = false;

        foreach (RaycastHit2D hit in predictionHits)
        {
            if (hit.collider == null)
                continue;

            bool isTerrain = hit.collider.CompareTag("Wall") || hit.collider.CompareTag("Ground");

            if (!isTerrain || hit.distance >= nearestDistance)
                continue;

            nearestDistance = hit.distance;
            found = true;
        }

        if (!found)
            return false;

        float travelDistance = nearestDistance + moveSpeed * explosionDelay;

        Vector2 predictedPosition = body.position + moveDirection * travelDistance;

        position = new Vector3(
            predictedPosition.x,
            predictedPosition.y,
            transform.position.z);

        return true;
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
        if (!isActiveAndEnabled || (!isFlying && !isExploding))
            return;

        TryDamagePlayer(other);

        if (!isFlying || hasDetectedTerrain)
            return;

        if ((terrainLayer.value &(1 << other.gameObject.layer)) == 0)
            return;

        bool isTerrain = other.CompareTag("Wall") || other.CompareTag("Ground");

        if (!isTerrain)
            return;

        hasDetectedTerrain = true;
        timeUntilExplosion = explosionDelay;

        if (explosionDelay <= 0f)
            RequestExplosion();
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
        isExploding = false;
        hasPrediction = false;
        isFlying = false;
        hasGrown = false;
        hasDetectedTerrain = false;
        timeUntilExplosion = 0f;
        growthTween?.Kill(false);
        growthTween = null;

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
            BeginGrowth(1f);
    }

    [ContextMenu("테스트/성장 후 플레이어 방향 발사")]
    private void TestGrowthAndLaunch()
    {
        if (!Application.isPlaying || testData == null ||
            testTargetFinder == null || testSpawnPoint == null)
            return;

        Stop();

        body.position = testSpawnPoint.position;

        GrowthFinished -= HandleTestGrowthFinished;
        GrowthFinished += HandleTestGrowthFinished;

        if (!BeginGrowth(testData.GrowthDuration))
            GrowthFinished -= HandleTestGrowthFinished;
    }

    private void HandleTestGrowthFinished(MidBossBombFieldProjectile projectile)
    {
        GrowthFinished -= HandleTestGrowthFinished;

        if (!testTargetFinder.TryGetTargetPosition(out Vector2 targetPosition))
        {
            Debug.LogWarning("테스트할 플레이어를 찾지 못했습니다.", this);
            Stop();
            return;
        }

        Vector2 direction = targetPosition - (Vector2)transform.position;

        if (!Launch(direction, testData))
        {
            Debug.LogWarning("테스트 구체를 발사하지 못했습니다.", this);
            Stop();
        }
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
