using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class CutsceneBackgroundFade : MonoBehaviour
{
    [SerializeField] private SpriteRenderer currentBackground;
    [SerializeField] private SpriteRenderer nextBackground;

    private void Awake()
    {
        nextBackground.color = new Color(1f, 1f, 1f, 0f);
    }

    public async UniTask CrossFadeAsync(Sprite nextSprite)
    {
        nextBackground.sprite = nextSprite;
        nextBackground.color = new Color(1f, 1f, 1f, 0f);

        // 기존 배경 위에 새 배경을 겹쳐서 전환합니다.
        await nextBackground.DOFade(1f, 1f)
            .SetLink(gameObject).AsyncWaitForCompletion();

        currentBackground.sprite = nextSprite;
        nextBackground.color = new Color(1f, 1f, 1f, 0f);
    }
}
