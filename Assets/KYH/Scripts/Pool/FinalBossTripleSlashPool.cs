using System;
using System.Collections.Generic;
using UnityEngine;

public class FinalBossTripleSlashPool : MonoBehaviour
{
    [Header("참격 보관")]
    [SerializeField] private FinalBossTripleSlashProjectile projectilePrefab;
    [SerializeField] private Transform storageRoot;

    private readonly Queue<FinalBossTripleSlashProjectile> available = new();
    private readonly HashSet<FinalBossTripleSlashProjectile> active = new();
    private readonly List<FinalBossTripleSlashProjectile> projectiles = new();

    public event Action<FinalBossTripleSlashProjectile> ProjectileReturned;

    private void Awake()
    {
        if (projectilePrefab == null || storageRoot == null)
        {
            Debug.LogError("참격 프리팹과 보관 위치를 연결해 주세요.", this);
            enabled = false;
            return;
        }

        for (int i = 0; i < 3; i++)
        {
            FinalBossTripleSlashProjectile projectile = Instantiate(projectilePrefab, storageRoot);

            projectile.gameObject.SetActive(false);
            projectile.Returned += HandleReturned;

            projectiles.Add(projectile);
            available.Enqueue(projectile);
        }
    }

    public FinalBossTripleSlashProjectile RentAndLaunch(Vector3 position, int direction, float speed, 
        FinalBossTripleSlashData.SlashSettings settings)
    {
        if (!isActiveAndEnabled || storageRoot == null || !storageRoot.gameObject.activeInHierarchy || available.Count == 0)
        {
            return null;
        }

        FinalBossTripleSlashProjectile projectile = available.Dequeue();

        active.Add(projectile);
        projectile.gameObject.SetActive(true);

        if (projectile.Launch(position, direction, speed, settings))
            return projectile;

        projectile.gameObject.SetActive(false);
        active.Remove(projectile);
        available.Enqueue(projectile);

        return null;
    }

    private void HandleReturned(FinalBossTripleSlashProjectile projectile)
    {
        if (!active.Remove(projectile))
            return;

        available.Enqueue(projectile);
        ProjectileReturned?.Invoke(projectile);
    }

    public void ReturnAll()
    {
        FinalBossTripleSlashProjectile[] remaining = new FinalBossTripleSlashProjectile[active.Count];

        active.CopyTo(remaining);

        foreach (FinalBossTripleSlashProjectile projectile in remaining)
            projectile.ReturnToPool();
    }

    private void OnDisable()
    {
        ReturnAll();
    }

    private void OnDestroy()
    {
        foreach (FinalBossTripleSlashProjectile projectile in projectiles)
        {
            if (projectile != null)
                projectile.Returned -= HandleReturned;
        }
    }
}
