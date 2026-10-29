using System.Collections;
using UnityEngine;
using UnityEngine.Analytics;

public class FinalBossComboPattern : MonoBehaviour
{
    [Header("실행 연결")]
    [SerializeField] private BossPatternRunner patternRunner;
    [SerializeField] private BossHealth bossHealth;
    [SerializeField] private FinalBossPhaseController phaseController;

    [Header("공격 연결")]
    [SerializeField] private BossTargetFinder targetFinder;
    [SerializeField] private FinalBossComboData data;
    [SerializeField] private FinalBossSlash slash;
    [SerializeField] private FinalBossPistol pistol;

    private Coroutine sequence;
    private bool ownsExecution;
    
    private void OnEnable()
    {
        if(patternRunner == null || bossHealth == null || phaseController == null || targetFinder == null || data == null || slash == null || pistol == null)
        {
            Debug.LogError("칼 연계에 필요한 참조를 모두 연결해 주세요.", this);

            enabled = false;
            return;
        }

        patternRunner.StateChanged += HandleStateChanged;
        bossHealth.HealthChanged += HandleHealthChanged;
    }

    private void OnDisable()
    {
        if(patternRunner != null)
            patternRunner.StateChanged -= HandleStateChanged;

        if(bossHealth != null)
            bossHealth.HealthChanged -= HandleHealthChanged;

        bool shouldStopRunner = ownsExecution;
        CancelExecution();

        if(shouldStopRunner && patternRunner != null)
            patternRunner.StopPattern();
    }

    private bool CanExecute()
    {
        return isActiveAndEnabled &&
               patternRunner.isActiveAndEnabled &&
               !bossHealth.IsDead &&
               phaseController.isActiveAndEnabled &&
               !phaseController.IsTransforming &&
               phaseController.CurrentPhase == FinalBossPhase.Phase1 &&
               targetFinder.isActiveAndEnabled &&
               slash.isActiveAndEnabled &&
               pistol != null && pistol.isActiveAndEnabled;
    }

    private void HandleStateChanged(BossPatternState state)
    {
        switch(state)
        {
            case BossPatternState.Windup:
                if (!patternRunner.IsSelectedPattern(this))
                    return;

                CancelExecution();
                ownsExecution = true;

                if(!CanExecute())
                    AbortPattern();
                
                break;

            case BossPatternState.Active:
                if(ownsExecution)
                    BeginCombo();

                break;

            case BossPatternState.Recovery:
            case BossPatternState.Idle:
                CancelExecution();
                break;
        }
    }

    private void BeginCombo()
    {
        if(sequence != null)
            return;

        if (!CanExecute())
        {
            AbortPattern();
            return;
        }

        if(!targetFinder.TryGetTargetPosition(out Vector2 targetPosition))
        {
            Debug.LogWarning("칼 연계의 대상을 찾지 못했습니다.", this);
            AbortPattern();
            return;
        }   

        int direction = targetPosition.x < slash.transform.position.x ? -1 : 1;

        sequence = StartCoroutine(PlayCombo(direction));
    }

    private IEnumerator PlayCombo(int direction)
    {
        yield return null;

        for(int i = 0; i < data.SlashCount; i++)
        {
            if(!ownsExecution)
                yield break;

            if(!CanExecute() || !slash.BeginSlash(direction))
            {
                AbortPattern();
                yield break;
            }

            if(!ownsExecution)
                yield break;

            Debug.Log($"최종보스 칼 공: {i + 1}타.", this);

            while (slash.IsAttacking)
            {
                if(!ownsExecution)
                    yield break;

                if(!CanExecute())
                {
                    AbortPattern();
                    yield break;
                }

                yield return null;
            }

            if(i < data.SlashCount - 1)
                yield return new WaitForSeconds(data.BetweenSlashDelay);
        }

         yield return new WaitForSeconds(data.BeforePistolDelay);

        if (!ownsExecution)
            yield break;

        if (!CanExecute() ||
            !pistol.TryFire(direction, data.PistolDamage))
        {
            Debug.LogWarning("권총 발사를 시작하지 못했습니다.", this);
            AbortPattern();
            yield break;
        }

        Debug.Log("최종보스 권총 발사", this);

        while (pistol.IsInFlight)
        {
            if (!ownsExecution)
                yield break;

            if (!CanExecute())
            {
                AbortPattern();
                yield break;
            }

            yield return null;
        }

        sequence = null;

        if (!ownsExecution)
            yield break;

        if (!CanExecute())
        {
            AbortPattern();
            yield break;
        }

        patternRunner.CompleteActive();
    }

    private void HandleHealthChanged(int currentHealth, int maxHealth)
    {
        if(currentHealth <= 0 && ownsExecution)
            AbortPattern();
    }

    private void CancelExecution()
    {
        bool wasExecuting = ownsExecution;
        ownsExecution = false;

        if(sequence != null)
        {
            StopCoroutine(sequence);
            sequence = null;
        }

        if (wasExecuting)
        {
            if (slash != null)
                slash.StopSlash();

            if (pistol != null)
                pistol.CancelShot();
        }
    }

    private void AbortPattern()
    {
        bool shouldStopRunner = ownsExecution;
        CancelExecution();

        if(shouldStopRunner && patternRunner != null)
            patternRunner.StopPattern();
    }
}
