using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BombFieldWarning : MonoBehaviour
{
    private SpriteRenderer warningRenderer;

    private void Awake()
    {
        warningRenderer = GetComponent<SpriteRenderer>();
        Hide();
    }

    public void Show(Vector3 position, float radius)
    {
        if (warningRenderer.sprite == null || radius <= 0f)
        {
            Hide();
            return;
        }

        Vector2 spriteSize = warningRenderer.sprite.bounds.size;

        if (spriteSize.x <= 0f || spriteSize.y <= 0f)
        {
            Hide();
            return;
        }

        float diameter = radius * 2f;

        float scale = diameter /spriteSize.x;

        transform.position = position;
        transform.localScale = new Vector3(scale, scale, 1f);

        warningRenderer.enabled = true;
    }

    public void Hide()
    {
        if (warningRenderer != null)
            warningRenderer.enabled = false;
    }
}
