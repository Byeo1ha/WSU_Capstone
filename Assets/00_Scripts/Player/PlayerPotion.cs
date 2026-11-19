using R3;
using UnityEngine;
using VContainer;

public class PlayerPotion : MonoBehaviour
{
    [SerializeField] private int healHp = 20;
    [SerializeField] private float cooldown = 1f;
    [SerializeField] private Transform healEFX;

    private PlayerAttributes playerAttributes;
    private InputManager inputManager;
    private float cool;

    [Inject]
    public void Construct(
        InputManager inputManager,
        PlayerAttributes playerAttributes)
    {
        this.inputManager = inputManager;
        this.playerAttributes = playerAttributes;
    }

    private void Awake()
    {
        healEFX.gameObject.SetActive(false);
    }

    private void Start()
    {
        inputManager.OnPotion
            .Subscribe(_ => Potion())
            .AddTo(this);
    }

    private void FixedUpdate()
    {
        if (cool <= 0) return;

        cool -= Time.fixedDeltaTime;
    }

    private void Potion()
    {
        if (cool > 0) return;

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

        cool = cooldown;

        playerAttributes.Healed(healHp);
        playerAttributes.DecreasePotionCount();
        Debug.Log("포션 먹음");

        if (healEFX == null) return;

        healEFX.position = transform.position;

        healEFX.gameObject.SetActive(true);
    }
}
