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

    public bool IsRunning { get; private set; }

    public event Action Finished;

    public bool Begin(MidBossBombFieldData data, Vector2 explosionPosition, float explosionRadius)
    {
        if (!isActiveAndEnabled || IsRunning || data == null || groundPoint == null)
        {
            return false;
        }

        terrainFilter = new ContactFilter2D();
        terrainFilter.SetLayerMask(groundLayer);
        terrainFilter.useTriggers = false;

        Vector2 searchOrigin = new Vector2(explosionPosition.x, groundPoint.position.y + 0.05f);

        fieldMinX = FindBoundaryX(searchOrigin, Vector2.left, data.FieldSearchDistance);

        fieldMaxX = FindBoundaryX(searchOrigin, Vector2.left, data.FieldSearchDistance);

        if (fieldMaxX - fieldMinX <= 0.001f)
            return false;

        float height = Mathf.Max(0.01f, data.FieldDamageHeight);

        damageCenter = new Vector2((fieldMinX + fieldMaxX) * 0.5f, groundPoint.position.y + height * 0.5f);

        damageSize = new Vector2(fieldMaxX - fieldMinX, height);

        float radius = Mathf.Max(0f, explosionRadius);
        safeMinX = Mathf.Clamp(explosionPosition.x - radius, fieldMinX, fieldMaxX);
        safeMaxX = Mathf.Clamp(explosionPosition.x + radius, fieldMinX, fieldMaxX);

        attackDamage = data.FieldDamage;
        timeRemaining = Mathf.Max(0.01f, data.FieldDuration);

        damageFilter = new ContactFilter2D();
        damageFilter.SetLayerMask(damageLayer);
        damageFilter.useTriggers = true;

        damagedTargets.Clear();
        IsRunning = true;

        CheckDamage();
        return true;
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
        IsRunning = false;
        timeRemaining = 0f;
        damagedTargets.Clear();
    }

    private void OnDisable()
    {
        Stop();
    }

}
