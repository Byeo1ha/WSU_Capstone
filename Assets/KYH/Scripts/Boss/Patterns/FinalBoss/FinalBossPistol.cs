using UnityEngine;

public class FinalBossPistol : MonoBehaviour
{
    [Header("권총 전용 풀")]
    [SerializeField] private BossFireballPool pistolpool;
    [Header("발사 기준")]
    [SerializeField] private Transform fireOrigin;
    [SerializeField] private Vector2 muzzleOffset = new Vector2(0.7f, 0f);
    [Header("탄환 설정 - 임시값")]
    [SerializeField, Min(0.1f)] private float speed = 12f;
    [SerializeField, Min(0.1f)] private float lifetime = 3f;

    private BossFireballPool subscribedPool;
    private GameObject currentShot;

    public bool IsInFlight => currentShot != null && currentShot.activeInHierarchy;

    private void OnEnable()
    {
        if(pistolpool == null || fireOrigin == null)
        {
            Debug.LogError("권총 풀과 발사 기준을 연결해 주세요", this);

            enabled = false;
            return;
        }

        subscribedPool = pistolpool;
        subscribedPool.FireballReturned += HandleReturned;
    }

    private void OnDisable()
    {
        CancelShot();

        if(subscribedPool != null)
            subscribedPool.FireballReturned -= HandleReturned;

        subscribedPool = null;
    }

    public bool TryFire(int direction, int damage)
    {
        if (!isActiveAndEnabled ||
            subscribedPool == null ||
            !subscribedPool.isActiveAndEnabled ||
            fireOrigin == null ||
            currentShot != null)
            return false;

        int facing = direction < 0 ? -1 : 1;

        Vector3 position = fireOrigin.position;
        position.x += muzzleOffset.x * facing;
        position.y += muzzleOffset.y;

        Vector2 moveDirection = new Vector2(facing, 0f);

        GameObject shot = subscribedPool.Rent(
            position,
            lifetime,
            moveDirection,
            speed,
            damage);

        if (shot == null)
            return false;

        if (!shot.TryGetComponent<BossFireball>(out var projectile))
        {
            subscribedPool.Return(shot);
            Debug.LogError(
                "권총 탄환에 BossFireball이 필요합니다.", this);

            return false;
        }

        currentShot = shot;
        projectile.launch(moveDirection);

        return true;
    }

    private void HandleReturned(GameObject shot)
    {
        if (currentShot == shot)
            currentShot = null;
    }

    public void CancelShot()
    {
        GameObject shot = currentShot;

        currentShot = null;

        if (shot != null && subscribedPool != null)
            subscribedPool.Return(shot);
    }
}
