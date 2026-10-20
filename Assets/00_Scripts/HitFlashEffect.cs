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

    // 외부에서 대상에게 섬광 이펙트를 재생하는 함수
    public void PlayHitFlash()
    {
        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);

        flashCoroutine = StartCoroutine(FlashRoutine());
    }

    // 일정 시간 동안 섬광을 적용한 뒤 해제하는 코루틴
    private IEnumerator FlashRoutine()
    {
        SetFlashAmount(0.5f);

        yield return new WaitForSeconds(flashDuration);

        SetFlashAmount(0f);
        flashCoroutine = null;
    }

    //스프라이트를 번쩍이게 하는 강도를 설정하는 함수
    private void SetFlashAmount(float value)
    {
        spriteRenderer.GetPropertyBlock(propertyBlock);

        propertyBlock.SetFloat(FlashAmountId, value);

        spriteRenderer.SetPropertyBlock(propertyBlock);
    }
}
