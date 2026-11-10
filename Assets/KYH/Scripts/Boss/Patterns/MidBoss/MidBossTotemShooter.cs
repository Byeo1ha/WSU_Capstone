using System;
using System.Collections.Generic;
using UnityEngine;

public class MidBossTotemShooter : MonoBehaviour
{
    [Header("발사 연결")]
    [SerializeField] private BossTargetFinder targetFinder;
    [SerializeField] private Transform firePoint;
    [SerializeField] private BossFireballPool fireballPool;

    [Header("발사 설정")]
    [SerializeField, Min(0f)] private float fireShotDelay = 0.5f;
    [SerializeField, Min(0.01f)] private float shotInterval = 0.9f;
    [SerializeField, Min(0f)] private float fireballSpeed =7.4f;
    [SerializeField, Min(0.01f)] private float fireballLifetime = 3f;
    [SerializeField, Min(0)] private int fireballDamage = 8;

    private readonly HashSet<GameObject> activeFireballs = new HashSet<GameObject>();

    private BossFireballPool subscribedPool;
    private bool isShooting;
    private float shotTimeRemaining;
    public int ActiveFireballCount => activeFireballs.Count;

    public bool StartShooting()
    {
        if(isShooting || activeFireballs.Count > 0)
            return false;

        CancelAndRetrunAll();

        if(!isActiveAndEnabled || targetFinder == null || firePoint == null || fireballPool == null || !fireballPool.isActiveAndEnabled)
        {
            Debug.LogError("활성화된 발사기와 대상탐색기, 발사위치, 전용 풀을 확인해 주세요.", this);
            return false;
        }

        subscribedPool = fireballPool;
        subscribedPool.FireballReturned += HandleFireballReturned;

        shotTimeRemaining = Mathf.Max(0f, fireShotDelay);
        isShooting = true;

        return true;
    }

    private void Update()
    {
        if(!isShooting)
            return;

        shotTimeRemaining -= Time.deltaTime;

        if(shotTimeRemaining > 0f)
            return;

        FireOne();
        shotTimeRemaining = Mathf.Max(0.01f, shotInterval);
    }

    private void FireOne()
    {
        if(subscribedPool == null || firePoint == null || targetFinder == null || !targetFinder.TryGetTargetPosition(out Vector2 targetPosition))
            return;

        Vector2 direction = targetPosition - (Vector2)firePoint.position;

        if(direction.sqrMagnitude <= 0.0001f)
            return;

        GameObject fireball = subscribedPool.Rent(firePoint.position, fireballLifetime, direction, fireballSpeed, fireballDamage);

        if(fireball == null)
            return;

        activeFireballs.Add(fireball);

        if(!fireball.TryGetComponent<BossFireball>(out var projectile))
        {
            subscribedPool.Return(fireball);
            return;
        }

        projectile.launch(direction);
    }

    private void HandleFireballReturned(GameObject fireball)
    {
        if (!activeFireballs.Remove(fireball))
            return;

        if(!isShooting && activeFireballs.Count == 0)
            UnsubscribePool();
    }

    public void CancelAndRetrunAll()
    {
        isShooting = false;
        shotTimeRemaining = 0f;

        GameObject[] remaining = new GameObject[activeFireballs.Count];
        activeFireballs.CopyTo(remaining);

        activeFireballs.Clear();

        foreach (GameObject fireball in remaining)
        {
            if(fireball != null && fireball.TryGetComponent<BossFireball>(out var projectile))
            {
                projectile.ReturnToPool();
            }
        }

        if(subscribedPool != null)
            subscribedPool.FireballReturned -= HandleFireballReturned;

        subscribedPool = null;
    }

    private void OnDisable()
    {
        CancelAndRetrunAll();
    }

    public void StopShooting()
    {
        isShooting = false;
        shotTimeRemaining = 0f;

        if(activeFireballs.Count == 0)
            UnsubscribePool();
    }

    private void UnsubscribePool()
    {
        if(subscribedPool != null)
            subscribedPool.FireballReturned -= HandleFireballReturned;

        subscribedPool = null;
    }
}
