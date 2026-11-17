using R3;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class UIPotion : MonoBehaviour
{
    [SerializeField] private Image[] potionImage;
    [SerializeField] private Sprite potionSprite;
    [SerializeField] private Sprite emptyPotionSprite;

    private PlayerAttributes playerAttributes;

    [Inject]
    public void Construct(PlayerAttributes playerAttributes)
    {
        this.playerAttributes = playerAttributes;
    }

    private void Start()
    {
        if (potionImage.Length < playerAttributes.PotionCount.CurrentValue)
        {
            Debug.LogWarning("포션 개수 표시 이미지보다 포션 개수가 더 많음.");
            return;
        }

        playerAttributes.PotionCount
            .Subscribe(PotionUIUpdate)
            .AddTo(this);
    }

    private void PotionUIUpdate(int count)
    {
        for (int i = 0; i < potionImage.Length; i++)
        {
            potionImage[i].sprite = i < count ? potionSprite : emptyPotionSprite;
        }
    }
}
