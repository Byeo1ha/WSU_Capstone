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
    [Header("연속 발사 설정")]
    [SerializeField, Min(1)]private int shotCount =3;
    [SerializeField, Min(0f)]private float shotInterval = 0.4f;
    [Header("준비 구체 배치")]
    [SerializeField, Min(0f)]private float preparationSpacing = 0.8f;
    [SerializeField, Min(0f)]private float centerHeightOffset = 0.3f;
    [Header("준비 구체 생성")]
    [SerializeField, Min(0f)]private float preparationDuration = 0.8f;

    private Coroutine firingCoroutine;
    private Coroutine preparationCoroutine;
    private bool hasFinishedFiring;

    private readonly HashSet<GameObject> activeFireballs = new HashSet<GameObject>();
    private readonly List<GameObject> preparedFireballs = new List<GameObject>();

    private void OnEnable()
    {
        patternRunner.StateChanged += HandleStateChanged;
        fireballPool.FireballReturned += HandleFireballReturned;
    }

    private void OnDisable()
    {
        StopPreparation();
        StopFiring();

        if(fireballPool != null)
        {
            ReturnPreparedFireballs();
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
            preparationCoroutine = StartCoroutine(PrepareSequence());
            return;
        }

        if(state != BossPatternState.Active)
        {
            StopFiring();
            if(state == BossPatternState.Idle)
            {
                StopPreparation();
                ReturnPreparedFireballs();
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

    private Vector3 GetPreparationPosition(int index)
    {
        float offsetX = (index - (shotCount - 1) * 0.5f) * preparationSpacing;

        float offsetY = 0f;

        if (shotCount % 2 == 1 && index == shotCount / 2)
            offsetY = centerHeightOffset;

        return firePoint.position + new Vector3(offsetX, offsetY, 0f);
    }

    private void CreatePreparedFireball(int index)
    {
        Vector3 position = GetPreparationPosition(index);

        GameObject fireball = fireballPool.Rent(position, 3f, Vector2.right, 5f);

        if (fireball == null)
            return;

        activeFireballs.Add(fireball);
        preparedFireballs.Add(fireball);
        
        BossFireball projectile = fireball.GetComponent<BossFireball>();
        Vector3 offset = position - firePoint.position;
        projectile.SetPreparationPoint(firePoint, offset);
    }

    private IEnumerator PrepareSequence()
    {
        float interval = shotCount > 1 ? preparationDuration / (shotCount - 1) : 0f;

        for(int i = 0; i < shotCount; i++)
        {
            CreatePreparedFireball(i);

            if(i < shotCount - 1 && interval > 0f)
                yield return new WaitForSeconds(interval);
        }

        yield return null;
        preparationCoroutine = null;
    }

    private IEnumerator FireSequence()
    {
        while (preparationCoroutine != null)
            yield return null;

        while (preparedFireballs.Count > 0)
        {
            GameObject fireball = preparedFireballs[0];
            preparedFireballs.RemoveAt(0);
            
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
            if (preparedFireballs.Count > 0)
                yield return new WaitForSeconds(shotInterval);
        }

        hasFinishedFiring = true;

        yield return null;
        hasFinishedFiring = true;
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

    private void StopPreparation()
    {
        if(preparationCoroutine == null)
            return;

        StopCoroutine(preparationCoroutine);
        preparationCoroutine = null;
    }

    private void ReturnPreparedFireballs()
    {
        hasFinishedFiring = false;

        while (preparedFireballs.Count > 0)
        {
            int lastIndex = preparedFireballs.Count - 1;
            GameObject fireball = preparedFireballs[lastIndex];

            preparedFireballs.RemoveAt(lastIndex);

            if(fireball != null)
                fireballPool.Return(fireball);
        }
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
