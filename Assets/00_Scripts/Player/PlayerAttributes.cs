using R3;
using UnityEngine;
using VContainer;

public class PlayerAttributes : MonoBehaviour
{
    [SerializeField] private PlayerAttributeData data;

    private ReactiveProperty<int> hp;
    public ReadOnlyReactiveProperty<int> Hp => hp;

    private InputManager inputManager;

    [Inject]
    public void Construct(InputManager inputManager)
    {
        this.inputManager = inputManager;
    }

    private void Awake()
    {
        hp = new (data.Hp);
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

    public void ResetHp()
    {
        hp.Value = data.Hp;
    }
}
