using R3;
using UnityEngine;
using VContainer;

public class PlayerAttributes : MonoBehaviour
{
    [SerializeField] private PlayerAttributeData data;

    private ReactiveProperty<int> hp;
    public ReadOnlyReactiveProperty<int> Hp => hp;
    public int MaxHp => data.Hp;

    private ReactiveProperty<int> potionCount;
    public ReadOnlyReactiveProperty<int> PotionCount => potionCount;

    private InputManager inputManager;

    [Inject]
    public void Construct(InputManager inputManager)
    {
        this.inputManager = inputManager;
    }

    private void Awake()
    {
        hp = new (data.Hp);
        potionCount = new (data.PotionCount);
    }

    public void Damaged(int damage)
    {
        if (hp.Value <= damage) 
        {
            hp.Value = 0;
            inputManager.SetGameplayInputBlocked(true);
            Debug.Log("고든 램쥐");
        }
        else 
        {
            hp.Value -= damage;
            Debug.Log("일반 뎀지");
        }
    }

    public void Healed(int heal)
    {
        if (hp.Value + heal > data.Hp)
        {
            hp.Value = data.Hp;
            Debug.Log($"풀피 || 피 : {hp.Value}");
            return;
        }

        hp.Value += heal;
        Debug.Log($"회복 완 || 피 : {hp.Value}");
    }

    public void DecreasePotionCount()
    {
        potionCount.Value = Mathf.Max(0, potionCount.Value - 1);
    }

    public void ResetHp()
    {
        hp.Value = data.Hp;
    }

    public void ResetPotion()
    {
        potionCount.Value = data.PotionCount;
    }
}
