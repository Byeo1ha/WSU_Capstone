using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BossLineWarning : MonoBehaviour
{
    private SpriteRenderer warningRenderer;

    private void Awake()
    {
        warningRenderer = GetComponent<SpriteRenderer>();
        Hide();
    }

    public void Show(float startX, float endX, float centerY, float height)
    {
        float length = Mathf.Abs(endX - startX);

        if (warningRenderer.sprite == null || length <= 0.01f || height <= 0f)
        {
            Hide();
            return;
        }

        Vector3 position = transform.position;
        position.x = (startX + endX) * 0.5f;
        position.y = centerY;
        transform.position = position;

        warningRenderer.size = new Vector2(length, height);
        warningRenderer.enabled = true;
    }

    public void Hide()
    {
        warningRenderer.enabled = false;
    }
}
