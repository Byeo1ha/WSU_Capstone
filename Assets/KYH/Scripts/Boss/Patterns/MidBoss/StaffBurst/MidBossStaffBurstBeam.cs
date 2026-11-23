using System;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class MidBossStaffBurstBeam : MonoBehaviour
{
    [Header("표시할 그림")]
    [SerializeField] private Sprite warningSprite;
    [SerializeField] private Sprite attackSprite;

    [Header("임시 표시 색상")]
    [SerializeField] private Color warningColor = new Color(1f, 0.15f, 0.1f, 0.35f);
    [SerializeField] private Color attackColor = new Color(1f, 0.8f, 0.2f, 1f);

    private SpriteRenderer beamRenderer;

    private Vector3 startPosition;
    private Vector3 endPosition;
    private float beamWidth;
    private bool hasGeometry;

    private void Awake()
    {
        beamRenderer = GetComponent<SpriteRenderer>();
        beamRenderer.drawMode = SpriteDrawMode.Simple;

        Hide();
    }

    public void ShowWarning(Vector3 start, Vector3 end, float width)
    {
        startPosition = start;
        endPosition = end;
        beamWidth = width;
        hasGeometry = true;

        Show(warningSprite, warningColor);
    }

    public void ShowAttack()
    {
        if (!hasGeometry)
            return;

        Show(attackSprite, attackColor);
    }

    private void Show(Sprite sprite, Color color)
    {
        Vector2 direction = endPosition - startPosition;
        float length = direction.magnitude;

        if (sprite == null || length <= 0.001f || beamWidth <= 0f)
        {
            Hide();
            return;
        }

        Vector2 spriteSize = sprite.bounds.size;

        if (spriteSize.x <= 0f || spriteSize.y <= 0f)
        {
            Hide();
            return;
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        transform.position = (startPosition + endPosition) * 0.5f;

        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        transform.localScale = new Vector3(length / spriteSize.x,beamWidth / spriteSize.y, 1f);

        beamRenderer.sprite = sprite;
        beamRenderer.color = color;
        beamRenderer.enabled = true;
    }

    public void Hide()
    {
        hasGeometry = false;

        if (beamRenderer != null)
            beamRenderer.enabled = false;
    }

    private void OnDisable()
    {
        Hide();
    }
}
