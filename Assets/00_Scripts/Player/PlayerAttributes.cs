using UnityEngine;

public class PlayerAttributes : MonoBehaviour, IDamageable
{
    [SerializeField] private PlayerAttributeData data;

    private int hp;

    private void Awake()
    {
        hp = data.Hp;
    }

    public void TakeDamage(int damage)
    {
        if (hp < damage) 
        {
            hp = 0;
            return;
        }

        hp -= damage;
        Debug.Log($"{damage} 피해를 입었습니다! 남은 체력 {hp}");
    }
}
