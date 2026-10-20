using UnityEngine;

public class PlayerAttributes : MonoBehaviour, IDamageable
{
    [SerializeField] private PlayerAttributeData data;

    private int hp;

    private void Awake()
    {

        hp = data.Hp;
    }

    // 외부에서 플레이어에게 피해를 입히기 위한 함수
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
