using UnityEngine;

public class PlayerAttributes : MonoBehaviour
{
    [SerializeField] private PlayerAttributeData data;

    private int hp;
    public int Hp => hp;

    private void Awake()
    {
        hp = data.Hp;
    }

    public void Damaged(int damage)
    {
        hp -= damage;
    }
}
