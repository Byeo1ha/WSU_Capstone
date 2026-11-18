using UnityEngine;

[CreateAssetMenu(fileName = "PlayerAttributeData", menuName = "Ria/PlayerAttributeData")]
public class PlayerAttributeData : ScriptableObject
{
    [SerializeField] private int hp = 100;
    [SerializeField] private int potionCount = 3;

    public int Hp => hp;
    public int PotionCount => potionCount;
}
