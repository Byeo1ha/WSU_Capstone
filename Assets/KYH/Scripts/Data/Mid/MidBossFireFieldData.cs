using UnityEngine;

[CreateAssetMenu(fileName = "MidBossFireFieldData", menuName = "Boss/Mid Boss Fire Field Data")]
public class MidBossFireFieldData : ScriptableObject
{
    [Header("장판 생성")]
    [SerializeField, Min(1)] private int spawnCount = 3;
    [SerializeField, Min(0.01f)] private float spawnInterval = 0.5f;
    [SerializeField, Min(0f)] private float warningDuration = 0.2f;

    [Header("장판 유지")]
    [SerializeField, Min(0.01f)] private float activeDuration = 3f;
    [SerializeField] private float visualOffsetY;

    [Header("피해 설정")]
    [SerializeField, Min(0)] private int damage = 4;
    [SerializeField, Min(0.01f)] private float damageInterval = 0.5f;
    [SerializeField] private Vector2 damageSize = new Vector2(1.5f, 0.5f);
    [SerializeField] private Vector2 damageOffset = new Vector2(0f, 0.25f);

    public int SpawnCount => spawnCount;
    public float SpawnInterval => spawnInterval;
    public float WarningDuration => warningDuration;
    public float ActiveDuration => activeDuration;
    public float VisualOffsetY => visualOffsetY;

    public int Damage => damage;
    public float DamageInterval => damageInterval;
    public Vector2 DamageSize => damageSize;
    public Vector2 DamageOffset => damageOffset;
}
