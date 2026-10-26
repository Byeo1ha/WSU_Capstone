using UnityEngine;

[CreateAssetMenu(fileName = "BossData", menuName = "KYH/Boss/Boss Data")]
public class BossData : ScriptableObject
{
    [Header ("체력 설정")]
    [SerializeField, Min(1)] private int maxHealth = 600;

    public int MaxHealth => maxHealth;
}