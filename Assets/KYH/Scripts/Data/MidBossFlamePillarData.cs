using System.IO.Enumeration;
using UnityEngine;

[CreateAssetMenu(
    fileName = "MidBossFlamePillarData",
    menuName = "KYH/Boss/Flame Pillar Data"
)]
    

public class MidBossFlamePillarData : ScriptableObject
{
    [Header("경고시간")]
    [SerializeField, Min(0f)] private float warningDuration = 0.9f;

    [Header("공격 유지 시간 - 임시값")]
    [SerializeField, Min(0.01f)] private float activeDuration = 0.4f;

    [Header("피해")]
    [SerializeField, Min(0)] private int damage = 14;

    [Header("공격 크기 - 임시값")]
    [SerializeField, Min(0.01f)] private float width = 1f;
    [SerializeField, Min(0.01f)] private float height = 3f;

    public float WarningDuration => warningDuration;
    public float ActiveDuration => activeDuration;
    public int Damage => damage;
    public Vector2 Size => new Vector2(width, height);
}
