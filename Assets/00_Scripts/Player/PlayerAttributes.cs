using R3;
using UnityEngine;

public class PlayerAttributes : MonoBehaviour
{
    [SerializeField] private PlayerAttributeData data;

    public ReactiveProperty<int> hp;

    private void Awake()
    {
        hp = new (data.Hp);
    }

    public void Damaged(int damage)
    {
        hp.Value -= damage;
    }
}
