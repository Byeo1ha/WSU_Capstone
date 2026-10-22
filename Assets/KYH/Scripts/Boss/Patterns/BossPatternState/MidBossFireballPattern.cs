using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MidBossFireballPattern : MonoBehaviour
{
    [Header("패턴 실행기")]
    [SerializeField] private BossPatternRunner patternRunner;
    [Header("대상 탐색")]
    [SerializeField] private BossTargetFinder targetFinder;
    [Header("발사 위치")]
    [SerializeField] private Transform firePoint;
    [Header("공유 파이어볼 풀")]
    [SerializeField] private BossFireballPool fireballPool;
    [Header("발사 순서별 파이어볼 풀")]
    [SerializeField] private BossFireballPool[] fireballPools = new BossFireballPool[3];
    [Header("연속 발사 설정")]
    [SerializeField, Min(1)]private int shotCount =3;
    [SerializeField, Min(0f)]private float shotInterval = 0.4f;



    private Coroutine firingCoroutine;
    private bool hasFinishedFiring;

    private readonly HashSet<GameObject> activeFireballs = new HashSet<GameObject>();

    private void OnEnable()
    {
        patternRunner.StateChanged += HandleStateChanged;
        fireballPool.FireballReturned += HandleFireballReturned;
    }

    private void OnDisable()
    {
        hasFinishedFiring = false;
        StopFiring();

        if(fireballPool != null)
        {
            fireballPool.FireballReturned -= HandleFireballReturned;
        }

        activeFireballs.Clear();

        if(patternRunner != null)
        {
            patternRunner.StateChanged -= HandleStateChanged;
            patternRunner.StopPattern();
        }
    }

    private void HandleStateChanged(BossPatternState state)
    {
        if(state == BossPatternState.Windup)
        {
            if(firePoint == null || fireballPool == null)
            {
                Debug.LogError("Fire Poin와 Fireball Pool을 연결해 주세요",  this);
                patternRunner.StopPattern();
                return;
            }

            hasFinishedFiring = false;
            return;
        }

        if(state != BossPatternState.Active)
        {
            StopFiring();
            if(state == BossPatternState.Idle)
            {
                hasFinishedFiring = false;
                activeFireballs.Clear();
            }
            return;   
        }

        if(firePoint == null || fireballPool == null)
        {
            Debug.LogError("Fire Point와 Fireball pool을 연결해 주세요", this);
                return;
        }

        if(firingCoroutine != null)
            return;

        hasFinishedFiring = false;
        firingCoroutine = StartCoroutine(FireSequence());
    }

    private IEnumerator FireSequence()
    {
        for(int i = 0; i < shotCount; i++)
        {
            GameObject fireball = fireballPool.Rent(firePoint.position, 3f, Vector2.right, 5f);
            
            if (fireball != null)
            {
                activeFireballs.Add(fireball);

                BossFireball projectile = fireball.GetComponent<BossFireball>();

                if(targetFinder != null && targetFinder.TryGetTargetPosition(out Vector2 targetPosition))
                {
                    Vector2 direction = targetPosition - (Vector2)fireball.transform.position;

                    if(direction.sqrMagnitude > 0.0001f)
                        projectile.launch(direction);
                    else
                        projectile.ReturnToPool();
                }
                else
                {
                    projectile.ReturnToPool();
                }
            }
            if (i < shotCount - 1)
                yield return new WaitForSeconds(shotInterval);
        }

        hasFinishedFiring = true;

        yield return null;
        firingCoroutine = null;

        TryCompleteActive();
    }

    private void StopFiring()
    {
        if (firingCoroutine == null)
            return;

        StopCoroutine(firingCoroutine);
        firingCoroutine = null;
    }
    private void HandleFireballReturned(GameObject fireball)
    {
        if(!activeFireballs.Remove(fireball))
            return;

        Debug.Log($"파이어볼 반환 알림: {fireball.name}, 남은 탄: {activeFireballs.Count}", this);
        TryCompleteActive();
    }

    private void TryCompleteActive()
    {
        if(!isActiveAndEnabled || patternRunner.CurrentState != BossPatternState.Active)
            return;


        if(!hasFinishedFiring || activeFireballs.Count > 0)
            return;

        patternRunner.CompleteActive();
    }
}
