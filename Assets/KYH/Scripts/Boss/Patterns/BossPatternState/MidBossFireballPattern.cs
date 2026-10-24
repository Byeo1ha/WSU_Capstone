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

    [Header("발사 순서별 풀: A, B, C")]
    [SerializeField] private BossFireballPool[] fireballPools =
        new BossFireballPool[3];

    [Header("발사 설정")]
    [SerializeField, Min(0f)] private float shotInterval = 0.4f;
    [SerializeField, Min(0.01f)] private float fireballLifetime = 3f;
    [SerializeField, Min(0f)] private float fireballSpeed = 5f;
    [SerializeField, Min(0)] private int fireballDamage = 10;

    private readonly List<BossFireballPool> subscribedPools =
        new List<BossFireballPool>();

    private readonly HashSet<GameObject> activeFireballs =
        new HashSet<GameObject>();

    private Coroutine firingCoroutine;
    private bool hasFinishedFiring;
    private bool ownsExecution;

    private void OnEnable()
    {
        if (!ValidateReferences())
        {
            enabled = false;
            return;
        }

        for (int i = 0; i < fireballPools.Length; i++)
        {
            BossFireballPool pool = fireballPools[i];
            subscribedPools.Add(pool);
            pool.FireballReturned += HandleFireballReturned;
        }

        patternRunner.StateChanged += HandleStateChanged;
    }

    private void OnDisable()
    {
        if (patternRunner != null)
            patternRunner.StateChanged -= HandleStateChanged;

        bool shouldStopRunner = ownsExecution;

        CancelExecution();

        for (int i = 0; i < subscribedPools.Count; i++)
        {
            BossFireballPool pool = subscribedPools[i];

            if (pool != null)
                pool.FireballReturned -= HandleFireballReturned;
        }

        subscribedPools.Clear();

        if (shouldStopRunner && patternRunner != null)
            patternRunner.StopPattern();
    }

    private bool ValidateReferences()
    {
        if (patternRunner == null ||
            targetFinder == null ||
            firePoint == null)
        {
            Debug.LogError(
                "Pattern Runner, Target Finder, Fire Point를 연결해 주세요.",
                this);
            return false;
        }

        if (fireballPools == null || fireballPools.Length != 3)
        {
            Debug.LogError("Fireball Pools는 A, B, C 세 칸이어야 합니다.", this);
            return false;
        }

        for (int i = 0; i < fireballPools.Length; i++)
        {
            if (fireballPools[i] == null)
            {
                Debug.LogError($"Fireball Pools의 {i}번을 연결해 주세요.", this);
                return false;
            }

            for (int j = 0; j < i; j++)
            {
                if (fireballPools[i] == fireballPools[j])
                {
                    Debug.LogError("A, B, C는 서로 다른 풀을 연결해 주세요.", this);
                    return false;
                }
            }
        }

        return true;
    }

    private void HandleStateChanged(BossPatternState state)
    {
        if(state == BossPatternState.Windup && !patternRunner.IsSelectedPattern(this))
        {
            return;
        }
        
        switch (state)
        {
            case BossPatternState.Windup:
                CancelExecution();
                ownsExecution = true;
                break;

            case BossPatternState.Active:
                if (!ownsExecution || firingCoroutine != null)
                    return;

                hasFinishedFiring = false;
                firingCoroutine = StartCoroutine(FireSequence());
                break;

            case BossPatternState.Recovery:
            case BossPatternState.Idle:
                CancelExecution();
                break;
        }
    }

    private IEnumerator FireSequence()
    {
        for (int i = 0; i < subscribedPools.Count; i++)
        {
            if (!ownsExecution ||
                patternRunner.CurrentState != BossPatternState.Active)
                break;

            FireOne(subscribedPools[i]);

            if (i < subscribedPools.Count - 1)
                yield return new WaitForSeconds(shotInterval);
        }

        hasFinishedFiring = true;

        // StartCoroutine 호출 결과가 필드에 저장될 때까지 한 프레임 기다립니다.
        yield return null;

        firingCoroutine = null;
        TryCompleteActive();
    }

    private void FireOne(BossFireballPool pool)
    {
        if (pool == null ||
            firePoint == null ||
            targetFinder == null ||
            !targetFinder.TryGetTargetPosition(out Vector2 targetPosition))
            return;

        Vector2 direction =
            targetPosition - (Vector2)firePoint.position;

        if (direction.sqrMagnitude <= 0.0001f)
            return;

        GameObject fireball = pool.Rent(
            firePoint.position,
            fireballLifetime,
            direction,
            fireballSpeed,
            fireballDamage);

        if (fireball == null)
        {
            Debug.LogWarning("파이어볼 대여에 실패했습니다.", this);
            return;
        }

        activeFireballs.Add(fireball);

        if (!fireball.TryGetComponent<BossFireball>(out var projectile))
        {
            pool.Return(fireball);
            Debug.LogError("파이어볼 루트에 BossFireball이 필요합니다.", this);
            return;
        }

        projectile.launch(direction);
    }

    private void HandleFireballReturned(GameObject fireball)
    {
        if (!activeFireballs.Remove(fireball))
            return;

        TryCompleteActive();
    }

    private void TryCompleteActive()
    {
        if (!isActiveAndEnabled ||
            !ownsExecution ||
            !hasFinishedFiring ||
            activeFireballs.Count > 0 ||
            patternRunner == null ||
            patternRunner.CurrentState != BossPatternState.Active)
            return;

        patternRunner.CompleteActive();
    }

    private void CancelExecution()
    {
        ownsExecution = false;
        hasFinishedFiring = false;

        if (firingCoroutine != null)
        {
            StopCoroutine(firingCoroutine);
            firingCoroutine = null;
        }

        GameObject[] remaining = new GameObject[activeFireballs.Count];
        activeFireballs.CopyTo(remaining);

        // 반환 알림이 발생하기 전에 추적을 비웁니다.
        activeFireballs.Clear();

        foreach (GameObject fireball in remaining)
        {
            if (fireball != null &&
                fireball.TryGetComponent<BossFireball>(out var projectile))
            {
                projectile.ReturnToPool();
            }
        }
    }
}