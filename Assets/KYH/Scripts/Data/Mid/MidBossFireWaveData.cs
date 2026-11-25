using UnityEngine;

[CreateAssetMenu(
    fileName = "MidBossFireWaveData",
    menuName = "Boss/Mid Boss Fire Wave Data")]
public class MidBossFireWaveData : ScriptableObject
{
    [Header("첫 기둥 위치")]
    [SerializeField, Min(0f)] private float startOffsetX = 0.5f;
    [SerializeField] private float spawnOffsetY;

    [Header("기둥 범위와 배치")]
    [SerializeField, Min(0.1f)] private float pillarWidth = 2f;
    [SerializeField, Min(0.1f)] private float pillarSpacing = 2f;

    [Header("기둥 생성 전 경고")]
    [SerializeField, Min(0f)] private float warningDuration = 0.5f;

    public float StartOffsetX => startOffsetX;
    public float SpawnOffsetY => spawnOffsetY;
    public float PillarWidth => pillarWidth;
    public float PillarSpacing => pillarSpacing;
    public float WarningDuration => warningDuration;

    [Header("피해 설정")]
    [SerializeField, Min(0)] private int damage = 10;
    [SerializeField, Min(0.1f)] private float damageHeight = 3f;
    [SerializeField] private Vector2 damageOffset; 

    public int Damage => damage;
    public Vector2 DamageSize => new Vector2(pillarWidth, damageHeight);
    public Vector2 DamageOffset => damageOffset;
}
