using UnityEngine;

[CreateAssetMenu( fileName = "MidBossStaffBurstData",
                  menuName = "Boss/Mid Boss Staff Burst Data")]
public class MidBossStaffBurstData : ScriptableObject
{
    [Header("광선 범위")]
    [SerializeField, Min(0.01f)] private float beamWidth = 0.3f;
    [SerializeField, Min(0.1f)] private float maxDistance = 30f;

    [Header("레이저 발사 시간과 흐르는 속도")]
    [SerializeField, Min(0.01f)] private float emissionDuration = 0.3f;
    [SerializeField, Min(0.01f)] private float moveSpeed = 120f;

    [Header("패턴 전체에서 대상당 1회 피해")]
    [SerializeField, Min(0)] private int damage = 12;

    public float BeamWidth => beamWidth;
    public float MaxDistance => maxDistance;
    public float EmissionDuration => emissionDuration;
    public float MoveSpeed => moveSpeed;
    public int Damage => damage;
}