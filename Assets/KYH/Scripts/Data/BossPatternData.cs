using UnityEngine;

[CreateAssetMenu
(
    fileName = "BossPatternData",
    menuName = "KYH/Boss/PatternData")]

public class BossPatternData : ScriptableObject
{
    [Header("단계별 시간")]
    [SerializeField, Min(0f)] private float windupDuration = 1f;
    [SerializeField, Min(0f)] private float activeDuration = 0.5f;
    [SerializeField, Min(0f)] private float recoveryDuration = 1f;

    public float WindupDuration => windupDuration;
    public float ActiveDuration => activeDuration;
    public float RecoveryDuration => recoveryDuration;
}
