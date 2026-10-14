using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class HitFlashEffect : MonoBehaviour
{
    [SerializeField] private float flashDuration = 0.2f;

    private SpriteRenderer spriteRenderer;

    private static readonly int FlashAmountId =
        Shader.PropertyToID("_FlashAmount");

    private MaterialPropertyBlock propertyBlock;
    private Coroutine flashCoroutine;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        propertyBlock = new MaterialPropertyBlock();
    }

    public void PlayHitFlash()
    {
        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);

        flashCoroutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        SetFlashAmount(0.7f);

        yield return new WaitForSeconds(flashDuration);

        SetFlashAmount(0f);
        flashCoroutine = null;
    }

    private void SetFlashAmount(float value)
    {
        spriteRenderer.GetPropertyBlock(propertyBlock);

        propertyBlock.SetFloat(FlashAmountId, value);

        spriteRenderer.SetPropertyBlock(propertyBlock);
    }
}
