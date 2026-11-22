using System;
using System.Collections.Generic;
using UnityEngine;

public class MidBossBombFieldFloor : MonoBehaviour
{
    [Header("공격 범위")]
    [SerializeField] private MidBossBlinkArea blinkArea;
    [SerializeField] private Transform groundPoint;
    [SerializeField] private LayerMask damageLayer;

    private readonly List<Collider2D> foundColliders = new();
    private readonly HashSet<IDamageable> damagedTargets = new();

    private ContactFilter2D damageFilter;
    private Vector2 damageCenter;
    private Vector2 damageSize;

    private Vector2 safeCenter;
    private float safeRadiusSquared;

    private int attackDamage;
    private float timeRemaining;

    public bool IsRunning { get; private set; }

    public event Action Finished;

    public bool Begin(MidBossBombFieldData data, Vector2 explosionPosition, float explosionRadius)
    {
        if (!isActiveAndEnabled || IsRunning || data == null || blinkArea == null || groundPoint == null)
        {
            return false;
        }

        if (!blinkArea.TryGetBounds(out float minX, out float maxX))
            return false;

        float height = Mathf.Max(0.01f, data.FieldDamageHeight);

        damageCenter = new Vector2((minX + maxX) * 0.5f, groundPoint.position.y + height * 0.5f);

        damageSize = new Vector2(maxX - minX, height);

        safeCenter = explosionPosition;

        float radius = Mathf.Max(0f, explosionRadius);
        safeRadiusSquared = radius * radius;

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

            Vector2 playerCenter = other.bounds.center;

            if ((playerCenter - safeCenter).sqrMagnitude <= safeRadiusSquared)
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
