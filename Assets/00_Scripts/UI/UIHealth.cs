using R3;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class UIHealth : MonoBehaviour
{
    [SerializeField] private Image filledHp;

    private PlayerAttributes playerAttributes;

    [Inject]
    public void Construct(PlayerAttributes playerAttributes)
    {
        this.playerAttributes = playerAttributes;
    }

    private void Start()
    {
        playerAttributes.hp
            .Subscribe(SetHpGauge)
            .AddTo(this);
    }

    private void SetHpGauge(int hp)
    {
        filledHp.fillAmount = (float)hp / 100;
    }
}
