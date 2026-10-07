using UnityEngine;

public class EnemyTest : MonoBehaviour, IDamageable
{
    public void TakeDamage(int damage)
    {
        Debug.Log($"{damage} 만큼의 피해를 입었습니다.");
    }
}
