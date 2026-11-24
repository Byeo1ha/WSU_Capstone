using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
public class MidBossStaffBurstBeam : MonoBehaviour
{
    [Header("표시할 그림")]
    [SerializeField] private Sprite warningSprite;
    [SerializeField] private Sprite attackSprite;

    [Header("임시 표시 색상")]
    [SerializeField] private Color warningColor = new Color(1f, 0.15f, 0.1f, 0.35f);
    [SerializeField] private Color attackColor = new Color(1f, 0.8f, 0.2f, 1f);

    private SpriteRenderer beamRenderer;
    private Animator beamAnimator;

    private Vector3 startPosition;
    private Vector3 endPosition;
    private float beamWidth;
    private bool hasGeometry;

    private void Awake()
    {
        beamRenderer = GetComponent<SpriteRenderer>();
        beamAnimator = GetComponent<Animator>();


        beamRenderer.drawMode = SpriteDrawMode.Simple;

        Hide();
    }

    public void ShowWarning(Vector3 start, Vector3 end, float width)
    {
        startPosition = start;
        endPosition = end;
        beamWidth = width;
        hasGeometry = true;

        beamAnimator.enabled = false;

        Show(warningSprite, warningColor);
    }

    public void ShowAttack()
    {
        if (!hasGeometry || attackSprite == null)
            return;

        beamAnimator.enabled = true;
        beamAnimator.Rebind();
        beamAnimator.Update(0f);
        Show(attackSprite, attackColor);
    
        beamAnimator.SetTrigger("PlayBeam");
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

        float scaleX = length / spriteSize.x;
        float scaleY = beamWidth / spriteSize.y;

        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        transform.localScale = new Vector3(scaleX, scaleY, 1f);

        Vector3 scaledCenter = new Vector3(sprite.bounds.center.x * scaleX, sprite.bounds.center.y * scaleY, 0f);
        Vector3 midpoint = (startPosition + endPosition) * 0.5f;
        transform.position = midpoint - transform.rotation * scaledCenter;
        
        beamRenderer.sprite = sprite;
        beamRenderer.color = color;
        beamRenderer.enabled = true;
    }

    public void Hide()
    {
        hasGeometry = false;

        if (beamAnimator != null)
            beamAnimator.enabled = false;

        if (beamRenderer != null)
            beamRenderer.enabled = false;
    }

    private void OnDisable()
    {
        Hide();
    }
}
