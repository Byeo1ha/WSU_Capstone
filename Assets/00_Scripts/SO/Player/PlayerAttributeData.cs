using UnityEngine;

[CreateAssetMenu(fileName = "PlayerAttributeData", menuName = "Scriptable Objects/PlayerAttributeData")]
public class PlayerAttributeData : ScriptableObject
{
    [SerializeField] private int hp;
    public int Hp => hp;
}
