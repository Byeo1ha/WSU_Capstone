using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MidBossBombFieldFloor : MonoBehaviour
{
    [Header("공격 범위")]
    [SerializeField] private Transform groundPoint;
    [SerializeField] private LayerMask damageLayer;
    [SerializeField] private LayerMask groundLayer;

    [Header("안전지대와 후폭풍 경고")]
    [SerializeField] private SpriteRenderer safeRenderer;
    [SerializeField] private SpriteRenderer leftWarningRenderer;
    [SerializeField] private SpriteRenderer rightWarningRenderer;

    [Header("경고 표시 크기와 위치")]
    [SerializeField, Min(0.01f)] private float warningHeight = 10f;
    [SerializeField] private float warningOffsetY = 0f;

    [Header("후폭풍 이펙트")]
    [SerializeField] private SpriteRenderer leftEffectRenderer;
    [SerializeField] private SpriteRenderer rightEffectRenderer;
    [SerializeField] private Animator leftEffectAnimator;
    [SerializeField] private Animator rightEffectAnimator;
    [SerializeField] private AnimationClip EffectClip;

    private readonly List<Collider2D> foundColliders = new();
    private readonly HashSet<IDamageable> damagedTargets = new();
    private readonly List<RaycastHit2D> terrainHits = new();
    private ContactFilter2D terrainFilter;

    private ContactFilter2D damageFilter;
    private Vector2 damageCenter;
    private Vector2 damageSize;

    private float fieldMinX;
    private float fieldMaxX;

    private float safeMinX;
    private float safeMaxX;

    private int attackDamage;
    private float timeRemaining;
    private bool isPrepared;

    public bool IsRunning { get; private set; }

    public event Action Finished;

    public bool PrepareWarning(MidBossBombFieldData data, Vector2 explosionPosition, float explosionRadius)
    {
        if (!isActiveAndEnabled || IsRunning || isPrepared ||
             data == null || groundPoint == null || safeRenderer == null ||
             safeRenderer.sprite == null || leftWarningRenderer == null || leftWarningRenderer.sprite == null ||
             rightWarningRenderer == null || rightWarningRenderer.sprite == null || leftEffectRenderer == null ||
             leftEffectRenderer.sprite == null || rightEffectRenderer == null || rightEffectRenderer.sprite == null ||
             leftEffectAnimator == null || rightEffectAnimator == null || EffectClip == null)
        {
            Debug.LogError("BombField 후폭풍의 참조와 스프라이트를 모두 연결해 주세요.", this);
            return false;
        }

        terrainFilter = new ContactFilter2D();
        terrainFilter.SetLayerMask(groundLayer);
        terrainFilter.useTriggers = false;

        Vector2 searchOrigin = new Vector2(explosionPosition.x, groundPoint.position.y + 0.05f);

        fieldMinX = FindBoundaryX(searchOrigin, Vector2.left, data.FieldSearchDistance);

        fieldMaxX = FindBoundaryX(searchOrigin, Vector2.right, data.FieldSearchDistance);

        if (fieldMaxX - fieldMinX <= 0.001f)
            return false;

        float height = Mathf.Max(0.01f, data.FieldDamageHeight);

        damageCenter = new Vector2((fieldMinX + fieldMaxX) * 0.5f, groundPoint.position.y + height * 0.5f);

        damageSize = new Vector2(fieldMaxX - fieldMinX, height);

        float radius = Mathf.Max(0f, explosionRadius);
        safeMinX = Mathf.Clamp(explosionPosition.x - radius, fieldMinX, fieldMaxX);
        safeMaxX = Mathf.Clamp(explosionPosition.x + radius, fieldMinX, fieldMaxX);

        Debug.Log(
            $"BombField 범위 | 전체 {fieldMinX:F2} ~ {fieldMaxX:F2}" +
            $" | 안전 {safeMinX:F2} ~ {safeMaxX:F2}" +
            $" | 왼쪽 폭 {safeMinX - fieldMinX:F2}" +
            $" | 오른쪽 폭 {fieldMaxX - safeMaxX:F2}", this);

        attackDamage = data.FieldDamage;

        damageFilter = new ContactFilter2D();
        damageFilter.SetLayerMask(damageLayer);
        damageFilter.useTriggers = true;

        damagedTargets.Clear();
        isPrepared = true;

        ShowRange(safeRenderer, safeMinX, safeMaxX, warningHeight);
        ShowRange(leftWarningRenderer, fieldMinX, safeMinX, warningHeight);
        ShowRange(rightWarningRenderer, safeMaxX, fieldMaxX, warningHeight);

        Vector3 offset = Vector3.up * warningOffsetY;

        safeRenderer.transform.position += offset;
        leftWarningRenderer.transform.position += offset;
        rightWarningRenderer.transform.position += offset;

        return true;
    }

    public bool Begin(MidBossBombFieldData data, Vector2 explosionPosition, float explosionRadius)

    {
        if (!isActiveAndEnabled || IsRunning || data == null)
            return false;

        if (!isPrepared && !PrepareWarning(data, explosionPosition, explosionRadius))
        {
            return false;
        }

        safeRenderer.enabled = false;
        leftWarningRenderer.enabled = false;
        rightWarningRenderer.enabled = false;

        isPrepared = false;
        IsRunning = true;
        timeRemaining = EffectClip.length;

        ShowEffects();
        CheckDamage();
        return true;
    }

    private void ShowEffects()
    {
        float leftWidth = Mathf.Max(0f, safeMinX - fieldMinX);
        float rightWidth = Mathf.Max(0f, fieldMaxX - safeMaxX);

        float leftScale = leftWidth / leftEffectRenderer.sprite.bounds.size.x;
        float rightScale = rightWidth / rightEffectRenderer.sprite.bounds.size.x;
        float scale = Mathf.Max(leftScale, rightScale);

        ShowEffect(leftEffectRenderer, leftEffectAnimator, safeMinX, leftWidth, scale, false);
        ShowEffect(rightEffectRenderer, rightEffectAnimator, safeMaxX, rightWidth, scale, true);
    }

    private void ShowEffect(SpriteRenderer renderer, Animator animator, float StartX, float width, float scale, bool flipX)
    {
        animator.enabled = false;
        renderer.enabled = false;

        if (width <= 0.001f)
            return;

        renderer.transform.position = new Vector3(StartX, groundPoint.position.y, groundPoint.position.z);
        renderer.transform.rotation = Quaternion.identity;
        renderer.transform.localScale = new Vector3(scale, scale, 1f);
        renderer.flipX = flipX;
        renderer.enabled = true;

        animator.enabled = true;
        animator.Rebind();
        animator.Update(0f);
        animator.SetTrigger("StartEffect");
        animator.Update(0f);
    }

    private void ShowRange(SpriteRenderer renderer, float minX, float maxX, float height)
    {
        float width = maxX - minX;

        if (width <= 0.001f || height <= 0f)
        {
            renderer.enabled = false;
            return;
        }

        Sprite sprite = renderer.sprite;
        Vector2 spriteSize = sprite.bounds.size;

        float scaleX = width / spriteSize.x;
        float scaleY = height / spriteSize.y;

        renderer.transform.rotation = Quaternion.identity;
        renderer.transform.localScale = new Vector3(scaleX, scaleY, 1f);

        Vector3 scaledCenter = new Vector3(sprite.bounds.center.x * scaleX, sprite.bounds.center.y * scaleY, 0f);
        Vector3 center = new Vector3((minX + maxX) * 0.5f, groundPoint.position.y + height * 0.5f, groundPoint.position.z);

        renderer.transform.position = center - scaledCenter;
        renderer.enabled = true;
    }

    private float FindBoundaryX(Vector2 origin, Vector2 direction, float maxDistance)
    {
        terrainHits.Clear();

        Physics2D.Raycast(origin, direction, terrainFilter, terrainHits, maxDistance);

        float distance = maxDistance;

        foreach (RaycastHit2D hit in terrainHits)
        {
            if (hit.collider == null)
                continue;

            distance = Mathf.Min(distance, hit.distance);
        }

        return origin.x + direction.x * distance;
    }

    private void Update()
    {
        if (!IsRunning || Time.timeScale <= 0f)
            return;

        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0f)
        {
            Stop();
            Finished?.Invoke();
            return;
        }

        CheckDamage();
    }

    private void CheckDamage()
    {
        foundColliders.Clear();

        Physics2D.OverlapBox(damageCenter, damageSize, 0f, damageFilter, foundColliders);

        foreach (Collider2D other in foundColliders)
        {
            if (!IsRunning || Time.timeScale <= 0f)
                return;

            if (other == null || !other.TryGetComponent<IDamageable>(out var target))
            {
                continue;
            }

            float playerCenterX = other.bounds.center.x;

            if (playerCenterX >= safeMinX && playerCenterX <= safeMaxX)
            {
                continue;
            }

            if (!damagedTargets.Add(target))
                continue;

            target.TakeDamage(attackDamage);
        }
    }

    public void Stop()
    {
        isPrepared = false;
        IsRunning = false;
        timeRemaining = 0f;
        damagedTargets.Clear();

        if (safeRenderer != null)
            safeRenderer.enabled = false;
        
        if (leftWarningRenderer != null)
            leftWarningRenderer.enabled = false;

        if (rightWarningRenderer != null)
            rightWarningRenderer.enabled = false;

        if (leftEffectAnimator != null)
            leftEffectAnimator.enabled = false;

        if (rightEffectAnimator != null)
            rightEffectAnimator.enabled = false;
        
        if (leftEffectRenderer != null)
            leftEffectRenderer.enabled = false;

        if (rightEffectRenderer != null)
            rightEffectRenderer.enabled = false;
    }

    private void OnDisable()
    {
        Stop();
    }

}
