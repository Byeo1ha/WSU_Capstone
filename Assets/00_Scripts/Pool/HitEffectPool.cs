using UnityEngine;
using UnityEngine.Pool;

public class HitEffectPool : MonoBehaviour
{
    [Header("풀링할 이펙트")]
    [SerializeField] private HitEffect hitEffect;

    private ObjectPool<HitEffect> pool;

    private void Awake()
    {
        pool = new ObjectPool<HitEffect>(
            CreateEffect,
            null,
            OnReleaseEffect,
            OnDestroyEffect
        );
    }

    public void Spawn(Vector3 position, bool flipX = false)
    {
        if (hitEffect == null) return;

        HitEffect effect = pool.Get();
        effect.transform.position = position;
        effect.SetDirection(flipX);
        effect.gameObject.SetActive(true);
    }

    private HitEffect CreateEffect()
    {
        HitEffect effect = Instantiate(hitEffect, transform);
        effect.gameObject.SetActive(false);
        effect.SetPool(pool);

        return effect;
    }

    private void OnReleaseEffect(HitEffect effect)
    {
        effect.gameObject.SetActive(false);
    }

    private void OnDestroyEffect(HitEffect effect)
    {
        Destroy(effect.gameObject);
    }

    private void OnDestroy()
    {
        pool?.Clear();
    }
}
