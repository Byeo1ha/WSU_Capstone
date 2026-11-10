using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MidBossPatternSelector : MonoBehaviour
{
    [Header("패턴 실행기")]
    [SerializeField] private BossPatternRunner patternRunner;

    [Header("파이어볼, 화염기둥, 토템")]
    [SerializeField] private MonoBehaviour[] patterns = new MonoBehaviour[3];
    [Header("블링크 직후 고정 패턴")]
    [SerializeField] private MidBossFireballPattern fireballPattern;

    [Header("첫 패턴 시작 대기")]
    [SerializeField, Min(0f)] private float firstPatternDelay = 1.5f;
    [Header("예약된 블링크")]
    [SerializeField] private MidBossBlinkController blinkController;
    [Header("블링크 직후 준비시간 배율")]
    [SerializeField, Range(0.01f, 1f)]
    private float windupScaleAfterBlink = 0.3f;

    public bool CurrentPatternFollowsBlink {get; private set;}

    private readonly List<MonoBehaviour> candidates = new List<MonoBehaviour>();

    private MonoBehaviour lastPattern;
    private float timeRemaining;
    private bool waitingForBlink;

    private void OnEnable()
    {
        if (!ValidateReferences())
        {
            enabled = false;
            return;
        }

        lastPattern = null;
        waitingForBlink = false;
        CurrentPatternFollowsBlink = false;
        timeRemaining = Mathf.Max(0f, firstPatternDelay);
    }

    private void OnDisable()
    {
        if (waitingForBlink && blinkController != null)
            blinkController.CancelBlink();

        waitingForBlink = false;
    }

    private bool ValidateReferences()
    {
        if(fireballPattern == null)
        {
            Debug.LogError("블링크 직후 사용할 파이어볼 패턴을 연결해 주세요.", this);
            return false;
        }
        if(blinkController == null)
        {
            Debug.LogError("블링크 관리기를 연결해 주세요.", this);
            return false;
        }
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
    if (patternRunner == null ||
        !patternRunner.IsStartAllowed ||
        patternRunner.CurrentState != BossPatternState.Idle)
    {
        if (waitingForBlink)
        {
            blinkController.CancelBlink();
            waitingForBlink = false;
        }

        return;
    }

    if (waitingForBlink)
    {
        if (blinkController.IsBlinking)
            return;

        waitingForBlink = false;

        if (!blinkController.BlinkSucceeded)
        {
            Debug.LogWarning("블링크가 중단되거나 이동에 실패했습니다. 연결을 확인해 주세요.", this);
            enabled = false;
            return;
        }

        StartNextPattern(true);
        return;
    }

    timeRemaining = Mathf.Max(
        0f, timeRemaining - Time.deltaTime);

    if (timeRemaining > 0f)
        return;

    StartNextPattern();
}

    private void StartNextPattern(bool followsBlink = false)
{
    if (!followsBlink && blinkController.PendingBlinkCount > 0)
    {
        if (!fireballPattern.isActiveAndEnabled)
        {
            Debug.LogWarning(
                "블링크 후 사용할 파이어볼 패턴을 활성화해 주세요.",
                this);
            enabled = false;
            return;
        }

        if (!blinkController.TryBeginBlink())
        {
            Debug.LogWarning(
                "블링크 모션을 시작하지 못했습니다. 연결을 확인해 주세요.",
                this);
            enabled = false;
            return;
        }

        waitingForBlink = true;
        return;
    }

    MonoBehaviour selected;

    if (followsBlink)
    {
        selected = fireballPattern;
    }
    else
    {
        candidates.Clear();

        foreach (MonoBehaviour pattern in patterns)
        {
            if (pattern != null && pattern.isActiveAndEnabled && pattern != lastPattern)
            {
                candidates.Add(pattern);
            }
        }

        if (candidates.Count == 0)
        {
            Debug.LogWarning("직전 패턴을 제외한 실행 가능한 패턴이 없습니다.", this);
            enabled = false;
            return;
        }

        selected = candidates[Random.Range(0, candidates.Count)];
    }

    if (!patternRunner.TrySelectPattern(selected))
        return;

    CurrentPatternFollowsBlink = followsBlink;

    float windupScale = followsBlink ? windupScaleAfterBlink : 1f;

    patternRunner.StartPattern(windupScale);

    if (patternRunner.CurrentState != BossPatternState.Windup)
    {
        Debug.LogWarning("패턴 시작에 실패했습니다. 연결과 설정을 확인해 주세요.", this);
        enabled = false;
        return;
    }

    lastPattern = selected;

    Debug.Log($"중간보스 패턴 선택: {selected.GetType().Name}", this);
}
}
