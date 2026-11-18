using R3;
using UnityEngine;
using VContainer;

public class PlayerPotion : MonoBehaviour
{
    [SerializeField] private int healHp = 20;

    private PlayerAttributes playerAttributes;
    private InputManager inputManager;

    [Inject]
    public void Construct(
        InputManager inputManager,
        PlayerAttributes playerAttributes)
    {
        this.inputManager = inputManager;
        this.playerAttributes = playerAttributes;
    }

    private void Start()
    {
        inputManager.OnPotion
            .Subscribe(_ => Potion())
            .AddTo(this);
    }

    private void Potion()
    {
        if (!isActiveAndEnabled) return;
        if (inputManager.IsGameplayInputBlocked) return;
        if (playerAttributes.Hp.CurrentValue <= 0) return;
        if (healHp <= 0) return;

        if (playerAttributes.Hp.CurrentValue >= playerAttributes.MaxHp)
        {
            Debug.Log("체력이 풀피얌");
            return;
        }

        if (playerAttributes.PotionCount.CurrentValue <= 0)
        {
            Debug.Log("포션이 없엉");
            return;
        }

        playerAttributes.Healed(healHp);
        playerAttributes.DecreasePotionCount();
        Debug.Log("포션 먹음");
    }
}
