using UnityEngine;

[CreateAssetMenu(
    fileName = "FinalBossComboData",
    menuName = "KYH/Boss/Final Boss Combo Data"
)]

public class FinalBossComboData : ScriptableObject
{
    [Header("피해")]
    [SerializeField, Min(0)] private int slashDamage = 8;
    [SerializeField, Min(0)] private int pistolDamage = 12;

    [Header("칼 공격 범위 - 임시값")]
    [SerializeField] private Vector2 slashSize = new Vector2 (2f, 1.5f);
    [SerializeField] private Vector2 slashOffset = new Vector2(1f, 0f);

    [Header("칼 공격 시간 - 임시값")]
    [SerializeField, Min(0.01f)] private float slashDuration = 0.1f;
    [SerializeField, Min(0f)] private float betweenSlashDelay = 0.25f;
    [SerializeField, Min(0f)] private float beforePistolDelay = 0.2f;

    public int SlashCount => 3;
    public int SlashDamage => slashDamage;
    public int PistolDamage => pistolDamage;

    public Vector2 SlashSize => slashSize;
    public Vector2 SlashOffset => slashOffset;

    public float SlashDuration => slashDuration;
    public float BetweenSlashDelay => betweenSlashDelay;
    public float BeforePistolDelay => beforePistolDelay;

}
