using System;
using UnityEngine;

public class MidBossBombFieldPool : MonoBehaviour
{
    [Header("구체 보관")]
    [SerializeField] private MidBossBombFieldProjectile projectilePrefab;
    [SerializeField] private Transform storageRoot;
    
    [Header("수동 테스트")]
    [SerializeField] private MidBossBombFieldData testData;
    [SerializeField] private BossTargetFinder testTargetFinder;
    [SerializeField] private Transform testSpawnPoint;

    private MidBossBombFieldProjectile projectile;
    private bool isRented;

    public event Action<Vector3, float> ExplosionFinished;

    private void Awake()
    {
        if (projectilePrefab == null || storageRoot == null)
        {
            Debug.LogError("BombField 구체 프리팹과 보관 위치를 연결해 주세요.", this);
            enabled = false;
            return;
        }

        projectile = Instantiate(projectilePrefab, storageRoot);
        projectile.gameObject.SetActive(false);

        projectile.ExplosionFinished += HandleExplosionFinished;
    }

    public MidBossBombFieldProjectile Rent(Vector3 position)
    {
        if (!isActiveAndEnabled || projectile == null ||
            isRented || !storageRoot.gameObject.activeInHierarchy)
        {
            return null;
        }

        isRented = true;

        projectile.gameObject.SetActive(true);
        projectile.PrepareAt(position);

        return projectile;
    }

    public void Return()
    {
        if (!isRented || projectile == null)
            return;

        isRented = false;

        projectile.GrowthFinished -= HandleTestGrowthFinished;

        projectile.Stop();
        projectile.gameObject.SetActive(false);
    }

    private void HandleExplosionFinished(MidBossBombFieldProjectile finishedProjectile)
    {
        if (!isRented || finishedProjectile != projectile)
            return;

        Vector3 explosionPosition = finishedProjectile.transform.position;
        float explosionRadius = finishedProjectile.ExplosionRadius;

        Return();

        ExplosionFinished?.Invoke(explosionPosition, explosionRadius);
    }

    private void OnDisable()
    {
        Return();
    }

    [ContextMenu("테스트/BombField 구체 발사")]
    private void TestLaunch()
    {
        if (!Application.isPlaying || testData == null ||
            testTargetFinder == null || testSpawnPoint == null)
        {
            return;
        }

        MidBossBombFieldProjectile rentedProjectile = Rent(testSpawnPoint.position);

        if (rentedProjectile == null)
            return;

        rentedProjectile.GrowthFinished += HandleTestGrowthFinished;

        if (!rentedProjectile.BeginGrowth(testData.GrowthDuration))
            Return();
    }

    private void HandleTestGrowthFinished(MidBossBombFieldProjectile grownProjectile)
    {
        grownProjectile.GrowthFinished -= HandleTestGrowthFinished;

        if (!testTargetFinder.TryGetTargetPosition(out Vector2 targetPosition))
        {
            Return();
            return;
        }

        Vector2 direction = targetPosition - (Vector2)grownProjectile.transform.position;

        if (!grownProjectile.Launch(direction, testData))
            Return();
    }

    private void OnDestroy()
    {
        if (projectile != null)
            projectile.ExplosionFinished -= HandleExplosionFinished;
    }
}
