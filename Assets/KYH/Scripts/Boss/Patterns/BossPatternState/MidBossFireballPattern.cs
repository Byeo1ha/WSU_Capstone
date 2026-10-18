using UnityEngine;

public class MidBossFireballPattern : MonoBehaviour
{
    [Header("패턴 실행기")]
    [SerializeField] private BossPatternRunner patternRunner;
    [Header("발사 위치")]
    [SerializeField] private Transform firePoint;
    [Header("공유 파이어볼 풀")]
    [SerializeField] private BossFireballPool fireballPool;

    private void OnEnable()
    {
        patternRunner.StateChanged += HandleStateChanged;
    }

    private void OnDisable()
    {
        patternRunner.StateChanged -= HandleStateChanged;
    }

    private void HandleStateChanged(BossPatternState state)
    {
        if(state != BossPatternState.Active)
            return;

        if(firePoint == null || fireballPool == null)
        {
            Debug.LogError("Fire Point와 Fireball pool을 연결해 주세요", this);
                return;
        }

        GameObject fireball = fireballPool.Rent(firePoint.position, 3f, Vector2.right, 5f);

        if(fireball == null)
        {
            Debug.LogWarning("파이어볼을 대여하지 못했습니다.", this);
            return;
        }
    }
}
