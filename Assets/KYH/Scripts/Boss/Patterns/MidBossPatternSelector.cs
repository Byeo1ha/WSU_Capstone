using System.Collections.Generic;
using UnityEngine;

public class MidBossPatternSelector : MonoBehaviour
{
    [Header("패턴 실행기")]
    [SerializeField] private BossPatternRunner patternRunner;

    [Header("파이어볼, 화염기둥, 토템")]
    [SerializeField] private MonoBehaviour[] patterns = new MonoBehaviour[3];

    [Header("첫 패턴 시작 대기")]
    [SerializeField, Min(0f)] private float firstPatternDelay = 1.5f;

    private readonly List<MonoBehaviour> candidates = new List<MonoBehaviour>();

    private MonoBehaviour lastPattern;
    private float timeRemaining;

    private void OnEnable()
    {
        if (!ValidateReferences())
        {
            enabled = false;
            return;
        }

        lastPattern = null;
        timeRemaining = Mathf.Max(0f, firstPatternDelay);
    }

    private bool ValidateReferences()
    {
        if(patternRunner == null || patterns == null || patterns.Length != 3)
        {
            Debug.LogError("실행기와 서로 다른 세 패턴을 연결해 주세요.", this);
            return false;
        }

        for(int i = 0; i < patterns.Length; i++)
        {
            if(patterns[i] == null)
            {
                Debug.LogError($"패턴 배열의 {i}번을 연결해 주세요.", this);
                return false;
            }

            for(int j = 0; j < i; j++)
            {
                if(patterns[i] == patterns[j])
                {
                    Debug.LogError("같은 패턴을 중복 연결할 수 없습니다.", this);
                    return false;
                }
            }
        }

        return true;
    }

    private void Update()
    {
        if(patternRunner == null || !patternRunner.IsStartAllowed || patternRunner.CurrentState != BossPatternState.Idle)
            return;

        timeRemaining = Mathf.Max(0f, timeRemaining - Time.deltaTime);

        if(timeRemaining > 0f)
            return;

        StartNextPattern();
    }

    private void StartNextPattern()
    {
        candidates.Clear();

        foreach(MonoBehaviour pattern in patterns)
        {
            if(pattern != null && pattern.isActiveAndEnabled && pattern != lastPattern)
            {
                candidates.Add(pattern);
            }
        }

        if(candidates.Count == 0)
        {
            Debug.LogWarning("직전 패턴을 제외한 실행 가능한 패턴이 없습니다. 자동 선택을 중단합니다.", this);

            enabled = false;
            return;
        }

        int index = Random.Range(0, candidates.Count);
        MonoBehaviour selected = candidates[index];

        if(!patternRunner.TrySelectPattern(selected))
            return;

        patternRunner.StartPattern();

        if(patternRunner.CurrentState != BossPatternState.Windup)
        {
            Debug.LogWarning("패턴 시작에 실패했습니다. 연결과 설정을 확인해 주세요.", this);

            enabled = false;
            return;
        }

        lastPattern = selected;

        Debug.Log($"중간보스 패턴 선택: {selected.GetType().Name}", this);
    }
}
