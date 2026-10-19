using UnityEngine;
using System.Collections;

public class MidBossFireballPattern : MonoBehaviour
{
    [Header("패턴 실행기")]
    [SerializeField] private BossPatternRunner patternRunner;
    [Header("발사 위치")]
    [SerializeField] private Transform firePoint;
    [Header("공유 파이어볼 풀")]
    [SerializeField] private BossFireballPool fireballPool;
    [Header("연속 발사 설정")]
    [SerializeField, Min(1)]private int shotCount =3;
    [SerializeField, Min(0f)]private float shotInterval = 0.4f;

    private Coroutine firingCoroutine;

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

        if(firingCoroutine != null)
            return;

        firingCoroutine = StartCoroutine(FireSequence());
    }

    private IEnumerator FireSequence()
    {
        for(int i = 0; i < shotCount; i++)
        {
            fireballPool.Rent(firePoint.position, 3f, Vector2.right, 5f);

            if (i < shotCount - 1)
                yield return new WaitForSeconds(shotInterval);
        }

        firingCoroutine = null;
    }
}
