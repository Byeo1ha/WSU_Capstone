using UnityEngine;

[CreateAssetMenu( fileName = "MidBossBombFieldData", 
        menuName = "Boss/Mid Boss Bomb Field Data")]
public class MidBossBombFieldData : ScriptableObject
{
    [Header("구체 성장")]
    [SerializeField, Min(0.01f)] private float growthDuration = 1f;
    [Header("구체 비행")]
    [SerializeField, Min(0.01f)] private float projectileSpeed = 2f;

    [Header("충돌 후 폭발")]
    [SerializeField, Min(0f)] private float explossionDelay = 0.3f;
    [SerializeField, Min(0.01f)] private float explosionRadiusMultiplier = 1.5f;

    [Header("후속 바닥 화염")]
    [SerializeField, Min(0f)] private float afterExplosionDely = 1f;
    [SerializeField, Min(0.01f)] private float fieldDuration = 2f;
    [SerializeField, Min(0.01f)] private float fieldDamageHeight = 0.5f;

    [Header("후폭풍 경고")]
    [SerializeField, Min(0f)] private float fieldWarningDuration = 1f;

    [Header("후폭풍 좌우 지형 탐색")]
    [SerializeField, Min(0.1f)] private float fieldSearchDistance = 50f; 

    [Header("대상당 1회 피해")]
    [SerializeField, Min(0)] private int projectileDamage = 4;
    [SerializeField, Min(0)] private int explosionDamage = 4;
    [SerializeField, Min(0)] private int fieldDamage = 4;

    public float GrowthDuration => growthDuration;
    public float ProjectileSpeed => projectileSpeed;

    public float ExplosionDelay => explossionDelay;
    public float ExplosionRadiusMultiplier => explosionRadiusMultiplier;

    public float AfterExplosionDelay => afterExplosionDely;
    public float FieldDuration => fieldDuration;
    public float FieldDamageHeight => fieldDamageHeight;
    public float FieldWarningDuration => fieldWarningDuration;
    public float FieldSearchDistance => fieldSearchDistance;

    public int ProjectileDamage => projectileDamage;
    public int ExplosionDamage => explosionDamage;
    public int FieldDamage => fieldDamage; 
}
